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
            var maND = HelperFunctions.NormalizeCode(request.MaNguoiDung);
            if (string.IsNullOrWhiteSpace(maND))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã người dùng không hợp lệ.");

            if (request.MaQuyen <= 0)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã quyền không hợp lệ.");

            // Kiểm tra quyền tồn tại
            var role = await _repo.GetRoleAsync(request.MaQuyen);
            if (role == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy quyền.");

            // Kiểm tra trùng PK & Username (unique)
            if (await _repo.ExistsByMaNguoiDungAsync(maND))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã người dùng đã tồn tại.");

            if (await _repo.ExistsByTenDangNhapAsync(maND)) // TenDangNhap = MaNguoiDung
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Tên đăng nhập đã tồn tại.");

            // Mật khẩu khởi tạo = MaNguoiDung (được băm BCrypt)
            var rawPassword = maND;
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

            var entity = new NguoiDung
            {
                MaNguoiDung = maND,
                TenDangNhap = maND,
                HoTen = maND,
                MatKhau = passwordHash,
                MaQuyen = request.MaQuyen,
                TrangThai = true
            };

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();

            var data = new CreateUserResponse
            {
                MaNguoiDung = entity.MaNguoiDung!,
                TenDangNhap = entity.TenDangNhap!,
                HoTen = entity.HoTen!,
                MaQuyen = entity.MaQuyen!.Value,
                TrangThai = entity.TrangThai ?? true
            };

            return data;
        }

        public async Task<object> GetInfoAsync(string maNguoiDung)
        {
            var maND = HelperFunctions.NormalizeCode(maNguoiDung);
            if (string.IsNullOrWhiteSpace(maND))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã người dùng không hợp lệ.");

            var user = await _repo.GetByMaNguoiDungAsync(maND);
            if (user == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy người dùng.");

            // Ưu tiên giảng viên nếu có; nếu không thì sinh viên
            var gv = await _repo.GetLecturerByMaNguoiDungAsync(user!.MaNguoiDung!);
            if (gv != null)
            {
                var payload = new LecturerInfoResponse
                {
                    MaNguoiDung = user.MaNguoiDung,
                    HoTen = user.HoTen,
                    GioiTinh = user.GioiTinh,
                    AnhDaiDien = user.AnhDaiDien,
                    Email = user.Email,
                    SoDienThoai = user.SoDienThoai,
                    NgaySinh    = user.NgaySinh.HasValue 
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

            var sv = await _repo.GetStudentByMaNguoiDungAsync(user.MaNguoiDung!);
            if (sv != null)
            {
                var payload = new StudentInfoResponse
                {
                    MaNguoiDung = user.MaNguoiDung,
                    HoTen = user.HoTen,
                    GioiTinh = user.GioiTinh,
                    AnhDaiDien = user.AnhDaiDien,
                    Email = user.Email,
                    SoDienThoai = user.SoDienThoai,
                    NgaySinh    = user.NgaySinh.HasValue 
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
            throw new InvalidOperationException("Unreachable"); // for compiler satisfaction
        }

        public async Task<PagedResult<UserActivityItem>> GetActivityAsync(UserActivityListRequest req)
        {
            var page = req.Page <= 0 ? 1 : req.Page;
            var pageSize = req.PageSize <= 0 ? 20 : Math.Min(req.PageSize, 200);
            var sortBy = req.SortBy ?? "ThoiGian";
            var sortDir = (req.SortDir ?? "DESC").Trim().ToUpperInvariant();
            var desc = sortDir == "DESC";

            var (rows, total) = await _repo.SearchActivitiesAsync(
                keyword: req.Keyword,
                maNguoiDung: HelperFunctions.NormalizeCode(req.MaNguoiDung),
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
                    MaNguoiDung = x.User.MaNguoiDung,
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
            var maND = HelperFunctions.NormalizeCode(maNguoiDungFromToken);
            if (string.IsNullOrWhiteSpace(maND))
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Phiên không hợp lệ.");

            var user = await _repo.GetByMaNguoiDungAsync(maND);
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
            user.NgaySinh = req.NgaySinh; // model là DATE (:contentReference[oaicite:6]{index=6})
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
                MaNguoiDung = user.MaNguoiDung!,
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
