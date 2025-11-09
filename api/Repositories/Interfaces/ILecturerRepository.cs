// File: Repositories/Interfaces/ILecturerRepository.cs
using api.Models;

namespace api.Repositories.Interfaces
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

        // Activity
        Task AddActivityAsync(LichSuHoatDong log);
        Task SaveChangesAsync();

        // Search list
        Task<(List<(GiangVien Gv, NguoiDung Nd)> Items, int Total)> SearchLecturersAsync(
            string? maGiangVien,
            string? hoTen,
            int? maKhoa,
            string? hocHam,
            string? hocVi,
            DateOnly? ngayTuyenDungFrom,
            DateOnly? ngayTuyenDungTo,
            bool? trangThaiUser,
            string? sortBy,
            bool desc,
            int page,
            int pageSize
        );
    }
}
