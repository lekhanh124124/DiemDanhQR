// File: Repositories/Interfaces/IScheduleRepository.cs
using DiemDanhQR_API.Models;

namespace DiemDanhQR_API.Repositories.Interfaces
{
    public interface IScheduleRepository
    {
        Task<(List<(BuoiHoc b, PhongHoc p, LopHocPhan l, MonHoc m, GiangVien gv, NguoiDung ndGv)> Items, int Total)>
            SearchSchedulesAsync(
                string? keyword,
                int? maBuoi,
                int? maPhong,
                string? tenPhong,
                string? maLopHocPhan,
                string? tenLop,
                string? tenMonHoc,
                DateTime? ngayHoc,
                byte? tietBatDau,
                byte? soTiet,
                string? ghiChu,
                bool? trangThai,
                string? maSinhVien,   // NEW
                string? maGiangVien,  // NEW
                string? sortBy,
                bool desc,
                int page,
                int pageSize);

        Task<(List<PhongHoc> Items, int Total)> SearchRoomsAsync(
            string? keyword,
            int? maPhong,
            string? tenPhong,
            string? toaNha,
            byte? tang,        // ← byte
            byte? sucChua,     // ← byte
            bool? trangThai,
            string? sortBy,
            bool desc,
            int page,
            int pageSize);
        Task<bool> RoomNameExistsAsync(string tenPhong);
        Task AddRoomAsync(PhongHoc room);
        Task WriteActivityLogAsync(LichSuHoatDong log);
        Task<bool> CourseExistsByCodeAsync(string maLopHocPhan);
        Task<bool> RoomExistsByIdAsync(int maPhong);
        Task<bool> ScheduleExistsAsync(string maLopHocPhan, DateTime ngayHoc, byte tietBatDau);
        Task AddScheduleAsync(BuoiHoc buoi);
        Task<PhongHoc?> GetRoomByIdAsync(int maPhong);
    }
}
