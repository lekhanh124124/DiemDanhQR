// File: Repositories/Interfaces/IUserRepository.cs
using DiemDanhQR_API.Models;

namespace DiemDanhQR_API.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> ExistsByMaNguoiDungAsync(string maNguoiDung);
        Task<bool> ExistsByTenDangNhapAsync(string tenDangNhap);
        Task<PhanQuyen?> GetRoleAsync(int maQuyen);
        Task AddAsync(NguoiDung entity);
        Task SaveChangesAsync();

        Task<NguoiDung?> GetByMaNguoiDungAsync(string maNguoiDung);
        Task<NguoiDung?> GetByTenDangNhapAsync(string tenDangNhap);
        Task<SinhVien?> GetStudentByMaNguoiDungAsync(string maNguoiDung);
        Task<GiangVien?> GetLecturerByMaNguoiDungAsync(string maNguoiDung);

        Task<(List<(LichSuHoatDong Log, NguoiDung User)> Items, int Total)> SearchActivitiesAsync(
            string? keyword,
            string? maNguoiDung,
            DateTime? from,
            DateTime? to,
            string? sortBy,
            bool desc,
            int page,
            int pageSize);
    }
}
