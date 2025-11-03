// File: Services/Implementations/UserService.cs
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;
using DiemDanhQR_API.Models;
using DiemDanhQR_API.Repositories.Interfaces;
using DiemDanhQR_API.Services.Interfaces;
using DiemDanhQR_API.Helpers;

namespace DiemDanhQR_API.Services.Implementations
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
            var username = HelperFunctions.NormalizeCode(request.TenDangNhap);
            if (string.IsNullOrWhiteSpace(username))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Tên đăng nhập không hợp lệ.");

            if (request.MaQuyen <= 0)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã quyền không hợp lệ.");

            // Kiểm tra quyền tồn tại
            var role = await _repo.GetRoleAsync(request.MaQuyen);
            if (role == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy quyền.");

            // Kiểm tra trùng Username (unique)
            if (await _repo.ExistsByTenDangNhapAsync(username))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Tên đăng nhập đã tồn tại.");

            // Mật khẩu khởi tạo = TenDangNhap (được băm BCrypt)
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(username);

            var entity = new NguoiDung
            {
                TenDangNhap = username,
                HoTen = username,
                MatKhau = passwordHash,
                MaQuyen = request.MaQuyen,
                TrangThai = true
            };

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();

            var data = new CreateUserResponse
            {
                MaNguoiDung = entity.MaNguoiDung?.ToString() ?? "0",
                TenDangNhap = entity.TenDangNhap!,
                HoTen = entity.HoTen!,
                MaQuyen = entity.MaQuyen!.Value,
                TrangThai = entity.TrangThai ?? true
            };

            return data;
        }

        public async Task<object> GetInfoAsync(string tenDangNhap)
        {
            var username = HelperFunctions.NormalizeCode(tenDangNhap);
            if (string.IsNullOrWhiteSpace(username))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Tên đăng nhập không hợp lệ.");

            var user = await _repo.GetByTenDangNhapAsync(username);
            if (user == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy người dùng.");

            // Ưu tiên giảng viên nếu có; nếu không thì sinh viên
            var userId = user!.MaNguoiDung ?? 0;

            var gv = await _repo.GetLecturerByMaNguoiDungAsync(userId);
            if (gv != null)
            {
                var payload = new LecturerInfoResponse
                {
                    MaNguoiDung = user.MaNguoiDung?.ToString(),
                    HoTen = user.HoTen,
                    GioiTinh = user.GioiTinh,
                    AnhDaiDien = user.AnhDaiDien,
                    Email = user.Email,
                    SoDienThoai = user.SoDienThoai,
                    NgaySinh = user.NgaySinh.HasValue
                                    ? user.NgaySinh.Value.ToString("dd-MM-yyyy")
                                    : null,
                    DanToc = user.DanToc,
                    TonGiao = user.TonGiao,
                    DiaChi = user.DiaChi,
                    TrangThai = user.TrangThai ?? true,
                    MaGiangVien = gv.MaGiangVien,
                    Khoa = gv.Khoa,
                    HocHam = gv.HocHam,
                    HocVi = gv.HocVi,
                    NgayTuyenDung = gv.NgayTuyenDung
                };

                return payload;
            }

            var sv = await _repo.GetStudentByMaNguoiDungAsync(userId);
            if (sv != null)
            {
                var payload = new StudentInfoResponse
                {
                    MaNguoiDung = user.MaNguoiDung?.ToString(),
                    HoTen = user.HoTen,
                    GioiTinh = user.GioiTinh,
                    AnhDaiDien = user.AnhDaiDien,
                    Email = user.Email,
                    SoDienThoai = user.SoDienThoai,
                    NgaySinh = user.NgaySinh.HasValue
                                    ? user.NgaySinh.Value.ToString("dd-MM-yyyy")
                                    : null,
                    DanToc = user.DanToc,
                    TonGiao = user.TonGiao,
                    DiaChi = user.DiaChi,
                    TrangThai = user.TrangThai ?? true,
                    MaSinhVien = sv.MaSinhVien,
                    LopHanhChinh = sv.LopHanhChinh,
                    NamNhapHoc = sv.NamNhapHoc,
                    Khoa = sv.Khoa,
                    Nganh = sv.Nganh
                };

                return payload;
            }

            ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Người dùng không có hồ sơ Sinh viên/Giảng viên.");
            throw new InvalidOperationException("Unreachable");
        }

        public async Task<PagedResult<UserActivityItem>> GetActivityAsync(UserActivityListRequest req)
        {
            var page = req.Page <= 0 ? 1 : req.Page;
            var pageSize = req.PageSize <= 0 ? 20 : Math.Min(req.PageSize, 200);
            var sortBy = req.SortBy ?? "ThoiGian";
            var sortDir = (req.SortDir ?? "DESC").Trim().ToUpperInvariant();
            var desc = sortDir == "DESC";

            var (rows, total) = await _repo.SearchActivitiesAsync(
                tenDangNhap: req.TenDangNhap,
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
                    MaLichSu = x.Log.MaLichSu,
                    ThoiGian = x.Log.ThoiGian.HasValue
                                ? HelperFunctions.UtcToVietnam(x.Log.ThoiGian.Value)
                                    .ToString("dd-MM-yyyy HH:mm:ss")
                                : null,
                    HanhDong = x.Log.HanhDong,
                    MaNguoiDung = x.User.MaNguoiDung?.ToString(),
                    TenDangNhap = x.User.TenDangNhap
                }
            ).ToList();

            return new PagedResult<UserActivityItem>
            {
                Page = page,
                PageSize = pageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            };
        }

        public async Task<UpdateUserProfileResponse> UpdateProfileAsync(
                    string maNguoiDungFromToken, UpdateUserProfileRequest req)
        {
            if (!int.TryParse(HelperFunctions.NormalizeCode(maNguoiDungFromToken), out var userId) || userId <= 0)
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Phiên không hợp lệ.");

            var user = await _repo.GetByIdAsync(userId);
            if (user == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy người dùng.");

            // --- Validate cơ bản ---
            if (!string.IsNullOrWhiteSpace(req.Email) && req.Email.Length > 100)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Email quá dài (<=100).");
            if (!string.IsNullOrWhiteSpace(req.SoDienThoai) && req.SoDienThoai.Length > 15)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Số điện thoại quá dài (<=15).");
            if (!string.IsNullOrWhiteSpace(req.AnhDaiDien) && req.AnhDaiDien.Length > 255)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Ảnh đại diện quá dài (<=255).");
            if (!string.IsNullOrWhiteSpace(req.DanToc) && req.DanToc.Length > 20)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Dân tộc quá dài (<=20).");
            if (!string.IsNullOrWhiteSpace(req.TonGiao) && req.TonGiao.Length > 20)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Tôn giáo quá dài (<=20).");
            if (!string.IsNullOrWhiteSpace(req.DiaChi) && req.DiaChi.Length > 255)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Địa chỉ quá dài (<=255).");

            // --- Cập nhật các trường được phép ---
            user!.GioiTinh = req.GioiTinh;
            user.AnhDaiDien = req.AnhDaiDien?.Trim();
            user.Email = req.Email?.Trim();
            user.SoDienThoai = req.SoDienThoai?.Trim();
            user.NgaySinh = req.NgaySinh;
            user.DanToc = req.DanToc?.Trim();
            user.TonGiao = req.TonGiao?.Trim();
            user.DiaChi = req.DiaChi?.Trim();

            await _repo.UpdateAsync(user);

            // --- Ghi lịch sử hoạt động ---
            var log = new LichSuHoatDong
            {
                MaNguoiDung = user.MaNguoiDung,
                HanhDong = "Cập nhật thông tin cá nhân",
                ThoiGian = HelperFunctions.UtcToVietnam(DateTime.UtcNow)
            };
            await _repo.AddActivityAsync(log);

            await _repo.SaveChangesAsync();

            return new UpdateUserProfileResponse
            {
                MaNguoiDung = user.MaNguoiDung?.ToString() ?? "0",
                HoTen = user.HoTen,
                GioiTinh = user.GioiTinh,
                AnhDaiDien = user.AnhDaiDien,
                Email = user.Email,
                SoDienThoai = user.SoDienThoai,
                NgaySinh = user.NgaySinh.HasValue
                                ? user.NgaySinh.Value.ToString("dd-MM-yyyy")
                                : null,
                DanToc = user.DanToc,
                TonGiao = user.TonGiao,
                DiaChi = user.DiaChi
            };
        }
    }
}
