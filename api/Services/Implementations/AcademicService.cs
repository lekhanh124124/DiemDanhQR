// File: Services/Implementations/AcademicService.cs
using api.DTOs;
using api.ErrorHandling;
using api.Models;
using api.Repositories.Interfaces;
using api.Services.Interfaces;

namespace api.Services.Implementations
{
    public class AcademicService : IAcademicService
    {
        private readonly IAcademicRepository _repo;
        public AcademicService(IAcademicRepository repo) => _repo = repo;

        private string inputResponse(string input) => input ?? "null";

        // ===== KHOA =====
        public async Task<PagedResult<KhoaDetailResponse>> GetKhoaListAsync(KhoaListRequest request)
        {
            var page = request.Page.GetValueOrDefault(1);
            var pageSize = request.PageSize.GetValueOrDefault(20);
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

            var sortBy = request.SortBy ?? "MaKhoa";
            var sortDir = (request.SortDir ?? "ASC").Trim().ToUpperInvariant();
            var desc = sortDir == "DESC";

            var (items, total) = await _repo.SearchKhoaAsync(
                request.MaKhoa, request.CodeKhoa, request.TenKhoa,
                sortBy, desc, page, pageSize
            );

            var list = items.Select(k => new KhoaDetailResponse
            {
                Khoa = new KhoaDTO
                {
                    MaKhoa = inputResponse(k.MaKhoa.ToString()),
                    CodeKhoa = inputResponse(k.CodeKhoa),
                    TenKhoa = inputResponse(k.TenKhoa)
                }
            }).ToList();

            return new PagedResult<KhoaDetailResponse>
            {
                Page = inputResponse(page.ToString()),
                PageSize = inputResponse(pageSize.ToString()),
                TotalRecords = inputResponse(total.ToString()),
                TotalPages = inputResponse(((int)Math.Ceiling(total / (double)pageSize)).ToString()),
                Items = list
            };
        }

        public async Task<KhoaDetailResponse> CreateKhoaAsync(CreateKhoaRequest request)
        {
            var code = (request.CodeKhoa ?? "").Trim();
            var name = (request.TenKhoa ?? "").Trim();

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "CodeKhoa và TenKhoa là bắt buộc.");

            if (code.Length > 20) ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "CodeKhoa tối đa 20 ký tự.");
            if (name.Length > 100) ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "TenKhoa tối đa 100 ký tự.");

            if (await _repo.KhoaCodeExistsAsync(code))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "CodeKhoa đã tồn tại.");

            var entity = new Khoa { CodeKhoa = code, TenKhoa = name };
            await _repo.AddKhoaAsync(entity);

            return new KhoaDetailResponse
            {
                Khoa = new KhoaDTO
                {
                    MaKhoa = inputResponse(entity.MaKhoa.ToString()),
                    CodeKhoa = inputResponse(entity.CodeKhoa),
                    TenKhoa = inputResponse(entity.TenKhoa)
                }
            };
        }

        public async Task<KhoaDetailResponse> UpdateKhoaAsync(UpdateKhoaRequest request)
        {
            var id = request.MaKhoa.GetValueOrDefault();
            if (id <= 0) ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "MaKhoa không hợp lệ.");

            var entity = await _repo.GetKhoaByIdAsync(id);
            if (entity == null) ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy Khoa.");

            if (!string.IsNullOrWhiteSpace(request.CodeKhoa))
            {
                var newCode = request.CodeKhoa!.Trim();
                if (newCode.Length > 20) ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "CodeKhoa tối đa 20 ký tự.");
                if (await _repo.KhoaCodeExistsAsync(newCode, excludeId: id))
                    ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "CodeKhoa đã tồn tại.");
                entity.CodeKhoa = newCode;
            }
            if (!string.IsNullOrWhiteSpace(request.TenKhoa))
            {
                var newName = request.TenKhoa!.Trim();
                if (newName.Length > 100) ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "TenKhoa tối đa 100 ký tự.");
                entity.TenKhoa = newName;
            }

            await _repo.UpdateKhoaAsync(entity);

            return new KhoaDetailResponse
            {
                Khoa = new KhoaDTO
                {
                    MaKhoa = inputResponse(entity.MaKhoa.ToString()),
                    CodeKhoa = inputResponse(entity.CodeKhoa),
                    TenKhoa = inputResponse(entity.TenKhoa)
                }
            };
        }

        public async Task<bool> DeleteKhoaAsync(int maKhoa)
        {
            if (maKhoa <= 0) ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "MaKhoa không hợp lệ.");

            var entity = await _repo.GetKhoaByIdAsync(maKhoa);
            if (entity == null) ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy Khoa.");

            // Không cho xoá nếu còn ngành thuộc khoa (theo FK trong AppDbContext)
            if (await _repo.AnyNganhInKhoaAsync(maKhoa))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Khoa đang có Ngành trực thuộc, không thể xoá.");

            await _repo.DeleteKhoaAsync(entity);
            return true;
        }

        // ===== NGÀNH =====
        public async Task<PagedResult<NganhDetailResponse>> GetNganhListAsync(NganhListRequest request)
        {
            var page = request.Page.GetValueOrDefault(1);
            var pageSize = request.PageSize.GetValueOrDefault(20);
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

            var sortBy = request.SortBy ?? "MaNganh";
            var sortDir = (request.SortDir ?? "ASC").Trim().ToUpperInvariant();
            var desc = sortDir == "DESC";

            var (items, total) = await _repo.SearchNganhAsync(
                request.MaNganh, request.CodeNganh, request.TenNganh, request.MaKhoa,
                sortBy, desc, page, pageSize
            );

            var list = items.Select(n => new NganhDetailResponse
            {
                Nganh = new NganhDTO
                {
                    MaNganh = inputResponse(n.MaNganh.ToString()),
                    CodeNganh = inputResponse(n.CodeNganh),
                    TenNganh = inputResponse(n.TenNganh)
                },
                // Optionally include Khoa info (not bắt buộc)
                Khoa = new KhoaDTO
                {
                    MaKhoa = inputResponse(n.MaKhoa.ToString())
                }
            }).ToList();

            return new PagedResult<NganhDetailResponse>
            {
                Page = inputResponse(page.ToString()),
                PageSize = inputResponse(pageSize.ToString()),
                TotalRecords = inputResponse(total.ToString()),
                TotalPages = inputResponse(((int)Math.Ceiling(total / (double)pageSize)).ToString()),
                Items = list
            };
        }

        public async Task<NganhDetailResponse> CreateNganhAsync(CreateNganhRequest request)
        {
            var code = (request.CodeNganh ?? "").Trim();
            var name = (request.TenNganh ?? "").Trim();
            var maKhoa = request.MaKhoa.GetValueOrDefault();

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "CodeNganh và TenNganh là bắt buộc.");
            if (maKhoa <= 0) ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "MaKhoa không hợp lệ.");

            if (code.Length > 20) ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "CodeNganh tối đa 20 ký tự.");
            if (name.Length > 100) ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "TenNganh tối đa 100 ký tự.");

            if (!await _repo.KhoaExistsAsync(maKhoa))
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy Khoa.");

            if (await _repo.NganhCodeExistsAsync(code))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "CodeNganh đã tồn tại.");

            var entity = new Nganh { CodeNganh = code, TenNganh = name, MaKhoa = maKhoa };
            await _repo.AddNganhAsync(entity);

            return new NganhDetailResponse
            {
                Nganh = new NganhDTO
                {
                    MaNganh = inputResponse(entity.MaNganh.ToString()),
                    CodeNganh = inputResponse(entity.CodeNganh),
                    TenNganh = inputResponse(entity.TenNganh)
                },
                Khoa = new KhoaDTO
                {
                    MaKhoa = inputResponse(entity.MaKhoa.ToString())
                }
            };
        }

        public async Task<NganhDetailResponse> UpdateNganhAsync(UpdateNganhRequest request)
        {
            var id = request.MaNganh.GetValueOrDefault();
            if (id <= 0) ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "MaNganh không hợp lệ.");

            var entity = await _repo.GetNganhByIdAsync(id);
            if (entity == null) ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy Ngành.");

            if (!string.IsNullOrWhiteSpace(request.CodeNganh))
            {
                var newCode = request.CodeNganh!.Trim();
                if (newCode.Length > 20) ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "CodeNganh tối đa 20 ký tự.");
                if (await _repo.NganhCodeExistsAsync(newCode, excludeId: id))
                    ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "CodeNganh đã tồn tại.");
                entity.CodeNganh = newCode;
            }
            if (!string.IsNullOrWhiteSpace(request.TenNganh))
            {
                var newName = request.TenNganh!.Trim();
                if (newName.Length > 100) ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "TenNganh tối đa 100 ký tự.");
                entity.TenNganh = newName;
            }
            if (request.MaKhoa.HasValue)
            {
                var newMaKhoa = request.MaKhoa.Value;
                if (newMaKhoa <= 0) ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "MaKhoa không hợp lệ.");
                if (!await _repo.KhoaExistsAsync(newMaKhoa))
                    ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy Khoa.");
                entity.MaKhoa = newMaKhoa;
            }

            await _repo.UpdateNganhAsync(entity);

            return new NganhDetailResponse
            {
                Nganh = new NganhDTO
                {
                    MaNganh = inputResponse(entity.MaNganh.ToString()),
                    CodeNganh = inputResponse(entity.CodeNganh),
                    TenNganh = inputResponse(entity.TenNganh)
                },
                Khoa = new KhoaDTO
                {
                    MaKhoa = inputResponse(entity.MaKhoa.ToString())
                }
            };
        }

        public async Task<bool> DeleteNganhAsync(int maNganh)
        {
            if (maNganh <= 0) ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "MaNganh không hợp lệ.");

            var entity = await _repo.GetNganhByIdAsync(maNganh);
            if (entity == null) ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy Ngành.");

            await _repo.DeleteNganhAsync(entity);
            return true;
        }
    }
}
