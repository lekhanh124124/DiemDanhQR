// File: Repositories/Interfaces/IUserRepository.cs
using api.Models;

namespace api.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> ExistsByTenDangNhapAsync(string tenDangNhap);
        Task<PhanQuyen?> GetRoleAsync(int maQuyen);

        Task AddAsync(NguoiDung entity);
        Task SaveChangesAsync();

        Task<NguoiDung?> GetByIdAsync(int maNguoiDung);
        Task<NguoiDung?> GetByTenDangNhapAsync(string tenDangNhap);
        Task<SinhVien?> GetStudentByMaNguoiDungAsync(int maNguoiDung);
        Task<GiangVien?> GetLecturerByMaNguoiDungAsync(int maNguoiDung);

        Task UpdateAsync(NguoiDung entity);
        Task AddActivityAsync(LichSuHoatDong log);

        Task<(List<(LichSuHoatDong Log, NguoiDung User)> Items, int Total)> SearchActivitiesAsync(
            string? tenDangNhap,
            DateTime? from,
            DateTime? to,
            string? sortBy,
            bool desc,
            int page,
            int pageSize);

        // Thêm: tìm kiếm danh sách người dùng
        Task<(List<(NguoiDung User, PhanQuyen? Role)> Items, int Total)> SearchUsersAsync(
            string? tenDangNhap,
            string? hoTen,
            int? maQuyen,
            string? codeQuyen,
            bool? trangThai,
            string? sortBy,
            bool desc,
            int page,
            int pageSize);

    }
}
