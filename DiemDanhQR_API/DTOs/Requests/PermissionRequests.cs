// File: DTOs/Requests/PermissionRequests.cs
namespace DiemDanhQR_API.DTOs.Requests
{
    public class PermissionListRequest
    {
        // Lọc
        public string? Keyword { get; set; }
        public int? MaQuyen { get; set; }
        public string? CodeQuyen { get; set; }
        public string? TenQuyen { get; set; }
        public string? MoTa { get; set; }

        // Sắp xếp
        public string? SortBy { get; set; }   // MaQuyen | CodeQuyen | TenQuyen | MoTa
        public string? SortDir { get; set; }  // ASC | DESC

        // Phân trang
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class FunctionListRequest
    {
        // Lọc
        public string? Keyword { get; set; }
        public int? MaChucNang { get; set; }
        public string? CodeChucNang { get; set; }
        public string? TenChucNang { get; set; }
        public string? MoTa { get; set; }
        public bool? TrangThai { get; set; }

        // NEW: lọc theo phân quyền (role)
        public int? MaQuyen { get; set; }

        // Sắp xếp
        // MaChucNang | CodeChucNang | TenChucNang | MoTa | TrangThai
        public string? SortBy { get; set; }
        public string? SortDir { get; set; } // ASC | DESC

        // Phân trang
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
