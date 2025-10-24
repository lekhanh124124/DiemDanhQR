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

        public async Task<ApiResponse<CreateUsertResponse>> CreateAsync(CreateUserRequest request)
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

            return new ApiResponse<CreateUsertResponse>
            {
                Status = 200,
                Message = "Tạo người dùng thành công.",
                Data = data
            };
        }
        public async Task<ApiResponse<object>> GetInfoAsync(GetUserInfoRequest request)
        {
            var maND = HelperFunctions.NormalizeCode(request.MaNguoiDung);
            var tenDN = HelperFunctions.NormalizeUsername(request.TenDangNhap);

            if (string.IsNullOrWhiteSpace(maND) && string.IsNullOrWhiteSpace(tenDN))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Cần cung cấp MaNguoiDung hoặc TenDangNhap.");

            var user = !string.IsNullOrWhiteSpace(maND)
                ? await _repo.GetByMaNguoiDungAsync(maND)
                : await _repo.GetByTenDangNhapAsync(tenDN!);

            if (user == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy người dùng.");

            // Ưu tiên giảng viên nếu tồn tại; nếu không thì sinh viên
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

                return new ApiResponse<object>
                {
                    Status = 200,
                    Message = "Lấy thông tin giảng viên thành công.",
                    Data = payload
                };
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
                return new ApiResponse<object>
                {
                    Status = 200,
                    Message = "Lấy thông tin sinh viên thành công.",
                    Data = payload
                };
            }

            ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Người dùng không có hồ sơ Sinh viên/Giảng viên.");
            throw new InvalidOperationException(); // unreachable
        }
    }
}
