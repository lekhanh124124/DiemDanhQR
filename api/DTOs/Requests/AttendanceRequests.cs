// File: DTOs/Requests/AttendanceRequests.cs
using System.ComponentModel.DataAnnotations;

namespace api.DTOs
{
    public class CreateQrRequest
    {
        [Required(ErrorMessage = "Mã buổi học phần không được để trống.")]
        public int MaBuoi { get; set; }

        [Range(1, 3600, ErrorMessage = "Thời gian tồn tại của QR phải từ 1 đến 3600 giây.")]
        public int TtlSeconds { get; set; } = 300;

        [Range(3, 20, ErrorMessage = "Pixels per module phải từ 3 đến 20.")]
        public int PixelsPerModule { get; set; } = 5;
    }

    public class CheckInRequest
    {
        [Required(ErrorMessage = "Token không được để trống.")]
        public string Token { get; set; } = string.Empty;
    }


    // Danh sách điểm danh
    public class AttendanceListRequest
    {
        // Filters (tên trùng cột)
        public int? MaDiemDanh { get; set; }
        public DateOnly? ThoiGianQuet { get; set; }
        public string? CodeTrangThai { get; set; }
        public bool? TrangThai { get; set; }
        public int? MaBuoi { get; set; }
        public string? MaSinhVien { get; set; }
        public string? MaLopHocPhan { get; set; }

        // Sorting
        public string? SortBy { get; set; } = "MaDiemDanh"; // MaDiemDanh|ThoiGianQuet|CodeTrangThai|TrangThai|MaBuoi|MaSinhVien|MaLopHocPhan
        public string? SortDir { get; set; } = "DESC";      // ASC|DESC

        // Paging
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
    public class CreateAttendanceRequest
    {
        [Required(ErrorMessage = "Mã buổi học phần không được để trống.")]
        public int MaBuoi { get; set; }
        [Required(ErrorMessage = "Mã sinh viên không được để trống.")]
        public string? MaSinhVien { get; set; }
        public string? CodeTrangThai { get; set; }
        public string? LyDo { get; set; }
        public bool? TrangThai { get; set; }
    }

    public class UpdateAttendanceRequest
    {
        [Required(ErrorMessage = "Mã điểm danh không được để trống.")]
        public int MaDiemDanh { get; set; }

        public string? CodeTrangThai { get; set; }
        public string? LyDo { get; set; }
        public bool? TrangThai { get; set; }
    }

    // TRẠNG THÁI ĐIỂM DANH
    public class AttendanceStatusListRequest
    {
        // Filters
        public int? MaTrangThai { get; set; }
        public string? TenTrangThai { get; set; }
        public string? CodeTrangThai { get; set; }

        // Sorting
        public string? SortBy { get; set; } = "MaTrangThai"; // MaTrangThai|TenTrangThai|CodeTrangThai
        public string? SortDir { get; set; } = "ASC";        // ASC|DESC

        // Paging
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
    public class CreateAttendanceStatusRequest
    {
        [Required(ErrorMessage = "Mã trạng thái không được để trống.")]
        public string? CodeTrangThai { get; set; }
        [Required(ErrorMessage = "Tên trạng thái không được để trống.")]
        public string? TenTrangThai { get; set; }
    }

    public class UpdateAttendanceStatusRequest
    {
        [Required(ErrorMessage = "Mã trạng thái không được để trống.")]
        public int MaTrangThai { get; set; }
        [Required(ErrorMessage = "Mã trạng thái không được để trống.")]
        public string? CodeTrangThai { get; set; }
        [Required(ErrorMessage = "Tên trạng thái không được để trống.")]
        public string? TenTrangThai { get; set; }
    }
}