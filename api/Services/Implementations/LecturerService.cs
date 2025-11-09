// File: Services/Implementations/LecturerService.cs
using api.DTOs;
using api.ErrorHandling;
using api.Helpers;
using api.Models;
using api.Repositories.Interfaces;
using api.Services.Interfaces;

namespace api.Services.Implementations
{
    public class LecturerService : ILecturerService
    {
        private readonly ILecturerRepository _repo;
        private readonly IPermissionRepository _permissionRepo;
        private readonly IAcademicRepository _academicRepo;
        private readonly IWebHostEnvironment _env;

        public LecturerService(
            ILecturerRepository repo,
            IPermissionRepository permissionRepo,
            IAcademicRepository academicRepo,
            IWebHostEnvironment env)
        {
            _repo = repo;
            _permissionRepo = permissionRepo;
            _academicRepo = academicRepo;
            _env = env;
        }

        private string inputResponse(string input)
        {
            return input ?? "null";
        }

        public async Task<CreateLecturerResponse> CreateAsync(CreateLecturerRequest request)
        {
            var maGV = request.MaGiangVien?.Trim();
            if (string.IsNullOrWhiteSpace(maGV))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã giảng viên không hợp lệ.");

            if (!request.MaKhoa.HasValue || request.MaKhoa <= 0)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu hoặc sai mã khoa.");

            // Khoa phải tồn tại
            var khoa = await _academicRepo.GetKhoaByIdAsync(request.MaKhoa!.Value);
            if (khoa == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Khoa không tồn tại.");
            // Lấy role mặc định cho giảng viên theo CodeQuyen = "GV"
            var role = await _permissionRepo.GetRoleByCodeAsync("GV");
            if (role == null)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Không tìm thấy quyền giảng viên (GV).");

            // Avatar
            string? avatarUrl = null;
            if (request.AnhDaiDien != null)
            {
                avatarUrl = await AvatarHelper.SaveAvatarAsync(request.AnhDaiDien, _env.WebRootPath ?? "", maGV!);
            }

            // User: TenDangNhap = MaGiangVien
            var user = await _repo.GetUserByUsernameAsync(maGV!);
            if (user == null)
            {
                user = new NguoiDung
                {
                    TenDangNhap = maGV!,
                    HoTen = string.IsNullOrWhiteSpace(request.HoTen) ? maGV! : request.HoTen!.Trim(),
                    MatKhau = BCrypt.Net.BCrypt.HashPassword(maGV),
                    TrangThai = true,
                    GioiTinh = request.GioiTinh,
                    AnhDaiDien = avatarUrl,
                    Email = request.Email,
                    SoDienThoai = request.SoDienThoai,
                    NgaySinh = request.NgaySinh,
                    DiaChi = request.DiaChi,
                    MaQuyen = role.MaQuyen
                };
                await _repo.AddUserAsync(user);
                await _repo.SaveChangesAsync();
            }
            else
            {
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Người dùng đã tồn tại.");
            }

            // Không trùng mã giảng viên
            if (await _repo.ExistsLecturerAsync(maGV!))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã giảng viên đã tồn tại.");

            var gv = new GiangVien
            {
                MaNguoiDung = user.MaNguoiDung,
                MaGiangVien = maGV!,
                MaKhoa = request.MaKhoa,
                HocHam = request.HocHam,
                HocVi = request.HocVi,
                NgayTuyenDung = request.NgayTuyenDung
            };
            await _repo.AddLecturerAsync(gv);

            await _repo.AddActivityAsync(new LichSuHoatDong
            {
                MaNguoiDung = user.MaNguoiDung,
                HanhDong = $"Tạo giảng viên [{gv.MaGiangVien}]",
                ThoiGian = TimeHelper.UtcToVietnam(DateTime.UtcNow)
            });
            await _repo.SaveChangesAsync();
            
            var department = await _academicRepo.GetKhoaByIdAsync((int)gv.MaKhoa);
            return new CreateLecturerResponse
            {
                // NguoiDung: xuất theo comment
                NguoiDung = new NguoiDungDTO
                {
                    HoTen = inputResponse(user.HoTen),
                    TenDangNhap = inputResponse(user.TenDangNhap),
                    TrangThai = inputResponse(user.TrangThai.ToString().ToLowerInvariant()),
                    GioiTinh = inputResponse(user.GioiTinh?.ToString()!),
                    AnhDaiDien = inputResponse(user.AnhDaiDien!),
                    Email = inputResponse(user.Email!),
                    SoDienThoai = inputResponse(user.SoDienThoai!),
                    NgaySinh = inputResponse(user.NgaySinh?.ToString("dd-MM-yyyy")!),
                    DiaChi = inputResponse(user.DiaChi!)
                },
                GiangVien = new GiangVienDTO
                {
                    MaGiangVien = inputResponse(gv.MaGiangVien),
                    HocHam = inputResponse(gv.HocHam!),
                    HocVi = inputResponse(gv.HocVi!),
                    NgayTuyenDung = inputResponse(gv.NgayTuyenDung?.ToString("yyyy-MM-dd")!)
                },
                Khoa = new KhoaDTO
                {
                    MaKhoa = inputResponse(department!.MaKhoa.ToString()),
                    TenKhoa = inputResponse(department.TenKhoa),
                    CodeKhoa = inputResponse(department.CodeKhoa)
                },
                PhanQuyen = new PhanQuyenDTO
                {
                    MaQuyen = inputResponse(role!.MaQuyen.ToString()),
                    CodeQuyen = inputResponse(role.CodeQuyen),
                    TenQuyen = inputResponse(role.TenQuyen)
                }
            };
        }

        public async Task<PagedResult<LecturerListItemResponse>> GetListAsync(GetLecturersRequest request)
        {
            var page = Math.Max(request.Page ?? 1, 1);
            var pageSize = Math.Min(Math.Max(request.PageSize ?? 20, 1), 200);
            var sortBy = request.SortBy ?? "HoTen";
            var desc = (request.SortDir ?? "ASC").Trim().ToUpperInvariant() == "DESC";

            var (items, total) = await _repo.SearchLecturersAsync(
                maGiangVien: request.MaGiangVien,
                hoTen: request.HoTen,
                maKhoa: request.MaKhoa,
                hocHam: request.HocHam,
                hocVi: request.HocVi,
                ngayTuyenDungFrom: request.NgayTuyenDungFrom,
                ngayTuyenDungTo: request.NgayTuyenDungTo,
                trangThaiUser: request.TrangThaiUser,
                sortBy: sortBy,
                desc: desc,
                page: page,
                pageSize: pageSize
            );

            var list = new List<LecturerListItemResponse>();
            foreach (var x in items)
            {
                var kh = await _academicRepo.GetKhoaByIdAsync((int)x.Gv.MaKhoa!);

                list.Add(new LecturerListItemResponse
                {
                    NguoiDung = new NguoiDungDTO
                    {
                        HoTen = inputResponse(x.Nd.HoTen),
                        TenDangNhap = inputResponse(x.Nd.TenDangNhap),
                        TrangThai = inputResponse(x.Nd.TrangThai.ToString().ToLowerInvariant())
                    },
                    GiangVien = new GiangVienDTO
                    {
                        MaGiangVien = inputResponse(x.Gv.MaGiangVien),
                        HocHam = inputResponse(x.Gv.HocHam!),
                        HocVi = inputResponse(x.Gv.HocVi!),
                        NgayTuyenDung = inputResponse(x.Gv.NgayTuyenDung?.ToString("dd-MM-yyyy")!)
                    },
                    Khoa = new KhoaDTO
                    {
                        MaKhoa = inputResponse(kh!.MaKhoa.ToString()),
                        TenKhoa = inputResponse(kh!.TenKhoa),
                        CodeKhoa = inputResponse(kh.CodeKhoa)
                    },
                });
            }
            ;


            return new PagedResult<LecturerListItemResponse>
            {
                Page = inputResponse(page.ToString()),
                PageSize = inputResponse(pageSize.ToString()),
                TotalRecords = inputResponse(total.ToString()),
                TotalPages = inputResponse(((int)Math.Ceiling(total / (double)pageSize)).ToString()),
                Items = list
            };
        }

        public async Task<UpdateLecturerResponse> UpdateAsync(UpdateLecturerRequest request)
        {
            var maGV = request.MaGiangVien?.Trim();
            if (string.IsNullOrWhiteSpace(maGV))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã giảng viên không được để trống.");

            var gv = await _repo.GetLecturerByMaGiangVienAsync(maGV!)
                ?? throw ApiExceptionHelper.New(ApiErrorCode.NotFound, "Không tìm thấy hồ sơ giảng viên.");

            var user = await _repo.GetUserByIdAsync(gv.MaNguoiDung)
                ?? throw ApiExceptionHelper.New(ApiErrorCode.NotFound, "Không tìm thấy người dùng.");

            // Update User
            if (request.TrangThai.HasValue) user.TrangThai = request.TrangThai.Value;
            if (!string.IsNullOrWhiteSpace(request.TenGiangVien)) user.HoTen = request.TenGiangVien!.Trim();
            if (request.GioiTinh.HasValue) user.GioiTinh = request.GioiTinh;

            if (request.AnhDaiDien != null)
            {
                // Xóa ảnh cũ
                if (!string.IsNullOrWhiteSpace(user.AnhDaiDien))
                {
                    var webRoot = _env.WebRootPath ?? "";
                    try
                    {
                        var relative = user.AnhDaiDien!;
                        if (Uri.TryCreate(relative, UriKind.Absolute, out var uriAbs))
                            relative = uriAbs.LocalPath;

                        var physical = Path.Combine(webRoot, relative.TrimStart('/', '\\'));
                        if (File.Exists(physical)) File.Delete(physical);
                    }
                    catch { }
                }

                // Lưu ảnh mới
                var newUrl = await AvatarHelper.SaveAvatarAsync(request.AnhDaiDien, _env.WebRootPath ?? "", maGV!);
                if (!string.IsNullOrWhiteSpace(newUrl)) user.AnhDaiDien = newUrl;
            }

            if (!string.IsNullOrWhiteSpace(request.Email)) user.Email = request.Email!.Trim();
            if (!string.IsNullOrWhiteSpace(request.SoDienThoai)) user.SoDienThoai = request.SoDienThoai!.Trim();
            if (request.NgaySinh.HasValue) user.NgaySinh = request.NgaySinh.Value;
            if (!string.IsNullOrWhiteSpace(request.DiaChi)) user.DiaChi = request.DiaChi!.Trim();
            await _repo.UpdateUserAsync(user);

            // Update GiangVien
            if (request.MaKhoa.HasValue)
            {
                var k = await _academicRepo.GetKhoaByIdAsync(request.MaKhoa.Value);
                if (k == null) ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Khoa không tồn tại.");
                gv.MaKhoa = request.MaKhoa.Value;
            }
            if (!string.IsNullOrWhiteSpace(request.HocHam)) gv.HocHam = request.HocHam!.Trim();
            if (!string.IsNullOrWhiteSpace(request.HocVi)) gv.HocVi = request.HocVi!.Trim();
            if (request.NgayTuyenDung.HasValue) gv.NgayTuyenDung = request.NgayTuyenDung.Value;

            await _repo.UpdateLecturerAsync(gv);

            await _repo.AddActivityAsync(new LichSuHoatDong
            {
                MaNguoiDung = user.MaNguoiDung,
                HanhDong = $"Cập nhật thông tin giảng viên [{gv.MaGiangVien}]",
                ThoiGian = TimeHelper.UtcToVietnam(DateTime.UtcNow)
            });
            await _repo.SaveChangesAsync();

            var role = await _permissionRepo.GetRoleByIdAsync(user.MaQuyen);
            var department = await _academicRepo.GetKhoaByIdAsync(gv.MaKhoa!.Value);

            return new UpdateLecturerResponse
            {
                NguoiDung = new NguoiDungDTO
                {
                    HoTen = inputResponse(user.HoTen),
                    TenDangNhap = inputResponse(user.TenDangNhap),
                    TrangThai = inputResponse(user.TrangThai.ToString().ToLowerInvariant()),
                    GioiTinh = inputResponse(user.GioiTinh?.ToString()!),
                    AnhDaiDien = inputResponse(user.AnhDaiDien!),
                    Email = inputResponse(user.Email!),
                    SoDienThoai = inputResponse(user.SoDienThoai!),
                    NgaySinh = inputResponse(user.NgaySinh?.ToString("dd-MM-yyyy")!),
                    DiaChi = inputResponse(user.DiaChi!)
                },
                GiangVien = new GiangVienDTO
                {
                    MaGiangVien = inputResponse(gv.MaGiangVien),
                    HocHam = inputResponse(gv.HocHam!),
                    HocVi = inputResponse(gv.HocVi!),
                    NgayTuyenDung = inputResponse(gv.NgayTuyenDung?.ToString("yyyy-MM-dd")!)
                },
                Khoa = new KhoaDTO
                {
                    MaKhoa = inputResponse(department!.MaKhoa.ToString()),
                    TenKhoa = inputResponse(department.TenKhoa),
                    CodeKhoa = inputResponse(department.CodeKhoa)
                },
                PhanQuyen = new PhanQuyenDTO
                {
                    MaQuyen = inputResponse(role!.MaQuyen.ToString()),
                    CodeQuyen = inputResponse(role.CodeQuyen),
                    TenQuyen = inputResponse(role.TenQuyen)
                }
            };
        }
    }
}
