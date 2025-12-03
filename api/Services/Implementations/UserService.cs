// File: Services/Implementations/UserService.cs
using api.DTOs;
using api.ErrorHandling;
using api.Helpers;
using api.Models;
using api.Repositories.Interfaces;
using api.Services.Interfaces;

namespace api.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IPermissionRepository _permRepo;
        private readonly IAcademicRepository _academicRepo;

        private readonly IWebHostEnvironment _env;

        public UserService(IUserRepository repo, IPermissionRepository permRepo, IAcademicRepository academicRepo, IWebHostEnvironment env)
        {
            _repo = repo;
            _permRepo = permRepo;
            _academicRepo = academicRepo;
            _env = env;
        }

        // Chuẩn hoá chuỗi trả về
        private string inputResponse(string input) => input ?? "null";
        private static string? FDateOnly(DateOnly? d) => d.HasValue ? d.Value.ToString("yyyy-MM-dd") : null;

        public async Task<CreateUserResponse> CreateAsync(CreateUserRequest request)
        {
            var username = request.TenDangNhap?.Trim();
            if (string.IsNullOrWhiteSpace(username))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Tên đăng nhập không hợp lệ.");

            if (!request.MaQuyen.HasValue || request.MaQuyen.Value <= 0)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã quyền không hợp lệ.");

            var role = await _repo.GetRoleAsync(request.MaQuyen!.Value);
            if (role == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy quyền.");

            if (await _repo.ExistsByTenDangNhapAsync(username!))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Tên đăng nhập đã tồn tại.");

            // Ảnh đại diện
            var avatarUrl = await AvatarHelper.SaveAvatarAsync(request.AnhDaiDien, _env.WebRootPath, username!);

            // Khởi tạo đầy đủ field của NguoiDung (theo AppDbContext/NguoiDung.cs)
            var entity = new NguoiDung
            {
                TenDangNhap = username!,
                HoTen = string.IsNullOrWhiteSpace(request.HoTen) ? username! : request.HoTen!.Trim(),
                MatKhau = BCrypt.Net.BCrypt.HashPassword(username),
                MaQuyen = request.MaQuyen.Value,
                TrangThai = true,

                GioiTinh = request.GioiTinh,
                AnhDaiDien = avatarUrl,
                Email = request.Email,
                SoDienThoai = request.SoDienThoai,
                NgaySinh = request.NgaySinh.HasValue ? DateOnly.FromDateTime(request.NgaySinh.Value) : null,
                DiaChi = request.DiaChi,

                RefreshTokenHash = null,
                RefreshTokenIssuedAt = null,
                RefreshTokenExpiresAt = null,
                RefreshTokenId = null,
                RefreshTokenRevokedAt = null
            };

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();

            return new CreateUserResponse
            {
                NguoiDung = new NguoiDungDTO
                {
                    MaNguoiDung = inputResponse(entity.MaNguoiDung.ToString()),
                    HoTen = inputResponse(entity.HoTen),
                    TenDangNhap = inputResponse(entity.TenDangNhap),
                    TrangThai = inputResponse(entity.TrangThai.ToString()),
                    GioiTinh = inputResponse(entity.GioiTinh?.ToString()!),
                    AnhDaiDien = inputResponse(entity.AnhDaiDien!),
                    Email = inputResponse(entity.Email!),
                    SoDienThoai = inputResponse(entity.SoDienThoai!),
                    NgaySinh = inputResponse(FDateOnly(entity.NgaySinh)!),
                    DiaChi = inputResponse(entity.DiaChi!)
                },
                PhanQuyen = new PhanQuyenDTO
                {
                    MaQuyen = inputResponse(role!.MaQuyen.ToString()),
                    CodeQuyen = inputResponse(role.CodeQuyen),
                    TenQuyen = inputResponse(role.TenQuyen)
                }
            };
        }

        public async Task<UpdateUserProfileResponse> UpdateProfileAsync(UpdateUserProfileRequest request, string currentUsername)
        {
            var username = request.TenDangNhap?.Trim();
            if (string.IsNullOrWhiteSpace(username))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Tên đăng nhập không hợp lệ.");

            var user = await _repo.GetByTenDangNhapAsync(username!);
            if (user == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy người dùng.");

            if (request.MaQuyen.HasValue)
            {
                var roleEntity = await _repo.GetRoleAsync(request.MaQuyen.Value);
                if (roleEntity == null)
                    ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy quyền.");
                user!.MaQuyen = request.MaQuyen.Value;
            }

            if (!string.IsNullOrWhiteSpace(request.HoTen)) user!.HoTen = request.HoTen!.Trim();
            if (request.GioiTinh.HasValue) user!.GioiTinh = request.GioiTinh.Value;
            if (!string.IsNullOrWhiteSpace(request.Email)) user!.Email = request.Email!.Trim();
            if (!string.IsNullOrWhiteSpace(request.SoDienThoai)) user!.SoDienThoai = request.SoDienThoai!.Trim();
            if (request.NgaySinh.HasValue) user!.NgaySinh = DateOnly.FromDateTime(request.NgaySinh.Value);
            if (!string.IsNullOrWhiteSpace(request.DiaChi)) user!.DiaChi = request.DiaChi!.Trim();

            if (request.TrangThai.HasValue)
                user!.TrangThai = request.TrangThai.Value;

            if (request.AnhDaiDien != null)
            {
                var newUrl = await AvatarHelper.SaveAvatarAsync(request.AnhDaiDien, _env.WebRootPath, user!.TenDangNhap);
                if (!string.IsNullOrWhiteSpace(newUrl)) user.AnhDaiDien = newUrl;
            }

            await _repo.UpdateAsync(user!);
            await _repo.AddActivityAsync(new LichSuHoatDong
            {
                MaNguoiDung = user!.MaNguoiDung,
                HanhDong = $"Cập nhật hồ sơ người dùng [{user.TenDangNhap}] bởi [{currentUsername}]",
                ThoiGian = TimeHelper.UtcToVietnam(DateTime.UtcNow)
            });
            await _repo.SaveChangesAsync();

            var role = await _repo.GetRoleAsync((int)user.MaQuyen!);

            return new UpdateUserProfileResponse
            {
                NguoiDung = new NguoiDungDTO
                {
                    MaNguoiDung = inputResponse(user.MaNguoiDung.ToString()),
                    HoTen = inputResponse(user.HoTen),
                    TenDangNhap = inputResponse(user.TenDangNhap),
                    TrangThai = inputResponse(user.TrangThai.ToString()),
                    GioiTinh = inputResponse(user.GioiTinh?.ToString()!),
                    AnhDaiDien = inputResponse(user.AnhDaiDien!),
                    Email = inputResponse(user.Email!),
                    SoDienThoai = inputResponse(user.SoDienThoai!),
                    NgaySinh = inputResponse(FDateOnly(user.NgaySinh)!),
                    DiaChi = inputResponse(user.DiaChi!)
                },
                PhanQuyen = new PhanQuyenDTO
                {
                    MaQuyen = inputResponse(role!.MaQuyen.ToString()),
                    CodeQuyen = inputResponse(role.CodeQuyen),
                    TenQuyen = inputResponse(role.TenQuyen)
                }
            };
        }

        public async Task<PagedResult<UserItem>> GetListAsync(UserListRequest request)
        {
            var page = request.page ?? 1;
            var pageSize = request.pageSize ?? 20;
            var sortBy = request.sortBy;
            var sortDir = request.sortDir;

            var tenDangNhap = request.tenDangNhap;
            var hoTen = request.hoTen;
            var maQuyen = request.maQuyen;
            var codeQuyen = request.codeQuyen;
            var trangThai = request.trangThai;
            {
                page = page <= 0 ? 1 : page;
                pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);
                var desc = string.Equals(sortDir, "DESC", StringComparison.OrdinalIgnoreCase);

                var (rows, total) = await _repo.SearchUsersAsync(
    tenDangNhap, hoTen, maQuyen, codeQuyen, trangThai, sortBy, desc, page, pageSize);

                var items = rows.Select(x => new UserItem
                {
                    NguoiDung = new NguoiDungDTO
                    {
                        MaNguoiDung = inputResponse(x.User.MaNguoiDung.ToString()),
                        HoTen = inputResponse(x.User.HoTen),
                        TenDangNhap = inputResponse(x.User.TenDangNhap),
                        TrangThai = inputResponse(x.User.TrangThai.ToString())
                    },
                    PhanQuyen = new PhanQuyenDTO
                    {
                        MaQuyen = inputResponse(x.Role != null ? x.Role.MaQuyen.ToString() : null!),
                        CodeQuyen = inputResponse(x.Role?.CodeQuyen!),
                        TenQuyen = inputResponse(x.Role?.TenQuyen!)
                    }
                }).ToList();

                return new PagedResult<UserItem>
                {
                    Page = inputResponse(page.ToString()),
                    PageSize = inputResponse(pageSize.ToString()),
                    TotalRecords = inputResponse(total.ToString()),
                    TotalPages = inputResponse(Math.Ceiling(total / (double)pageSize).ToString()),
                    Items = items
                };
            }
        }

        public async Task<object> GetInfoAsync(string tenDangNhap)
        {
            var username = tenDangNhap?.Trim();
            if (string.IsNullOrWhiteSpace(username))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Tên đăng nhập không hợp lệ.");

            var user = await _repo.GetByTenDangNhapAsync(username!);
            if (user == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy người dùng.");

            // Lấy quyền
            var pqEntity = await _permRepo.GetRoleByIdAsync((int)user!.MaQuyen!);
            if (pqEntity == null)
                ApiExceptionHelper.Throw(ApiErrorCode.InternalError, "Không tìm thấy quyền của người dùng.");

            var phanQuyenDto = new PhanQuyenDTO
            {
                MaQuyen = inputResponse(pqEntity!.MaQuyen.ToString()),
                CodeQuyen = inputResponse(pqEntity.CodeQuyen),
                TenQuyen = inputResponse(pqEntity.TenQuyen)
            };

            // DTO người dùng chung
            var nguoiDungDto = new NguoiDungDTO
            {
                MaNguoiDung = inputResponse(user.MaNguoiDung.ToString()),
                HoTen = inputResponse(user.HoTen),
                GioiTinh = inputResponse(user.GioiTinh?.ToString()!),
                AnhDaiDien = inputResponse(user.AnhDaiDien!),
                Email = inputResponse(user.Email!),
                SoDienThoai = inputResponse(user.SoDienThoai!),
                NgaySinh = inputResponse(FDateOnly(user.NgaySinh)!),
                DiaChi = inputResponse(user.DiaChi!),
                TenDangNhap = inputResponse(user.TenDangNhap),
                TrangThai = inputResponse(user.TrangThai.ToString())
            };

            // Thử load giảng viên & sinh viên
            var gv = await _repo.GetLecturerByMaNguoiDungAsync(user.MaNguoiDung);
            var sv = await _repo.GetStudentByMaNguoiDungAsync(user.MaNguoiDung);

            // ====== CASE 1: Giảng viên ======
            if (gv != null)
            {
                Khoa? kh = null;
                if (gv.MaKhoa.HasValue)
                {
                    kh = await _academicRepo.GetKhoaByIdAsync(gv.MaKhoa.Value);
                }

                return new LecturerInfoResponse
                {
                    NguoiDung = nguoiDungDto,
                    GiangVien = new GiangVienDTO
                    {
                        MaGiangVien = inputResponse(gv.MaGiangVien),
                        HocHam = inputResponse(gv.HocHam!),
                        HocVi = inputResponse(gv.HocVi!),
                        NgayTuyenDung = inputResponse(FDateOnly(gv.NgayTuyenDung)!)
                    },
                    Khoa = kh == null
                        ? new KhoaDTO()
                        : new KhoaDTO
                        {
                            MaKhoa = inputResponse(kh.MaKhoa.ToString()),
                            CodeKhoa = inputResponse(kh.CodeKhoa),
                            TenKhoa = inputResponse(kh.TenKhoa)
                        },
                    PhanQuyen = phanQuyenDto ?? null
                };
            }

            // ====== CASE 2: Sinh viên ======
            if (sv != null)
            {
                Nganh? ng = null;
                Khoa? kh = null;

                if (sv.MaNganh.HasValue)
                {
                    ng = await _academicRepo.GetNganhByIdAsync(sv.MaNganh.Value);
                    if (ng != null)
                    {
                        kh = await _academicRepo.GetKhoaByIdAsync(ng.MaKhoa);
                    }
                }

                return new StudentInfoResponse
                {
                    NguoiDung = nguoiDungDto,
                    SinhVien = new SinhVienDTO
                    {
                        MaSinhVien = inputResponse(sv.MaSinhVien),
                        NamNhapHoc = inputResponse(sv.NamNhapHoc.ToString())
                    },
                    Nganh = ng == null
                        ? new NganhDTO()
                        : new NganhDTO
                        {
                            MaNganh = inputResponse(ng.MaNganh.ToString()),
                            CodeNganh = inputResponse(ng.CodeNganh),
                            TenNganh = inputResponse(ng.TenNganh)
                        },
                    Khoa = kh == null
                        ? new KhoaDTO()
                        : new KhoaDTO
                        {
                            MaKhoa = kh == null ? null : inputResponse(kh.MaKhoa.ToString()),
                            CodeKhoa = kh == null ? null : inputResponse(kh.CodeKhoa),
                            TenKhoa = kh == null ? null : inputResponse(kh.TenKhoa)
                        },
                    PhanQuyen = phanQuyenDto ?? null
                };
            }

            // ====== CASE 3: User không phải GV cũng không phải SV (ví dụ ADMIN hệ thống) ======
            return new
            {
                NguoiDung = nguoiDungDto,
                PhanQuyen = phanQuyenDto
            };
        }

        public async Task<PagedResult<UserActivityItem>> GetActivityAsync(UserActivityListRequest req)
        {
            var page = req.Page <= 0 ? 1 : req.Page;
            var pageSize = req.PageSize <= 0 ? 20 : Math.Min(req.PageSize, 200);
            var sortBy = req.SortBy ?? "ThoiGian";
            var sortDir = (req.SortDir ?? "DESC").Trim().ToUpperInvariant();
            var desc = sortDir == "DESC";

            var (rows, total) = await _repo.SearchActivitiesAsync(
                tenDangNhap: string.IsNullOrWhiteSpace(req.TenDangNhap) ? null : req.TenDangNhap.Trim(),
                from: req.DateFrom,
                to: req.DateTo,
                sortBy: sortBy,
                desc: desc,
                page: page,
                pageSize: pageSize
            );

            var items = rows.Select(x =>
                new UserActivityItem
                {
                    NguoiDung = new NguoiDungDTO
                    {
                        MaNguoiDung = inputResponse(x.User.MaNguoiDung.ToString()),
                        TenDangNhap = inputResponse(x.User.TenDangNhap),
                        HoTen = inputResponse(x.User.HoTen)
                    },
                    LichSuHoatDong = new LichSuHoatDongDTO
                    {
                        MaLichSu = inputResponse(x.Log.MaLichSu.ToString()),
                        ThoiGian = inputResponse(TimeHelper.FormatDateTime(x.Log.ThoiGian)),
                        HanhDong = inputResponse(x.Log.HanhDong)
                    }
                }
            ).ToList();

            return new PagedResult<UserActivityItem>
            {
                Page = inputResponse(page.ToString()),
                PageSize = inputResponse(pageSize.ToString()),
                TotalRecords = inputResponse(total.ToString()),
                TotalPages = inputResponse(((int)Math.Ceiling(total / (double)pageSize)).ToString()),
                Items = items
            };
        }
    }
}
