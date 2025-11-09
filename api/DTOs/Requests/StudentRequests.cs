// File: DTOs/Requests/StudentRequests.cs
using System.ComponentModel.DataAnnotations;

namespace api.DTOs
{
    public class CreateStudentRequest
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc.")]
        public string? MaSinhVien { get; set; }

        // Hồ sơ người dùng 
        public string? HoTen { get; set; }
        public byte? GioiTinh { get; set; }
        public IFormFile? AnhDaiDien { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? DiaChi { get; set; }

        // Thông tin sinh viên
        public int? NamNhapHoc { get; set; }
        [Required(ErrorMessage = "Mã ngành là bắt buộc.")]
        public int? MaNganh { get; set; }
    }

    public class GetStudentsRequest
    {
        public int? Page { get; set; } = 1;
        public int? PageSize { get; set; } = 20;
        public int? MaKhoa { get; set; }
        public int? MaNganh { get; set; }
        public int? NamNhapHoc { get; set; }
        public bool? TrangThaiUser { get; set; }
        public string? MaLopHocPhan { get; set; }
        public string? SortBy { get; set; } = "HoTen";
        public string? SortDir { get; set; } = "ASC";
    }

    public class UpdateStudentRequest
    {
        // Định danh theo mã sinh viên 
        public string? MaSinhVien { get; set; }

        // User
        public string? TenSinhVien { get; set; }
        public bool? TrangThai { get; set; }

        // Student
        public string? LopHanhChinh { get; set; }
        public int? NamNhapHoc { get; set; }
        public int? MaKhoa { get; set; }
        public int? MaNganh { get; set; }

        // Hồ sơ cá nhân (User)
        public byte? GioiTinh { get; set; }
        public IFormFile? AnhDaiDien { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? DiaChi { get; set; }
    }

        public class AddStudentToCourseRequest
    {
        [Required(ErrorMessage = "Mã lớp học phần là bắt buộc.")]
        public string? MaLopHocPhan { get; set; }

        [Required(ErrorMessage = "Mã sinh viên là bắt buộc.")]
        public string? MaSinhVien { get; set; }

        public DateOnly? NgayThamGia { get; set; }
        public bool? TrangThai { get; set; } = true;
    }
    public class RemoveStudentFromCourseRequest
    {
        [Required(ErrorMessage = "Mã lớp học phần là bắt buộc.")]
        public string? MaLopHocPhan { get; set; }

        [Required(ErrorMessage = "Mã sinh viên là bắt buộc.")]
        public string? MaSinhVien { get; set; }
    }
}
