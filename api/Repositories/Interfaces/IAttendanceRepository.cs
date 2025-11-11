// File: Repositories/Interfaces/IAttendanceRepository.cs
using api.Models;

namespace api.Repositories.Interfaces
{
    public interface IAttendanceRepository
    {
        Task<BuoiHoc?> GetActiveBuoiByIdAsync(int maBuoi);
        Task<BuoiHoc?> GetBuoiByIdAsync(int maBuoi);
        Task<LopHocPhan?> GetLopHocPhanByIdAsync(string maLopHocPhan);

        Task<bool> SaveChangesAsync();

        Task<bool> IsLopHocPhanActiveAsync(string maLopHocPhan);

        Task<NguoiDung?> GetNguoiDungByUsernameAsync(string username);
        Task<SinhVien?> GetSinhVienByMaNguoiDungAsync(int maNguoiDung);

        Task<bool> IsSinhVienInActiveLopAsync(string maLopHocPhan, string maSinhVien);

        Task<bool> AttendanceExistsAsync(int maBuoi, string maSinhVien);
        Task<DiemDanh> CreateAttendanceAsync(DiemDanh entity);
        Task<DiemDanh?> GetAttendanceByIdAsync(int id);
        Task UpdateAttendanceAsync(DiemDanh entity);

        Task<int?> TryGetTrangThaiIdByCodeAsync(string code);
        Task<TrangThaiDiemDanh?> GetStatusByIdAsync(int id);
        Task<bool> StatusCodeExistsAsync(string code, int? excludeId = null);
        Task<bool> IsStatusInUseAsync(int id);
        Task<TrangThaiDiemDanh> CreateStatusAsync(TrangThaiDiemDanh entity);
        Task UpdateStatusAsync(TrangThaiDiemDanh entity);
        Task<bool> DeleteStatusAsync(int id);

        Task LogHistoryAsync(int? maNguoiDung, string action);

        Task<(List<TrangThaiDiemDanh> Items, int Total)> SearchStatusesAsync(
            int? maTrangThai,
            string? tenTrangThai,
            string? codeTrangThai,
            string? sortBy,
            bool desc,
            int page,
            int pageSize);

        // Trả về đầy đủ để build AttendanceListItem (DiemDanh + TrangThai + BuoiHoc + SinhVien + LopHocPhan)
        Task<(List<(DiemDanh d, TrangThaiDiemDanh? t, BuoiHoc b, SinhVien s, LopHocPhan lhp)> Items, int Total)> SearchAttendancesAsync(
            int? maDiemDanh,
            DateOnly? thoiGianQuetDateOnly,
            int? maTrangThai,
            bool? trangThai,
            int? maBuoi,
            string? maSinhVien,
            string? maLopHocPhan,
            string? sortBy,
            bool desc,
            int page,
            int pageSize);
    }
}
