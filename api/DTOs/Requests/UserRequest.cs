// File: DTOs/Requests/UserRequest.cs
using System.ComponentModel.DataAnnotations;
namespace api.DTOs
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        public string? TenDangNhap { get; set; }
        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        public int? MaQuyen { get; set; }
        public string? HoTen { get; set; }
        public byte? GioiTinh { get; set; }
        public IFormFile? AnhDaiDien { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? DiaChi { get; set; }
    }

    public class GetUserInfoRequest
    {
        public string? TenDangNhap { get; set; }
    }

    public class UserActivityListRequest
    {
        // Paging
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Sorting
        public string? SortBy { get; set; } = "ThoiGian";
        public string? SortDir { get; set; } = "DESC";

        // Filters
        public string? TenDangNhap { get; set; }

        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class UpdateUserProfileRequest
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        public string? TenDangNhap { get; set; }
        public int? MaQuyen { get; set; }
        public string? HoTen { get; set; }
        public byte? GioiTinh { get; set; }
        public IFormFile? AnhDaiDien { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? DiaChi { get; set; }
    }

    public class UserListRequest
    {
        public int? page { get; set; } = 1;
        public int? pageSize { get; set; } = 20;
        public string? sortBy { get; set; } = "MaNguoiDung";
        public string? sortDir { get; set; } = "DESC";
        public string? tenDangNhap { get; set; } = null;
        public string? hoTen { get; set; } = null;
        public int? maQuyen { get; set; } = null;
        public bool? trangThai { get; set; } = null;
    }
}
