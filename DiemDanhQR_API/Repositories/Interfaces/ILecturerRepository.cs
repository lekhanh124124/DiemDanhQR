// File: Repositories/Interfaces/ILecturerRepository.cs
using DiemDanhQR_API.Models;

namespace DiemDanhQR_API.Repositories.Interfaces
{
    public interface ILecturerRepository
    {
        // Lecturer
        Task<bool> ExistsLecturerAsync(string maGiangVien);
        Task AddLecturerAsync(GiangVien entity);

        // User
        Task<NguoiDung?> GetUserByMaAsync(string maNguoiDung);
        Task<NguoiDung?> GetUserByUsernameAsync(string tenDangNhap);
        Task AddUserAsync(NguoiDung user);

        // Role
        Task<PhanQuyen?> GetRoleAsync(int maQuyen);

        Task SaveChangesAsync();
    }
}
