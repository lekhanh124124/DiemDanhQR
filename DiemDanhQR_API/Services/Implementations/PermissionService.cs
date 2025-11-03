// File: Services/Implementations/PermissionService.cs
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;
using DiemDanhQR_API.Helpers;
using DiemDanhQR_API.Models;
using DiemDanhQR_API.Repositories.Interfaces;
using DiemDanhQR_API.Services.Interfaces;

namespace DiemDanhQR_API.Services.Implementations
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _repo;
        public PermissionService(IPermissionRepository repo) => _repo = repo;

        public async Task<PagedResult<PermissionListItem>> GetListAsync(PermissionListRequest request)
        {
            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 200);

            var sortBy = request.SortBy ?? "MaQuyen";
            var sortDir = (request.SortDir ?? "ASC").Trim().ToUpperInvariant();
            var desc = sortDir == "DESC";

            var (items, total) = await _repo.SearchAsync(
                request.MaQuyen,
                request.CodeQuyen,
                request.TenQuyen,
                request.MoTa,
                // 🔹 truyền tham số lọc theo mã chức năng
                request.MaChucNang,
                sortBy,
                desc,
                page,
                pageSize
            );

            var list = items.Select(x => new PermissionListItem(
                x.MaQuyen ?? 0,
                x.CodeQuyen,
                x.TenQuyen,
                x.MoTa
            )).ToList();

            return new PagedResult<PermissionListItem>
            {
                Page = page,
                PageSize = pageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Items = list
            };
        }

        public async Task<PagedResult<FunctionListItem>> GetFunctionListAsync(FunctionListRequest request)
        {
            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 200);

            var sortBy = request.SortBy ?? "MaChucNang";
            var sortDir = (request.SortDir ?? "ASC").Trim().ToUpperInvariant();
            var desc = sortDir == "DESC";

            var (items, total) = await _repo.SearchFunctionsAsync(
                request.MaChucNang,
                request.CodeChucNang,
                request.TenChucNang,
                request.MoTa,
                request.TrangThai,
                request.MaQuyen,
                sortBy,
                desc,
                page,
                pageSize
            );

            var list = items.Select(x => new FunctionListItem(
                (int)x.MaChucNang!,
                x.CodeChucNang,
                x.TenChucNang,
                x.MoTa,
                x.TrangThai ?? true
            )).ToList();

            return new PagedResult<FunctionListItem>
            {
                Page = page,
                PageSize = pageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Items = list
            };
        }

        public async Task<RoleDetailResponse> CreateRoleAsync(CreateRoleRequest req, string? currentUsername)
        {
            var code = (req.CodeQuyen ?? "").Trim();
            var name = (req.TenQuyen ?? "").Trim();

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "CodeQuyen và TenQuyen là bắt buộc.");

            if (await _repo.RoleCodeExistsAsync(code))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "CodeQuyen đã tồn tại.");

            var entity = new PhanQuyen { CodeQuyen = code, TenQuyen = name, MoTa = string.IsNullOrWhiteSpace(req.MoTa) ? null : req.MoTa!.Trim() };
            await _repo.AddRoleAsync(entity);
            await _repo.LogActivityAsync(currentUsername, $"Tạo quyền: {entity.CodeQuyen} - {entity.TenQuyen}");

            return new RoleDetailResponse
            {
                MaQuyen = entity.MaQuyen ?? 0,
                CodeQuyen = entity.CodeQuyen,
                TenQuyen = entity.TenQuyen,
                MoTa = entity.MoTa
            };
        }

        public async Task<RoleDetailResponse> UpdateRoleAsync(UpdateRoleRequest req, string? currentUsername)
        {
            var id = req.MaQuyen ?? 0;
            if (id <= 0) ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "MaQuyen không hợp lệ.");

            var role = await _repo.GetRoleByIdAsync(id);
            if (role == null) ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy quyền.");

            if (!string.IsNullOrWhiteSpace(req.CodeQuyen))
            {
                var code = req.CodeQuyen!.Trim();
                if (await _repo.RoleCodeExistsAsync(code, excludeId: id))
                    ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "CodeQuyen đã tồn tại.");
                role.CodeQuyen = code;
            }
            if (!string.IsNullOrWhiteSpace(req.TenQuyen)) role.TenQuyen = req.TenQuyen!.Trim();
            if (req.MoTa != null) role.MoTa = string.IsNullOrWhiteSpace(req.MoTa) ? null : req.MoTa!.Trim();

            await _repo.UpdateRoleAsync(role);
            await _repo.LogActivityAsync(currentUsername, $"Cập nhật quyền: {role.MaQuyen} - {role.CodeQuyen}");

            return new RoleDetailResponse
            {
                MaQuyen = role.MaQuyen ?? 0,
                CodeQuyen = role.CodeQuyen,
                TenQuyen = role.TenQuyen,
                MoTa = role.MoTa
            };
        }

        public async Task<bool> DeleteRoleAsync(int maQuyen, string? currentUsername)
        {
            if (maQuyen <= 0) ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "MaQuyen không hợp lệ.");

            var role = await _repo.GetRoleByIdAsync(maQuyen);
            if (role == null) ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy quyền.");

            // Không cho xóa nếu đang được tham chiếu
            var users = await _repo.CountUsersByRoleAsync(maQuyen);
            if (users > 0)
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Quyền đang được gán cho người dùng, không thể xóa.");

            var hasMappings = await _repo.AnyRoleFunctionMappingsAsync(maQuyen);
            if (hasMappings)
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Quyền đang được gán chức năng, không thể xóa.");

            await _repo.DeleteRoleAsync(role);
            await _repo.LogActivityAsync(currentUsername, $"Xóa quyền: {role.CodeQuyen}");

            return true;
        }

        public async Task<FunctionDetailResponse> CreateFunctionAsync(CreateFunctionRequest req, string? currentUsername)
        {
            var code = (req.CodeChucNang ?? "").Trim();
            var name = (req.TenChucNang ?? "").Trim();
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "CodeChucNang và TenChucNang là bắt buộc.");

            if (await _repo.FunctionCodeExistsAsync(code))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "CodeChucNang đã tồn tại.");

            var entity = new ChucNang
            {
                CodeChucNang = code,
                TenChucNang = name,
                MoTa = string.IsNullOrWhiteSpace(req.MoTa) ? null : req.MoTa!.Trim(),
                TrangThai = req.TrangThai ?? true
            };
            await _repo.AddFunctionAsync(entity);
            await _repo.LogActivityAsync(currentUsername, $"Tạo chức năng: {entity.CodeChucNang} - {entity.TenChucNang}");

            return new FunctionDetailResponse
            {
                MaChucNang = entity.MaChucNang ?? 0,
                CodeChucNang = entity.CodeChucNang,
                TenChucNang = entity.TenChucNang,
                MoTa = entity.MoTa,
                TrangThai = entity.TrangThai ?? true
            };
        }

        public async Task<FunctionDetailResponse> UpdateFunctionAsync(UpdateFunctionRequest req, string? currentUsername)
        {
            var id = req.MaChucNang ?? 0;
            if (id <= 0) ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "MaChucNang không hợp lệ.");

            var fn = await _repo.GetFunctionByIdAsync(id);
            if (fn == null) ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy chức năng.");

            if (!string.IsNullOrWhiteSpace(req.CodeChucNang))
            {
                var code = req.CodeChucNang!.Trim();
                if (await _repo.FunctionCodeExistsAsync(code, excludeId: id))
                    ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "CodeChucNang đã tồn tại.");
                fn.CodeChucNang = code;
            }
            if (!string.IsNullOrWhiteSpace(req.TenChucNang)) fn.TenChucNang = req.TenChucNang!.Trim();
            if (req.MoTa != null) fn.MoTa = string.IsNullOrWhiteSpace(req.MoTa) ? null : req.MoTa!.Trim();
            if (req.TrangThai.HasValue) fn.TrangThai = req.TrangThai;

            await _repo.UpdateFunctionAsync(fn);
            await _repo.LogActivityAsync(currentUsername, $"Cập nhật chức năng: {fn.MaChucNang} - {fn.CodeChucNang}");

            return new FunctionDetailResponse
            {
                MaChucNang = fn.MaChucNang ?? 0,
                CodeChucNang = fn.CodeChucNang,
                TenChucNang = fn.TenChucNang,
                MoTa = fn.MoTa,
                TrangThai = fn.TrangThai ?? true
            };
        }

        public async Task<bool> DeleteFunctionAsync(int maChucNang, string? currentUsername)
        {
            if (maChucNang <= 0) ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "MaChucNang không hợp lệ.");

            var fn = await _repo.GetFunctionByIdAsync(maChucNang);
            if (fn == null) ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy chức năng.");

            var mapped = await _repo.AnyFunctionRoleMappingsAsync(maChucNang);
            if (mapped)
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Chức năng đang được gán cho quyền, không thể xóa.");

            await _repo.DeleteFunctionAsync(fn);
            await _repo.LogActivityAsync(currentUsername, $"Xóa chức năng: {fn.CodeChucNang}");

            return true;
        }

        public async Task<RoleFunctionDetailResponse> CreateRoleFunctionByCodeAsync(CreateRoleFunctionByCodeRequest req, string? currentUsername)
        {
            var codeRole = (req.CodeQuyen ?? "").Trim();
            var codeFn = (req.CodeChucNang ?? "").Trim();
            if (string.IsNullOrWhiteSpace(codeRole) || string.IsNullOrWhiteSpace(codeFn))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "CodeQuyen/CodeChucNang là bắt buộc.");

            var role = await _repo.GetRoleByCodeAsync(codeRole);
            if (role == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy quyền.");
            var fn = await _repo.GetFunctionByCodeAsync(codeFn);
            if (fn == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy chức năng.");

            if (await _repo.RoleFunctionExistsAsync(role.MaQuyen ?? 0, fn.MaChucNang ?? 0))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Cặp Quyền–Chức năng đã tồn tại.");

            await _repo.AddRoleFunctionAsync(new NhomChucNang { MaQuyen = role.MaQuyen, MaChucNang = fn.MaChucNang });
            await _repo.LogActivityAsync(currentUsername, $"Thêm nhóm chức năng: {role.CodeQuyen} - {fn.CodeChucNang}");

            return new RoleFunctionDetailResponse
            {
                MaQuyen = role.MaQuyen ?? 0,
                CodeQuyen = role.CodeQuyen,
                MaChucNang = fn.MaChucNang ?? 0,
                CodeChucNang = fn.CodeChucNang
            };
        }

        public async Task<RoleFunctionDetailResponse> UpdateRoleFunctionByCodeAsync(UpdateRoleFunctionByCodeRequest req, string? currentUsername)
        {
            var fromRole = (req.FromCodeQuyen ?? "").Trim();
            var fromFn = (req.FromCodeChucNang ?? "").Trim();
            var toRole = (req.ToCodeQuyen ?? "").Trim();
            var toFn = (req.ToCodeChucNang ?? "").Trim();

            if (string.IsNullOrWhiteSpace(fromRole) || string.IsNullOrWhiteSpace(fromFn) ||
                string.IsNullOrWhiteSpace(toRole) || string.IsNullOrWhiteSpace(toFn))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu tham số.");

            var srcRole = await _repo.GetRoleByCodeAsync(fromRole);
            if (srcRole == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy quyền nguồn.");
            var srcFn = await _repo.GetFunctionByCodeAsync(fromFn);
            if (srcFn == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy chức năng nguồn.");

            if (!await _repo.RoleFunctionExistsAsync(srcRole.MaQuyen ?? 0, srcFn.MaChucNang ?? 0))
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Nhóm chức năng nguồn không tồn tại.");

            var dstRole = await _repo.GetRoleByCodeAsync(toRole);
            if (dstRole == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy quyền đích.");
            var dstFn = await _repo.GetFunctionByCodeAsync(toFn);
            if (dstFn == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy chức năng đích.");

            if (await _repo.RoleFunctionExistsAsync(dstRole.MaQuyen ?? 0, dstFn.MaChucNang ?? 0))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Cặp Quyền–Chức năng đích đã tồn tại.");

            await _repo.DeleteRoleFunctionAsync(srcRole.MaQuyen ?? 0, srcFn.MaChucNang ?? 0);
            await _repo.AddRoleFunctionAsync(new NhomChucNang { MaQuyen = dstRole.MaQuyen, MaChucNang = dstFn.MaChucNang });

            await _repo.LogActivityAsync(currentUsername, $"Cập nhật nhóm chức năng: ({fromRole},{fromFn}) -> ({toRole},{toFn})");

            return new RoleFunctionDetailResponse
            {
                MaQuyen = dstRole.MaQuyen ?? 0,
                CodeQuyen = dstRole.CodeQuyen,
                MaChucNang = dstFn.MaChucNang ?? 0,
                CodeChucNang = dstFn.CodeChucNang
            };
        }

        public async Task<bool> DeleteRoleFunctionByCodeAsync(string codeQuyen, string codeChucNang, string? currentUsername)
        {
            var role = await _repo.GetRoleByCodeAsync((codeQuyen ?? "").Trim());
            if (role == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy quyền.");
            var fn = await _repo.GetFunctionByCodeAsync((codeChucNang ?? "").Trim());
            if (fn == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy chức năng.");

            if (!await _repo.RoleFunctionExistsAsync(role.MaQuyen ?? 0, fn.MaChucNang ?? 0))
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Nhóm chức năng không tồn tại.");

            await _repo.DeleteRoleFunctionAsync(role.MaQuyen ?? 0, fn.MaChucNang ?? 0);
            await _repo.LogActivityAsync(currentUsername, $"Xóa nhóm chức năng: {role.CodeQuyen} - {fn.CodeChucNang}");
            return true;
        }
    }
}
