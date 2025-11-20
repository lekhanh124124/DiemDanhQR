// File: Repositories/Interfaces/IScheduleRepository.cs
using api.Models;

namespace api.Repositories.Interfaces
{
    public interface IScheduleRepository
    {
        Task<(List<(BuoiHoc b, PhongHoc? p, LopHocPhan l, MonHoc m, GiangVien? gv)> Items, int Total)>
        SearchSchedulesAsync(
            int? maBuoi,
            int? maPhong,
            string? tenPhong,
            string? maLopHocPhan,
            string? tenLopHocPhan,
            string? tenMonHoc,
            int? maHocKy,       
            DateOnly? ngayHoc,
            int? nam,
            int? tuan,
            int? thang,
            byte? tietBatDau,
            byte? soTiet,
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

        Task LogActivityAsync(string? tenDangNhap, string hanhDong);

        Task<bool> CourseExistsByCodeAsync(string maLopHocPhan);
        Task<bool> RoomExistsByIdAsync(int maPhong);

        Task<bool> ScheduleExistsAsync(string maLopHocPhan, DateOnly ngayHoc, byte tietBatDau);
        Task AddScheduleAsync(BuoiHoc buoi);

        Task<PhongHoc?> GetRoomByIdAsync(int maPhong);
        Task<PhongHoc?> GetRoomForUpdateAsync(int maPhong);
        Task<bool> RoomNameExistsExceptIdAsync(string tenPhong, int excludeMaPhong);
        Task UpdateRoomAsync(PhongHoc room);

        Task<BuoiHoc?> GetScheduleByIdAsync(int maBuoi);
        Task UpdateScheduleAsync(BuoiHoc buoi);

        Task<bool> ScheduleExistsExceptAsync(string maLopHocPhan, DateOnly ngayHoc, byte tietBatDau, int excludeMaBuoi);

        // bundle thông tin cần cho auto-generate
        Task<(LopHocPhan Lhp, MonHoc Mh, HocKy Hk, GiangVien? Gv)?> GetCourseBundleAsync(string maLopHocPhan);
        Task<List<PhongHoc>> GetActiveRoomsAsync();

        // Kiểm tra trùng lịch (room & course)
        Task<bool> AnyRoomConflictAsync(int maPhong, DateOnly ngayHoc, byte tietBatDau, byte soTiet);
        Task<bool> AnyCourseConflictAsync(string maLopHocPhan, DateOnly ngayHoc, byte tietBatDau, byte soTiet);

        // Thêm hàng loạt buổi học
        Task AddSchedulesAsync(IEnumerable<BuoiHoc> items);
        Task<NguoiDung?> GetUserByIdAsync(int maNguoiDung);

    }
}
