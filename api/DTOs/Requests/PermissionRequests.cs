using System.ComponentModel.DataAnnotations;

namespace api.DTOs
{
    public class PermissionListRequest
    {
        public int? MaQuyen { get; set; }
        public string? CodeQuyen { get; set; }
        public string? TenQuyen { get; set; }
        public string? MoTa { get; set; }

        // Lọc theo mã chức năng (lấy danh sách phân quyền theo mã chức năng)
        public int? MaChucNang { get; set; }

        // Sắp xếp
        public string? SortBy { get; set; }   // MaQuyen | CodeQuyen | TenQuyen | MoTa
        public string? SortDir { get; set; }  // ASC | DESC

        // Phân trang
        public int? Page { get; set; } = 1;
        public int? PageSize { get; set; } = 20;
    }

    public class FunctionListRequest
    {
        public int? MaChucNang { get; set; }
        public string? CodeChucNang { get; set; }
        public string? TenChucNang { get; set; }
        public string? MoTa { get; set; }

        // Lọc theo mã phân quyền (lấy danh sách chức năng theo mã phân quyền)
        public int? MaQuyen { get; set; }
        public int? ParentChucNangId { get; set; }   // null = chỉ lấy root nếu bạn muốn, hoặc bỏ lọc nếu không truyền


        // Sắp xếp
        public string? SortBy { get; set; }   // MaChucNang | CodeChucNang | TenChucNang | MoTa
        public string? SortDir { get; set; }  // ASC | DESC

        // Phân trang
        public int? Page { get; set; } = 1;
        public int? PageSize { get; set; } = 20;
    }

    // ===== Role (PhanQuyen) =====
    public class CreateRoleRequest
    {
        [Required(ErrorMessage = "Mã quyền không được để trống.")]
        public string? CodeQuyen { get; set; }

        [Required(ErrorMessage = "Tên quyền không được để trống.")]
        public string? TenQuyen { get; set; }
        public string? MoTa { get; set; }

    }

    public class UpdateRoleRequest
    {
        [Required(ErrorMessage = "Mã quyền không được để trống.")]
        public int? MaQuyen { get; set; }

        public string? CodeQuyen { get; set; }
        public string? TenQuyen { get; set; }
        public string? MoTa { get; set; }

    }

    // ===== Function (ChucNang) =====
    public class CreateFunctionRequest
    {
        [Required(ErrorMessage = "Mã chức năng không được để trống.")]
        public string? CodeChucNang { get; set; }

        [Required(ErrorMessage = "Tên chức năng không được để trống.")]
        public string? TenChucNang { get; set; }

        public string? MoTa { get; set; }
        public int? ParentChucNangId { get; set; }
    }

    public class UpdateFunctionRequest
    {
        [Required(ErrorMessage = "Mã chức năng không được để trống.")]
        public int? MaChucNang { get; set; }

        public string? CodeChucNang { get; set; }
        public string? TenChucNang { get; set; }
        public string? MoTa { get; set; }
        public int? ParentChucNangId { get; set; }
    }

    public class CreateRoleFunctionByCodeRequest
    {
        [Required(ErrorMessage = "Mã quyền không được để trống.")]
        public int? MaQuyen { get; set; }
        [Required(ErrorMessage = "Mã chức năng không được để trống.")]
        public int? MaChucNang { get; set; }
        public bool? TrangThai { get; set; } = true;
    }

    public class UpdateRoleFunctionByCodeRequest
    {
        [Required(ErrorMessage = "Mã quyền không được để trống.")]
        public int? FromMaQuyen { get; set; }
        [Required(ErrorMessage = "Mã chức năng không được để trống.")]
        public int? FromMaChucNang { get; set; }

        public int? ToMaQuyen { get; set; }
        public int? ToMaChucNang { get; set; }

        // Optional: trạng thái mới cho mapping đích
        public bool? TrangThai { get; set; }
    }

    public class RoleFunctionListRequest
    {
        public int? MaQuyen { get; set; }
        public int? MaChucNang { get; set; }

        // Sắp xếp: MaQuyen | CodeQuyen | TenQuyen | MaChucNang | CodeChucNang | TenChucNang | TrangThai
        public string? SortBy { get; set; }
        public string? SortDir { get; set; } // ASC | DESC

        // Phân trang
        public int? Page { get; set; } = 1;
        public int? PageSize { get; set; } = 20;
    }
}
