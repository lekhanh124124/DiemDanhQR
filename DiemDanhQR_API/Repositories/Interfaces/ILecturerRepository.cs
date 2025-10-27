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
        Task<(List<(GiangVien Gv, NguoiDung Nd)> Items, int Total)> SearchLecturersAsync(
            string? keyword,
            string? khoa,
            string? hocHam,
            string? hocVi,
            DateTime? ngayTuyenDungFrom,
            DateTime? ngayTuyenDungTo,
            bool? trangThaiUser,
            string? sortBy,
            bool desc,
            int page,
            int pageSize
        );

        Task<GiangVien?> GetLecturerByMaNguoiDungAsync(string maNguoiDung);
        Task UpdateLecturerAsync(GiangVien entity);
        Task<bool> ExistsUsernameForAnotherAsync(string tenDangNhap);
        Task UpdateUserAsync(NguoiDung user);
        Task AddActivityAsync(LichSuHoatDong log);
    }
}
