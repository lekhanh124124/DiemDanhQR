// File: DTOs/Requests/PermissionRequests.cs
using System.ComponentModel.DataAnnotations;

namespace DiemDanhQR_API.DTOs.Requests
{
    public class PermissionListRequest
    {
        // Lọc
        // public string? Keyword { get; set; } // removed
        public int? MaQuyen { get; set; }
        public string? CodeQuyen { get; set; }
        public string? TenQuyen { get; set; }
        public string? MoTa { get; set; }
        public int? MaChucNang { get; set; }

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
        // public string? Keyword { get; set; } // removed
        public int? MaChucNang { get; set; }
        public string? CodeChucNang { get; set; }
        public string? TenChucNang { get; set; }
        public string? MoTa { get; set; }
        public bool? TrangThai { get; set; }
        public int? MaQuyen { get; set; }

        // Sắp xếp
        public string? SortBy { get; set; }
        public string? SortDir { get; set; } 

        // Phân trang
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // ===== Role (PhanQuyen) =====
    public class CreateRoleRequest
    {
        [Required, StringLength(50)]
        public string? CodeQuyen { get; set; }

        [Required, StringLength(50)]
        public string? TenQuyen { get; set; }

        [StringLength(200)]
        public string? MoTa { get; set; }
    }

    public class UpdateRoleRequest
    {
        [Required]
        public int? MaQuyen { get; set; }

        [StringLength(50)]
        public string? CodeQuyen { get; set; }

        [StringLength(50)]
        public string? TenQuyen { get; set; }

        [StringLength(200)]
        public string? MoTa { get; set; }
    }

    // ===== Function (ChucNang) =====
    public class CreateFunctionRequest
    {
        [Required, StringLength(50)]
        public string? CodeChucNang { get; set; }

        [Required, StringLength(100)]
        public string? TenChucNang { get; set; }

        [StringLength(200)]
        public string? MoTa { get; set; }

        public bool? TrangThai { get; set; } = true;
    }

    public class UpdateFunctionRequest
    {
        [Required]
        public int? MaChucNang { get; set; }

        [StringLength(50)]
        public string? CodeChucNang { get; set; }

        [StringLength(100)]
        public string? TenChucNang { get; set; }

        [StringLength(200)]
        public string? MoTa { get; set; }

        public bool? TrangThai { get; set; }
    }

    // ===== Role-Function mapping by Codes =====
    public class CreateRoleFunctionByCodeRequest
    {
        [Required] public string? CodeQuyen { get; set; }
        [Required] public string? CodeChucNang { get; set; }
    }

    public class UpdateRoleFunctionByCodeRequest
    {
        [Required] public string? FromCodeQuyen { get; set; }
        [Required] public string? FromCodeChucNang { get; set; }

        [Required] public string? ToCodeQuyen { get; set; }
        [Required] public string? ToCodeChucNang { get; set; }
    }
}
