// File: DTOs/Requests/StudentRequests.cs
using System.ComponentModel.DataAnnotations;

namespace DiemDanhQR_API.DTOs.Requests
{
    public class CreateStudentRequest
    {
        // Bắt buộc
        public string MaSinhVien { get; set; } = string.Empty;
        public int MaQuyen { get; set; }

        // Hồ sơ người dùng 
        public string? HoTen { get; set; }
        public byte? GioiTinh { get; set; }
        public IFormFile? AnhDaiDien { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? DanToc { get; set; }
        public string? TonGiao { get; set; }
        public string? DiaChi { get; set; }

        // Thông tin sinh viên (tuỳ chọn)
        public string? LopHanhChinh { get; set; }
        public int? NamNhapHoc { get; set; }
        public string? Khoa { get; set; }
        public string? Nganh { get; set; }
    }

    public class GetStudentsRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Khoa { get; set; }
        public string? Nganh { get; set; }
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
        public int? MaQuyen { get; set; }

        // Student
        public string? LopHanhChinh { get; set; }
        public int? NamNhapHoc { get; set; }
        public string? Khoa { get; set; }
        public string? Nganh { get; set; }

        // Hồ sơ cá nhân (User)
        public byte? GioiTinh { get; set; }
        public IFormFile? AnhDaiDien { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? DanToc { get; set; }
        public string? TonGiao { get; set; }
        public string? DiaChi { get; set; }
    }

        public class AddStudentToCourseRequest
    {
        [Required(ErrorMessage = "Mã lớp học phần là bắt buộc.")]
        [StringLength(20, ErrorMessage = "Mã lớp học phần tối đa 20 ký tự.")]
        [RegularExpression(@"^[A-Za-z0-9_\-]+$", ErrorMessage = "Mã lớp học phần chỉ cho phép chữ, số, gạch dưới và gạch nối.")]
        public string? MaLopHocPhan { get; set; }

        [Required(ErrorMessage = "Mã sinh viên là bắt buộc.")]
        [StringLength(30, ErrorMessage = "Mã sinh viên tối đa 30 ký tự.")]
        [RegularExpression(@"^[A-Za-z0-9_\-]+$", ErrorMessage = "Mã sinh viên chỉ cho phép chữ, số, gạch dưới và gạch nối.")]
        public string? MaSinhVien { get; set; }

        public DateTime? NgayThamGia { get; set; }
        public bool? TrangThai { get; set; } = true;
    }
    public class RemoveStudentFromCourseRequest
    {
        [Required(ErrorMessage = "Mã lớp học phần là bắt buộc.")]
        [StringLength(20, ErrorMessage = "Mã lớp học phần tối đa 20 ký tự.")]
        [RegularExpression(@"^[A-Za-z0-9_\-]+$", ErrorMessage = "Mã lớp học phần chỉ cho phép chữ, số, gạch dưới và gạch nối.")]
        public string? MaLopHocPhan { get; set; }

        [Required(ErrorMessage = "Mã sinh viên là bắt buộc.")]
        [StringLength(30, ErrorMessage = "Mã sinh viên tối đa 30 ký tự.")]
        [RegularExpression(@"^[A-Za-z0-9_\-]+$", ErrorMessage = "Mã sinh viên chỉ cho phép chữ, số, gạch dưới và gạch nối.")]
        public string? MaSinhVien { get; set; }
    }
}
