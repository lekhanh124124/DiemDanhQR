// File: DTOs/Requests/LecturerRequests.cs
using System.ComponentModel.DataAnnotations;

namespace api.DTOs
{
    public class CreateLecturerRequest
    {
        public string? HoTen { get; set; }
        [Required(ErrorMessage = "Mã khoa là bắt buộc.")]
        public int? MaKhoa { get; set; }
        public string? HocHam { get; set; }
        public string? HocVi { get; set; }
        public DateOnly? NgayTuyenDung { get; set; } // Nếu null sẽ dùng năm hiện tại (VN)

        // Tuỳ chọn hồ sơ người dùng
        public byte? GioiTinh { get; set; }
        public IFormFile? AnhDaiDien { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public DateOnly? NgaySinh { get; set; }
        public string? DiaChi { get; set; }
    }

    public class GetLecturersRequest
    {
        // Paging
        public int? Page { get; set; } = 1;
        public int? PageSize { get; set; } = 20;
        // Sorting
        public string? SortBy { get; set; } = "HoTen";
        public string? SortDir { get; set; } = "ASC"; // ASC | DESC

        // Filters
        public string? MaGiangVien { get; set; }
        public string? HoTen { get; set; }       
        public int? MaKhoa { get; set; }
        public string? HocHam { get; set; }
        public string? HocVi { get; set; }
        public DateOnly? NgayTuyenDungFrom { get; set; }
        public DateOnly? NgayTuyenDungTo { get; set; }
        public bool? TrangThaiUser { get; set; }
    }

    public class UpdateLecturerRequest
    {
        [Required(ErrorMessage = "Mã giảng viên là bắt buộc.")]
        public string? MaGiangVien { get; set; }

        // Thông tin user/giảng viên có thể cập nhật
        public string? TenGiangVien { get; set; }
        public bool? TrangThai { get; set; }

        public int? MaKhoa { get; set; }
        public string? HocHam { get; set; }
        public string? HocVi { get; set; }
        public DateOnly? NgayTuyenDung { get; set; }

        public byte? GioiTinh { get; set; }
        public IFormFile? AnhDaiDien { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public DateOnly? NgaySinh { get; set; }
        public string? DiaChi { get; set; }
    }
}
