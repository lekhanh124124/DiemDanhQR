// File: CourseRequests.cs
using System.ComponentModel.DataAnnotations;
namespace api.DTOs
{
    public class CourseListRequest
    {
        // Paging
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Sorting
        // Allowed: MaLopHocPhan, TenLopHocPhan, TrangThai, MaMonHoc, SoTinChi, MaGiangVien, NamHoc, Ky
        public string? SortBy { get; set; } = "MaLopHocPhan";
        public string? SortDir { get; set; } = "ASC";

        // Filters
        public string? MaLopHocPhan { get; set; }
        public string? TenLopHocPhan { get; set; }
        public bool? TrangThai { get; set; }

        public string? MaMonHoc { get; set; }
        public byte? SoTinChi { get; set; }

        public string? MaGiangVien { get; set; }

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
        public string? MaMonHoc { get; set; }

        [Required(ErrorMessage = "Tên môn học là bắt buộc.")]
        public string? TenMonHoc { get; set; }

        [Required(ErrorMessage = "Số tín chỉ là bắt buộc.")]
        public byte? SoTinChi { get; set; }

        [Required(ErrorMessage = "Số tiết là bắt buộc.")]
        public byte? SoTiet { get; set; }

        public string? MoTa { get; set; }

        public bool? TrangThai { get; set; }
    }

    public class CreateCourseRequest
    {
        [Required(ErrorMessage = "Mã lớp học phần là bắt buộc.")]
        public string? MaLopHocPhan { get; set; }

        [Required(ErrorMessage = "Tên lớp học phần là bắt buộc.")]
        public string? TenLopHocPhan { get; set; }

        [Required(ErrorMessage = "Mã môn học là bắt buộc.")]
        public string? MaMonHoc { get; set; }

        [Required(ErrorMessage = "Mã giảng viên là bắt buộc.")]
        public string? MaGiangVien { get; set; }

        [Required(ErrorMessage = "Mã học kỳ là bắt buộc.")]
        public int? MaHocKy { get; set; }

        public bool? TrangThai { get; set; }
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
        public short? NamHoc { get; set; }

        [Required(ErrorMessage = "Kỳ là bắt buộc.")]
        public byte? Ky { get; set; }
    }

    public class UpdateSemesterRequest
    {
        [Required(ErrorMessage = "Mã học kỳ là bắt buộc.")]
        public int? MaHocKy { get; set; }
        public short? NamHoc { get; set; }
        public byte? Ky { get; set; }
    }

    public class UpdateSubjectRequest
    {
        [Required(ErrorMessage = "Mã môn học là bắt buộc.")]
        public string? MaMonHoc { get; set; } // định danh
        public string? TenMonHoc { get; set; }
        public byte? SoTinChi { get; set; }
        public byte? SoTiet { get; set; }
        public string? MoTa { get; set; }
        public bool? TrangThai { get; set; }
    }

    public class UpdateCourseRequest
    {
        [Required(ErrorMessage = "Mã lớp học phần là bắt buộc.")]
        public string? MaLopHocPhan { get; set; } // định danh
        public string? TenLopHocPhan { get; set; }
        public bool? TrangThai { get; set; }
        public string? MaMonHoc { get; set; }
        public string? MaGiangVien { get; set; }
        public int? MaHocKy { get; set; }
    }
}
