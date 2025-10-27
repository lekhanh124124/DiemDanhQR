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

        public async Task<CreateUsertResponse> CreateAsync(CreateUserRequest request)
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

            var data = new CreateUsertResponse
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
                var payload = new LecturerInfoResponse(
                    user.MaNguoiDung ?? string.Empty,
                    user.HoTen,
                    user.GioiTinh,
                    user.AnhDaiDien,
                    user.Email,
                    user.SoDienThoai,
                    user.NgaySinh,
                    user.DanToc,
                    user.TonGiao,
                    user.DiaChi,
                    user.TrangThai ?? true,
                    gv.MaGiangVien ?? string.Empty,
                    gv.Khoa,
                    gv.HocHam,
                    gv.HocVi,
                    gv.NgayTuyenDung
                );

                return payload;
            }

            var sv = await _repo.GetStudentByMaNguoiDungAsync(user.MaNguoiDung!);
            if (sv != null)
            {
                var payload = new StudentInfoResponse(
                    user.MaNguoiDung ?? string.Empty,
                    user.HoTen,
                    user.GioiTinh,
                    user.AnhDaiDien,
                    user.Email,
                    user.SoDienThoai,
                    user.NgaySinh,
                    user.DanToc,
                    user.TonGiao,
                    user.DiaChi,
                    user.TrangThai ?? true,
                    sv.MaSinhVien ?? string.Empty,
                    sv.LopHanhChinh,
                    sv.NamNhapHoc,
                    sv.Khoa,
                    sv.Nganh
                );

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
                new UserActivityItem(
                    (int)x.Log.MaLichSu!,
                    (DateTime)x.Log.ThoiGian!,
                    x.Log.HanhDong ?? string.Empty,
                    x.User.MaNguoiDung ?? string.Empty,
                    x.User.TenDangNhap ?? string.Empty
                )
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

    }
}
