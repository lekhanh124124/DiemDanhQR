// File: DTOs/Requests/ScheduleRequests.cs
using System.ComponentModel.DataAnnotations;

namespace DiemDanhQR_API.DTOs.Requests
{
    public class ScheduleListRequest
    {
        // Tìm kiếm
        public int? MaBuoi { get; set; }
        public int? MaPhong { get; set; }
        public string? TenPhong { get; set; }
        public string? MaLopHocPhan { get; set; }
        public string? TenLop { get; set; }
        public string? TenMonHoc { get; set; }
        public DateTime? NgayHoc { get; set; }
        public byte? TietBatDau { get; set; }
        public byte? SoTiet { get; set; }
        public string? GhiChu { get; set; }
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
        [StringLength(100, ErrorMessage = "Tên phòng tối đa 100 ký tự.")]
        public string? TenPhong { get; set; }

        [StringLength(100, ErrorMessage = "Tòa nhà tối đa 100 ký tự.")]
        public string? ToaNha { get; set; }

        [Range(0, 100, ErrorMessage = "Tầng phải trong khoảng 0–100.")]
        public byte? Tang { get; set; }

        [Required(ErrorMessage = "Sức chứa là bắt buộc.")]
        [Range(1, 500, ErrorMessage = "Sức chứa phải trong khoảng 1–500.")]
        public byte? SucChua { get; set; }

        public bool? TrangThai { get; set; } = true;
    }
    public class CreateScheduleRequest
    {
        [Required(ErrorMessage = "Mã lớp học phần là bắt buộc.")]
        [StringLength(20, ErrorMessage = "Mã lớp học phần tối đa 20 ký tự.")]
        public string? MaLopHocPhan { get; set; }

        [Required(ErrorMessage = "Mã phòng là bắt buộc.")]
        public int? MaPhong { get; set; }

        [Required(ErrorMessage = "Ngày học là bắt buộc.")]
        public DateTime? NgayHoc { get; set; }   // chỉ lấy phần Date

        [Required(ErrorMessage = "Tiết bắt đầu là bắt buộc.")]
        [Range(1, 20, ErrorMessage = "Tiết bắt đầu phải trong khoảng 1–20.")]
        public byte? TietBatDau { get; set; }

        [Required(ErrorMessage = "Số tiết là bắt buộc.")]
        [Range(1, 20, ErrorMessage = "Số tiết phải trong khoảng 1–20.")]
        public byte? SoTiet { get; set; }

        [StringLength(200, ErrorMessage = "Ghi chú tối đa 200 ký tự.")]
        public string? GhiChu { get; set; }

        public bool? TrangThai { get; set; } = true;
    }
}
