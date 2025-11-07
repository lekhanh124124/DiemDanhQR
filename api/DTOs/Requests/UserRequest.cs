// File: DTOs/Requests/UserRequest.cs
using System.ComponentModel.DataAnnotations;
namespace api.DTOs.Requests
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        public string? TenDangNhap { get; set; }
        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        public int? MaQuyen { get; set; }
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
}
