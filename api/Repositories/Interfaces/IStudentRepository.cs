// File: Repositories/Interfaces/IStudentRepository.cs
using api.Models;

namespace api.Repositories.Interfaces
{
    public interface IStudentRepository
    {
        // Student
        Task<bool> ExistsStudentAsync(string maSinhVien);
        Task AddStudentAsync(SinhVien entity);
        Task<SinhVien?> GetStudentByMaNguoiDungAsync(int maNguoiDung);
        Task<SinhVien?> GetStudentByMaSinhVienAsync(string maSinhVien);
        Task UpdateStudentAsync(SinhVien entity);

        // User
        Task<NguoiDung?> GetUserByIdAsync(int maNguoiDung);
        Task<NguoiDung?> GetUserByUsernameAsync(string tenDangNhap);
        Task AddUserAsync(NguoiDung user);
        Task UpdateUserAsync(NguoiDung user);

        Task SaveChangesAsync();

        // Tìm kiếm danh sách SV (join NguoiDung + Nganh (+Khoa qua Nganh))
        Task<(List<(SinhVien Sv, NguoiDung Nd, Nganh? Ng, Khoa? Kh)> Items, int Total)> SearchStudentsAsync(
            int? maKhoa,
            int? maNganh,
            int? namNhapHoc,
            bool? trangThaiUser,
            string? maLopHocPhan,
            string? sortBy,
            bool desc,
            int page,
            int pageSize
        );

        // Tham gia lớp
        Task<bool> CourseExistsAsync(string maLopHocPhan);
        Task<bool> ParticipationExistsAsync(string maLopHocPhan, string maSinhVien);
        Task AddParticipationAsync(ThamGiaLop thamGia);
        Task<ThamGiaLop?> GetParticipationAsync(string maLopHocPhan, string maSinhVien);
        Task UpdateParticipationAsync(ThamGiaLop thamGia);

        // Log
        Task AddActivityAsync(LichSuHoatDong log);
    }
}
