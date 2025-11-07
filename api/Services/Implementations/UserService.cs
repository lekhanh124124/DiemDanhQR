using api.DTOs;
using api.DTOs.Requests;
using api.DTOs.Responses;
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

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task<CreateUserResponse> CreateAsync(CreateUserRequest request)
        {
            var username = request.TenDangNhap?.Trim();
            if (string.IsNullOrWhiteSpace(username))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Tên đăng nhập không hợp lệ.");

            if (request.MaQuyen <= 0)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã quyền không hợp lệ.");

            // Kiểm tra quyền tồn tại
            var role = await _repo.GetRoleAsync((int)request.MaQuyen!);
            if (role == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy quyền.");

            // Kiểm tra trùng Username (unique)
            if (await _repo.ExistsByTenDangNhapAsync(username!))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Tên đăng nhập đã tồn tại.");

            // Mật khẩu khởi tạo = TenDangNhap (băm BCrypt)
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(username);

            var entity = new NguoiDung
            {
                TenDangNhap = username!,
                HoTen = username!,
                MatKhau = passwordHash,
                MaQuyen = (int)request.MaQuyen!,
                TrangThai = true
            };

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();

            var nguoiDungDto = new NguoiDungDTO
            {
                MaNguoiDung = entity.MaNguoiDung.ToString() ?? "null",
                HoTen = entity.HoTen ?? "null",
                GioiTinh = entity.GioiTinh?.ToString() ?? "null",
                AnhDaiDien = entity.AnhDaiDien ?? "null",
                Email = entity.Email ?? "null",
                SoDienThoai = entity.SoDienThoai ?? "null",
                NgaySinh = entity.NgaySinh?.ToString("dd-MM-yyyy") ?? "null",
                DiaChi = entity.DiaChi ?? "null",
                TenDangNhap = entity.TenDangNhap ?? "null",
                TrangThai = entity.TrangThai.ToString().ToLowerInvariant() ?? "null"
            };

            var phanQuyenDto = new PhanQuyenDTO
            {
                MaQuyen = role!.MaQuyen.ToString() ?? "null",
                CodeQuyen = role.CodeQuyen ?? "null",
                TenQuyen = role.TenQuyen ?? "null",
                MoTa = role.MoTa ?? "null"
            };

            return new CreateUserResponse
            {
                NguoiDung = nguoiDungDto,
                PhanQuyen = phanQuyenDto
            };
        }

        public async Task<object> GetInfoAsync(string tenDangNhap)
        {
            var username = tenDangNhap?.Trim();
            if (string.IsNullOrWhiteSpace(username))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Tên đăng nhập không hợp lệ.");

            var user = await _repo.GetByTenDangNhapAsync(username!);
            if (user == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy người dùng.");

            var userId = user!.MaNguoiDung;

            var gv = await _repo.GetLecturerByMaNguoiDungAsync(userId);
            var sv = gv == null ? await _repo.GetStudentByMaNguoiDungAsync(userId) : null;

            var nguoiDungDto = new NguoiDungDTO
            {
                MaNguoiDung = user.MaNguoiDung.ToString() ?? "null",
                HoTen = user.HoTen ?? "null",
                GioiTinh = user.GioiTinh?.ToString() ?? "null",
                AnhDaiDien = user.AnhDaiDien ?? "null",
                Email = user.Email ?? "null",
                SoDienThoai = user.SoDienThoai ?? "null",
                NgaySinh = user.NgaySinh?.ToString("dd-MM-yyyy") ?? "null",
                DiaChi = user.DiaChi ?? "null"
            };

            if (gv != null)
            {
                return new
                {
                    NguoiDung = nguoiDungDto,
                    GiangVien = new GiangVienDTO
                    {
                        MaGiangVien = gv.MaGiangVien ?? "null",
                        MaKhoa = gv.MaKhoa?.ToString() ?? "null",
                        HocHam = gv.HocHam ?? "null",
                        HocVi = gv.HocVi ?? "null",
                        NgayTuyenDung = gv.NgayTuyenDung?.ToString("dd-MM-yyyy") ?? "null"
                    },
                };
            }

            if (sv != null)
            {
                return new
                {
                    NguoiDung = nguoiDungDto,
                    SinhVien = new SinhVienDTO
                    {
                        MaSinhVien = sv.MaSinhVien ?? "null",
                        NamNhapHoc = sv.NamNhapHoc.ToString() ?? "null"
                    }
                };
            }
            
            return new
            {
                NguoiDung = nguoiDungDto
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
                    // NguoiDungDTO trong activity chỉ cần: MaNguoiDung, TenDangNhap, HoTen
                    NguoiDung = new NguoiDungDTO
                    {
                        MaNguoiDung = x.User.MaNguoiDung.ToString() ?? "null",
                        TenDangNhap = x.User.TenDangNhap ?? "null",
                        HoTen = x.User.HoTen ?? "null"
                    },
                    LichSuHoatDong = new LichSuHoatDongDTO
                    {
                        MaLichSu = x.Log.MaLichSu.ToString() ?? "null",
                        ThoiGian = TimeHelper.FormatDateTime(x.Log.ThoiGian) ?? "null",
                        HanhDong = x.Log.HanhDong ?? "null"
                    }
                }
            ).ToList();

            return new PagedResult<UserActivityItem>
            {
                Page = page.ToString(),
                PageSize = pageSize.ToString(),
                TotalRecords = total.ToString(),
                TotalPages = ((int)Math.Ceiling(total / (double)pageSize)).ToString(),
                Items = items
            };
        }
    }
}
