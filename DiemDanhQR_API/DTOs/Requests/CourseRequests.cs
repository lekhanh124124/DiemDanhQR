// File: CourseRequests.cs
using System.ComponentModel.DataAnnotations;
namespace DiemDanhQR_API.DTOs.Requests
{
    public class CourseListRequest
    {
        // Paging
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Sorting
        // Allowed: MaLopHocPhan, TenLopHocPhan, TrangThai, MaMonHoc, TenMonHoc, SoTinChi, SoTiet, HocKy, MaGiangVien, TenGiangVien
        public string? SortBy { get; set; } = "MaLopHocPhan";
        public string? SortDir { get; set; } = "ASC";

        // Filters
        public string? MaLopHocPhan { get; set; }
        public string? TenLopHocPhan { get; set; }
        public bool? TrangThai { get; set; }

        public string? MaMonHoc { get; set; }
        public string? TenMonHoc { get; set; }
        public byte? SoTinChi { get; set; }
        public byte? SoTiet { get; set; }
        public byte? HocKy { get; set; }

        public string? MaGiangVien { get; set; }
        public string? TenGiangVien { get; set; }

        public string? MaSinhVien { get; set; }
        public string? Keyword { get; set; }
    }

    public class SubjectListRequest
    {
        // Paging
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Sorting
        public string? SortBy { get; set; } = "MaMonHoc";
        public string? SortDir { get; set; } = "ASC";

        // Filters
        public string? Keyword { get; set; }
        public string? MaMonHoc { get; set; }
        public string? TenMonHoc { get; set; }
        public byte? SoTinChi { get; set; }
        public byte? SoTiet { get; set; }
        public byte? HocKy { get; set; }
        public bool? TrangThai { get; set; }
    }

    public class CreateSubjectRequest
    {
        [Required(ErrorMessage = "Mã môn học là bắt buộc.")]
        [StringLength(20, ErrorMessage = "Mã môn học tối đa 20 ký tự.")]
        [RegularExpression(@"^[A-Za-z0-9_\-]+$", ErrorMessage = "Mã môn học chỉ cho phép chữ, số, gạch dưới và gạch nối.")]
        public string? MaMonHoc { get; set; }

        [Required(ErrorMessage = "Tên môn học là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên môn học tối đa 100 ký tự.")]
        public string? TenMonHoc { get; set; }

        [Required(ErrorMessage = "Số tín chỉ là bắt buộc.")]
        [Range(1, 20, ErrorMessage = "Số tín chỉ phải trong khoảng 1–20.")]
        public byte? SoTinChi { get; set; }

        [Required(ErrorMessage = "Số tiết là bắt buộc.")]
        [Range(1, 300, ErrorMessage = "Số tiết phải trong khoảng 1–300.")]
        public byte? SoTiet { get; set; }

        [Range(1, 12, ErrorMessage = "Học kỳ (nếu có) phải trong khoảng 1–12.")]
        public byte? HocKy { get; set; }

        [StringLength(200, ErrorMessage = "Mô tả tối đa 200 ký tự.")]
        public string? MoTa { get; set; }

        public bool? TrangThai { get; set; } = true;
    }

    public class CreateCourseRequest
    {
        [Required(ErrorMessage = "Mã lớp học phần là bắt buộc.")]
        [StringLength(20, ErrorMessage = "Mã lớp học phần tối đa 20 ký tự.")]
        [RegularExpression(@"^[A-Za-z0-9_\-]+$", ErrorMessage = "Mã lớp học phần chỉ cho phép chữ, số, gạch dưới và gạch nối.")]
        public string? MaLopHocPhan { get; set; }

        [Required(ErrorMessage = "Tên lớp học phần là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên lớp học phần tối đa 100 ký tự.")]
        public string? TenLopHocPhan { get; set; }

        [Required(ErrorMessage = "Mã môn học là bắt buộc.")]
        [StringLength(20, ErrorMessage = "Mã môn học tối đa 20 ký tự.")]
        public string? MaMonHoc { get; set; }

        [Required(ErrorMessage = "Mã giảng viên là bắt buộc.")]
        [StringLength(20, ErrorMessage = "Mã giảng viên tối đa 20 ký tự.")]
        public string? MaGiangVien { get; set; }

        public bool? TrangThai { get; set; } = true;
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

        // optional: nếu không gửi thì mặc định là thời điểm hiện tại
        public DateTime? NgayThamGia { get; set; }

        // optional: mặc định tham gia = true
        public bool? TrangThai { get; set; } = true;
    }
}
