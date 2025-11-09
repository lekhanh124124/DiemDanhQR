// File: DTOs/Requests/AcademicRequests.cs
using System.ComponentModel.DataAnnotations;

namespace api.DTOs
{
    // ===== KHOA =====
    public class KhoaListRequest
    {
        // Filters
        public int? MaKhoa { get; set; }
        public string? CodeKhoa { get; set; }
        public string? TenKhoa { get; set; }

        // Sorting
        public string? SortBy { get; set; } = "MaKhoa"; // MaKhoa | CodeKhoa | TenKhoa
        public string? SortDir { get; set; } = "ASC";   // ASC | DESC

        // Paging
        public int? Page { get; set; } = 1;
        public int? PageSize { get; set; } = 20;
    }

    public class CreateKhoaRequest
    {
        [Required] public string? CodeKhoa { get; set; }
        [Required] public string? TenKhoa { get; set; }
    }

    public class UpdateKhoaRequest
    {
        [Required] public int? MaKhoa { get; set; }
        public string? CodeKhoa { get; set; }
        public string? TenKhoa { get; set; }
    }

    // ===== NGÀNH =====
    public class NganhListRequest
    {
        // Filters
        public int? MaNganh { get; set; }
        public string? CodeNganh { get; set; }
        public string? TenNganh { get; set; }
        public int? MaKhoa { get; set; } // FK

        // Sorting
        public string? SortBy { get; set; } = "MaNganh"; // MaNganh | CodeNganh | TenNganh | MaKhoa
        public string? SortDir { get; set; } = "ASC";    // ASC | DESC

        // Paging
        public int? Page { get; set; } = 1;
        public int? PageSize { get; set; } = 20;
    }

    public class CreateNganhRequest
    {
        [Required] public string? CodeNganh { get; set; }
        [Required] public string? TenNganh { get; set; }
        [Required] public int? MaKhoa { get; set; } // FK bắt buộc
    }

    public class UpdateNganhRequest
    {
        [Required] public int? MaNganh { get; set; }
        public string? CodeNganh { get; set; }
        public string? TenNganh { get; set; }
        public int? MaKhoa { get; set; } // có thể đổi Khoa
    }
}
