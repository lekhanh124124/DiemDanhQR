// File: Repositories/Interfaces/ILecturerRepository.cs
using DiemDanhQR_API.Models;

namespace DiemDanhQR_API.Repositories.Interfaces
{
    public interface ILecturerRepository
    {
        // Lecturer
        Task<bool> ExistsLecturerAsync(string maGiangVien);
        Task AddLecturerAsync(GiangVien entity);
        Task<GiangVien?> GetLecturerByMaNguoiDungAsync(int maNguoiDung);
        Task<GiangVien?> GetLecturerByMaGiangVienAsync(string maGiangVien);
        Task UpdateLecturerAsync(GiangVien entity);

        // User
        Task<NguoiDung?> GetUserByIdAsync(int maNguoiDung);
        Task<NguoiDung?> GetUserByUsernameAsync(string tenDangNhap);
        Task AddUserAsync(NguoiDung user);
        Task UpdateUserAsync(NguoiDung user);

        // Role
        Task<PhanQuyen?> GetRoleAsync(int maQuyen);

        Task AddActivityAsync(LichSuHoatDong log);
        Task SaveChangesAsync();

        Task<(List<(GiangVien Gv, NguoiDung Nd)> Items, int Total)> SearchLecturersAsync(
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
    }
}
