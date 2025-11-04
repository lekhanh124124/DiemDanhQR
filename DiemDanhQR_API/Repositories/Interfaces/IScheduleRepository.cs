// File: Repositories/Interfaces/IScheduleRepository.cs
using DiemDanhQR_API.Models;

namespace DiemDanhQR_API.Repositories.Interfaces
{
    public interface IScheduleRepository
    {
        Task<(List<(BuoiHoc b, PhongHoc p, LopHocPhan l, MonHoc m, GiangVien gv, NguoiDung ndGv)> Items, int Total)>
            SearchSchedulesAsync(
                int? maBuoi,
                int? maPhong,
                string? tenPhong,
                string? maLopHocPhan,
                string? tenLop,
                string? tenMonHoc,
                DateTime? ngayHoc,
                int? nam,        // NEW
                int? tuan,
                int? thang,
                byte? tietBatDau,
                byte? soTiet,
                string? ghiChu,
                bool? trangThai,
                string? maSinhVien,
                string? maGiangVien,
                string? sortBy,
                bool desc,
                int page,
                int pageSize);

        Task<(List<PhongHoc> Items, int Total)> SearchRoomsAsync(
            int? maPhong,
            string? tenPhong,
            string? toaNha,
            byte? tang,
            byte? sucChua,
            bool? trangThai,
            string? sortBy,
            bool desc,
            int page,
            int pageSize);
        Task<bool> RoomNameExistsAsync(string tenPhong);
        Task AddRoomAsync(PhongHoc room);

        // Ghi log theo TenDangNhap -> map sang MaNguoiDung
        Task LogActivityAsync(string? tenDangNhap, string hanhDong);

        Task<bool> CourseExistsByCodeAsync(string maLopHocPhan);
        Task<bool> RoomExistsByIdAsync(int maPhong);
        Task<bool> ScheduleExistsAsync(string maLopHocPhan, DateTime ngayHoc, byte tietBatDau);
        Task AddScheduleAsync(BuoiHoc buoi);
        Task<PhongHoc?> GetRoomByIdAsync(int maPhong);

        Task<PhongHoc?> GetRoomForUpdateAsync(int maPhong);
        Task<bool> RoomNameExistsExceptIdAsync(string tenPhong, int excludeMaPhong);
        Task UpdateRoomAsync(PhongHoc room);
        Task<BuoiHoc?> GetScheduleByIdAsync(int maBuoi);
        Task UpdateScheduleAsync(BuoiHoc buoi);
        Task<bool> ScheduleExistsAsync(string maLopHocPhan, DateTime ngayHoc, byte tietBatDau, int excludeMaBuoi);
    }
}
