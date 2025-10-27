// File: CourseRequests.cs
namespace DiemDanhQR_API.DTOs.Requests
{
    public class CourseListRequest
    {
        // Paging
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Sorting (hỗ trợ thêm MaMonHoc, MaGiangVien)
        // Allowed: MaLopHocPhan, TenLopHocPhan, TrangThai, MaMonHoc, TenMonHoc, SoTinChi, SoTiet, HocKy, MaGiangVien, TenGiangVien
        public string? SortBy { get; set; } = "MaLopHocPhan";
        public string? SortDir { get; set; } = "ASC"; // ASC | DESC

        // Filters
        public string? MaLopHocPhan { get; set; }
        public string? TenLopHocPhan { get; set; }
        public bool? TrangThai { get; set; }

        public string? MaMonHoc { get; set; }        // NEW
        public string? TenMonHoc { get; set; }
        public byte? SoTinChi { get; set; }
        public byte? SoTiet { get; set; }
        public byte? HocKy { get; set; }

        public string? MaGiangVien { get; set; }     // NEW
        public string? TenGiangVien { get; set; }

        // Keyword tổng quát
        public string? Keyword { get; set; }
    }
    public class CourseParticipantsRequest
    {
        // Paging
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Sorting
        // Allowed: MaLopHocPhan, TenLopHocPhan, MaMonHoc, TenMonHoc, MaSinhVien, TenSinhVien, NgayThamGia, TrangThaiThamGia, MaGiangVien, TenGiangVien
        public string? SortBy { get; set; } = "MaLopHocPhan";
        public string? SortDir { get; set; } = "ASC";

        // Filters
        public string? MaLopHocPhan { get; set; }
        public string? TenLopHocPhan { get; set; }

        public string? MaMonHoc { get; set; }
        public string? TenMonHoc { get; set; }
        public byte? HocKy { get; set; }

        public string? MaSinhVien { get; set; }
        public string? TenSinhVien { get; set; }
        public DateTime? NgayThamGiaFrom { get; set; }
        public DateTime? NgayThamGiaTo { get; set; }
        public bool? TrangThaiThamGia { get; set; }

        public string? MaGiangVien { get; set; }
        public string? TenGiangVien { get; set; }

        // Keyword tổng quát (OR nhiều trường)
        public string? Keyword { get; set; }
    }

    public class SubjectListRequest
    {
        // Paging
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Sorting: MaMonHoc (default), TenMonHoc, SoTinChi, SoTiet, HocKy, TrangThai
        public string? SortBy { get; set; } = "MaMonHoc";
        public string? SortDir { get; set; } = "ASC"; // ASC | DESC

        // Filters
        public string? Keyword { get; set; }      // OR theo nhiều trường
        public string? MaMonHoc { get; set; }
        public string? TenMonHoc { get; set; }
        public byte? SoTinChi { get; set; }
        public byte? SoTiet { get; set; }
        public byte? HocKy { get; set; }
        public bool? TrangThai { get; set; }
    }
}
