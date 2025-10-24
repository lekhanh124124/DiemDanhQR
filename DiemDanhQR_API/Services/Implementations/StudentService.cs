// File: Services/Implementations/StudentService.cs
using BCrypt.Net;
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;
using DiemDanhQR_API.Models;
using DiemDanhQR_API.Repositories.Interfaces;
using DiemDanhQR_API.Helpers;
using DiemDanhQR_API.Services.Interfaces;

namespace DiemDanhQR_API.Services.Implementations
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _repo;

        public StudentService(IStudentRepository repo)
        {
            _repo = repo;
        }

        public async Task<ApiResponse<CreateStudentResponse>> CreateAsync(CreateStudentRequest request)
        {
            var maSV = HelperFunctions.NormalizeCode(request.MaSinhVien);
            if (string.IsNullOrWhiteSpace(maSV))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã sinh viên không hợp lệ.");

            var maND = HelperFunctions.NormalizeCode(
                string.IsNullOrWhiteSpace(request.MaNguoiDung) ? request.MaSinhVien : request.MaNguoiDung
            );
            if (string.IsNullOrWhiteSpace(maND))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã người dùng không hợp lệ.");

            if (request.MaQuyen <= 0)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã quyền không hợp lệ.");

            // Quyền phải tồn tại
            var role = await _repo.GetRoleAsync(request.MaQuyen);
            if (role == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy quyền.");

            // Sinh viên không được trùng
            if (await _repo.ExistsStudentAsync(maSV))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã sinh viên đã tồn tại.");

            // Kiểm tra/tạo user
            var user = await _repo.GetUserByMaAsync(maND)
                       ?? await _repo.GetUserByUsernameAsync(maND); // TenDangNhap = MaND theo quy ước

            if (user == null)
            {
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(maND);
                user = new NguoiDung
                {
                    MaNguoiDung = maND,
                    TenDangNhap = maND,
                    HoTen = string.IsNullOrWhiteSpace(request.HoTen) ? maND : request.HoTen!.Trim(),
                    MatKhau = passwordHash,
                    TrangThai = true,
                    MaQuyen = request.MaQuyen,

                    Email = request.Email,
                    SoDienThoai = request.SoDienThoai,
                    NgaySinh = request.NgaySinh,
                    GioiTinh = request.GioiTinh,
                    DiaChi = request.DiaChi
                };

                await _repo.AddUserAsync(user);
            }
            else
            {
                // Có thể bổ sung cập nhật nhẹ nếu cần
                user.HoTen ??= request.HoTen ?? maND;
                user.MaQuyen ??= request.MaQuyen;
            }

            // Tạo sinh viên
            var sv = new SinhVien
            {
                MaSinhVien = maSV,
                MaNguoiDung = maND,
                LopHanhChinh = request.LopHanhChinh,
                NamNhapHoc = request.NamNhapHoc ?? DateTime.Now.Year,
                Khoa = request.Khoa,
                Nganh = request.Nganh,
            };

            await _repo.AddStudentAsync(sv);
            await _repo.SaveChangesAsync();

            var resp = new CreateStudentResponse(
                maSV,
                maND,
                user.TenDangNhap ?? maND,
                user.HoTen ?? maND,
                user.MaQuyen ?? request.MaQuyen,
                user.TrangThai ?? true,
                sv.LopHanhChinh,
                sv.NamNhapHoc,
                sv.Khoa,
                sv.Nganh
            );

            return new ApiResponse<CreateStudentResponse>
            {
                Status = 200,
                Message = "Tạo sinh viên thành công.",
                Data = resp
            };
        }
    }
}
