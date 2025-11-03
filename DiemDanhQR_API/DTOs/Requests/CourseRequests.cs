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
        // Allowed: MaLopHocPhan, TenLopHocPhan, TrangThai, MaMonHoc, TenMonHoc, SoTinChi, SoTiet, MaGiangVien, TenGiangVien, NamHoc, Ky
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

        public string? MaGiangVien { get; set; }
        public string? TenGiangVien { get; set; }

        // HocKy filters
        public int? MaHocKy { get; set; }
        public short? NamHoc { get; set; }
        public byte? Ky { get; set; }

        public string? MaSinhVien { get; set; }
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
        public string? MaMonHoc { get; set; }
        public string? TenMonHoc { get; set; }
        public byte? SoTinChi { get; set; }
        public byte? SoTiet { get; set; }
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

        [Required(ErrorMessage = "Mã học kỳ là bắt buộc.")]
        public int? MaHocKy { get; set; }

        public bool? TrangThai { get; set; } = true;
    }



    public class SemesterListRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // SortBy: MaHocKy | NamHoc | Ky
        public string? SortBy { get; set; } = "MaHocKy";
        public string? SortDir { get; set; } = "ASC";

        public short? NamHoc { get; set; }
        public byte? Ky { get; set; }
    }

    public class CreateSemesterRequest
    {
        [Required(ErrorMessage = "Năm học là bắt buộc.")]
        [Range(2000, 9999, ErrorMessage = "Năm học không hợp lệ.")]
        public short? NamHoc { get; set; }

        [Required(ErrorMessage = "Kỳ là bắt buộc.")]
        [Range(1, 4, ErrorMessage = "Kỳ phải trong khoảng 1-4.")]
        public byte? Ky { get; set; }
    }

    public class UpdateSemesterRequest
    {
        [Required(ErrorMessage = "Mã học kỳ là bắt buộc.")]
        public int? MaHocKy { get; set; }

        [Range(2000, 9999, ErrorMessage = "Năm học không hợp lệ.")]
        public short? NamHoc { get; set; }

        [Range(1, 4, ErrorMessage = "Kỳ phải trong khoảng 1-4.")]
        public byte? Ky { get; set; }
    }

    public class UpdateSubjectRequest
    {
        [Required(ErrorMessage = "Mã môn học là bắt buộc.")]
        [StringLength(20, ErrorMessage = "Mã môn học tối đa 20 ký tự.")]
        public string? MaMonHoc { get; set; } // định danh

        [StringLength(100, ErrorMessage = "Tên môn học tối đa 100 ký tự.")]
        public string? TenMonHoc { get; set; }

        [Range(1, 20, ErrorMessage = "Số tín chỉ phải trong khoảng 1–20.")]
        public byte? SoTinChi { get; set; }

        [Range(1, 300, ErrorMessage = "Số tiết phải trong khoảng 1–300.")]
        public byte? SoTiet { get; set; }

        [StringLength(200, ErrorMessage = "Mô tả tối đa 200 ký tự.")]
        public string? MoTa { get; set; }

        public bool? TrangThai { get; set; }
    }

    public class UpdateCourseRequest
    {
        [Required(ErrorMessage = "Mã lớp học phần là bắt buộc.")]
        [StringLength(20, ErrorMessage = "Mã lớp học phần tối đa 20 ký tự.")]
        public string? MaLopHocPhan { get; set; } // định danh

        [StringLength(100, ErrorMessage = "Tên lớp học phần tối đa 100 ký tự.")]
        public string? TenLopHocPhan { get; set; }

        public bool? TrangThai { get; set; }

        [StringLength(20, ErrorMessage = "Mã môn học tối đa 20 ký tự.")]
        public string? MaMonHoc { get; set; }

        [StringLength(20, ErrorMessage = "Mã giảng viên tối đa 20 ký tự.")]
        public string? MaGiangVien { get; set; }

        public int? MaHocKy { get; set; }
    }
}
