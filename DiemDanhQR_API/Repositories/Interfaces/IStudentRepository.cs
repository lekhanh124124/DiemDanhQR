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
        Task<NguoiDung?> GetUserByMaAsync(string maNguoiDung);
        Task<NguoiDung?> GetUserByUsernameAsync(string tenDangNhap);
        Task AddUserAsync(NguoiDung user);

        // Role
        Task<PhanQuyen?> GetRoleAsync(int maQuyen);

        Task SaveChangesAsync();

        Task<(List<(SinhVien Sv, NguoiDung Nd)> Items, int Total)> SearchStudentsAsync(
            string? keyword,
            string? khoa,
            string? nganh,
            int? namNhapHoc,
            bool? trangThaiUser,
            string? sortBy,
            bool desc,
            int page,
            int pageSize
        );
    }
}
