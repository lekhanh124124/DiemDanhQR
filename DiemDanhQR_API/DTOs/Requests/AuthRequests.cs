// File: DTOs/Requests/AuthRequests.cs
namespace DiemDanhQR_API.DTOs.Requests
{
    public class LoginRequest
    {
        public string TenDangNhap { get; set; } = string.Empty;
        public string MatKhau { get; set; } = string.Empty;
    }

    public class RefreshTokenRequest
    {
        public string TenDangNhap { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
    {
        public string MatKhauCu { get; set; } = string.Empty;
        public string MatKhauMoi { get; set; } = string.Empty;
    }
    public class RefreshPasswordRequest
    {
        public string TenDangNhap { get; set; } = string.Empty;
    }
}
