// File: DTOs/Requests/ScheduleRequests.cs
using System.ComponentModel.DataAnnotations;

namespace api.DTOs
{
    public class ScheduleListRequest
    {
        // Tìm kiếm
        public int? MaBuoi { get; set; }
        public int? MaPhong { get; set; }
        public string? TenPhong { get; set; }
        public string? MaLopHocPhan { get; set; }
        public string? TenLopHocPhan { get; set; }
        public string? TenMonHoc { get; set; }

        // mới thêm
        public int? MaHocKy { get; set; }

        public DateOnly? NgayHoc { get; set; }
        public int? Nam { get; set; }
        public int? Tuan { get; set; }
        public int? Thang { get; set; }
        public byte? TietBatDau { get; set; }
        public byte? SoTiet { get; set; }
        public bool? TrangThai { get; set; }

        public string? MaSinhVien { get; set; }
        public string? MaGiangVien { get; set; }

        // Sắp xếp & phân trang
        public string? SortBy { get; set; }
        public string? SortDir { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }


    public class RoomListRequest
    {
        // Paging
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Sorting
        public string? SortBy { get; set; } = "MaPhong";
        public string? SortDir { get; set; } = "ASC"; // ASC | DESC

        // Filters
        public int? MaPhong { get; set; }
        public string? TenPhong { get; set; }
        public string? ToaNha { get; set; }
        public byte? Tang { get; set; }
        public byte? SucChua { get; set; }
        public bool? TrangThai { get; set; }
    }
    public class CreateRoomRequest
    {
        [Required(ErrorMessage = "Tên phòng là bắt buộc.")]
        public string? TenPhong { get; set; }
        public string? ToaNha { get; set; }
        public byte? Tang { get; set; }
        [Required(ErrorMessage = "Sức chứa là bắt buộc.")]
        public byte? SucChua { get; set; }
        public bool? TrangThai { get; set; } = true;
    }
    public class CreateScheduleRequest
    {
        [Required(ErrorMessage = "Mã lớp học phần là bắt buộc.")]
        public string? MaLopHocPhan { get; set; }
        [Required(ErrorMessage = "Mã phòng là bắt buộc.")]
        public int? MaPhong { get; set; }
        [Required(ErrorMessage = "Ngày học là bắt buộc.")]
        public DateOnly? NgayHoc { get; set; }
        [Required(ErrorMessage = "Tiết bắt đầu là bắt buộc.")]
        public byte? TietBatDau { get; set; }
        [Required(ErrorMessage = "Số tiết là bắt buộc.")]
        public byte? SoTiet { get; set; }
        public string? GhiChu { get; set; }
        public bool? TrangThai { get; set; } = true;
    }
    public class UpdateRoomRequest
    {
        [Required(ErrorMessage = "Mã phòng là bắt buộc.")]
        public int? MaPhong { get; set; }
        public string? TenPhong { get; set; }
        public string? ToaNha { get; set; }
        public byte? Tang { get; set; }
        public byte? SucChua { get; set; }
        public bool? TrangThai { get; set; }
    }

    public class UpdateScheduleRequest
    {
        [Required(ErrorMessage = "Mã buổi học là bắt buộc.")]
        public int? MaBuoi { get; set; }
        public int? MaPhong { get; set; }
        public DateOnly? NgayHoc { get; set; }
        public byte? TietBatDau { get; set; }
        public byte? SoTiet { get; set; }
        public string? GhiChu { get; set; }
        public bool? TrangThai { get; set; }
    }
    public class AutoGenerateScheduleRequest
    {
        [Required(ErrorMessage = "Mã lớp học phần là bắt buộc.")]
        public string? MaLopHocPhan { get; set; }
    }
}
