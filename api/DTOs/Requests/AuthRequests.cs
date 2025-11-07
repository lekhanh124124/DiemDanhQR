// File: DTOs/Requests/AuthRequests.cs
using System.ComponentModel.DataAnnotations;
namespace api.DTOs.Requests
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        public string? TenDangNhap { get; set; }
        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        public string? MatKhau { get; set; }
    }

    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        public string? TenDangNhap { get; set; }
        [Required(ErrorMessage = "Refresh token không được để trống.")]
        public string? RefreshToken { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Mật khẩu cũ không được để trống.")]
        public string? MatKhauCu { get; set; }
        [Required(ErrorMessage = "Mật khẩu mới không được để trống.")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.")]
        public string?  MatKhauMoi { get; set; }
    }
    public class RefreshPasswordRequest
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        public string? TenDangNhap { get; set; }
    }
}
