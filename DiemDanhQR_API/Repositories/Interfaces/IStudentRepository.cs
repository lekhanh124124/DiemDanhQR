// File: Repositories/Interfaces/IStudentRepository.cs
using DiemDanhQR_API.Models;

namespace DiemDanhQR_API.Repositories.Interfaces
{
    public interface IStudentRepository
    {
        // Student
        Task<bool> ExistsStudentAsync(string maSinhVien);
        Task AddStudentAsync(SinhVien entity);

        // User
        Task<NguoiDung?> GetUserByIdAsync(int maNguoiDung);
        Task<NguoiDung?> GetUserByUsernameAsync(string tenDangNhap);
        Task AddUserAsync(NguoiDung user);
        Task UpdateUserAsync(NguoiDung user);

        // Role
        Task<PhanQuyen?> GetRoleAsync(int maQuyen);

        Task SaveChangesAsync();

        Task<(List<(SinhVien Sv, NguoiDung Nd)> Items, int Total)> SearchStudentsAsync(
            string? khoa,
            string? nganh,
            int? namNhapHoc,
            bool? trangThaiUser,
            string? maLopHocPhan,
            string? sortBy,
            bool desc,
            int page,
            int pageSize
        );

        Task<SinhVien?> GetStudentByMaNguoiDungAsync(int maNguoiDung);
        Task<SinhVien?> GetStudentByMaSinhVienAsync(string maSinhVien);
        Task UpdateStudentAsync(SinhVien entity);

        // Activity
        Task AddActivityAsync(LichSuHoatDong log);

        // Moved from CourseRepository for adding student to course
        Task<bool> CourseExistsAsync(string maLopHocPhan);
        Task<bool> ParticipationExistsAsync(string maLopHocPhan, string maSinhVien);
        Task AddParticipationAsync(ThamGiaLop thamGia);

        // New for remove
        Task<ThamGiaLop?> GetParticipationAsync(string maLopHocPhan, string maSinhVien);
        Task UpdateParticipationAsync(ThamGiaLop thamGia);
    }
}
