// File: DTOs/Responses/AuthResponses.cs
namespace DiemDanhQR_API.DTOs.Responses
{
    public class LoginResponse
    {
        public string AccessToken { get; }
        public string RefreshToken { get; }
        public DateTime ExpiresAt { get; }
        public string MaNguoiDung { get; }
        public string TenDangNhap { get; }
        public string HoTen { get; }
        public int MaQuyen { get; }
        public string RoleCode { get; }

        public LoginResponse(
            string accessToken,
            string refreshToken,
            DateTime expiresAt,
            string maNguoiDung,
            string tenDangNhap,
            string hoTen,
            int maQuyen,
            string roleCode)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresAt = expiresAt;
            MaNguoiDung = maNguoiDung;
            TenDangNhap = tenDangNhap;
            HoTen = hoTen;
            MaQuyen = maQuyen;
            RoleCode = roleCode;
        }
    }

    public class LogoutResponse
    {
        public string MaNguoiDung { get; }
        public DateTime RevokedAt { get; }

        public LogoutResponse(string maNguoiDung, DateTime revokedAt)
        {
            MaNguoiDung = maNguoiDung;
            RevokedAt = revokedAt;
        }
    }

    /// <summary>Response khi cấp AccessToken mới.</summary>
    public class RefreshAccessTokenResponse
    {
        public string AccessToken { get; }
        public DateTime ExpiresAt { get; }
        public string MaNguoiDung { get; }
        public string TenDangNhap { get; }
        public string HoTen { get; }
        public int MaQuyen { get; }
        public string RoleCode { get; }

        // Service truyền thẳng tham số
        public RefreshAccessTokenResponse(
            string accessToken,
            DateTime expiresAt,
            string maNguoiDung,
            string tenDangNhap,
            string hoTen,
            int maQuyen,
            string roleCode)
        {
            AccessToken = accessToken;
            ExpiresAt = expiresAt;
            MaNguoiDung = maNguoiDung;
            TenDangNhap = tenDangNhap;
            HoTen = hoTen;
            MaQuyen = maQuyen;
            RoleCode = roleCode;
        }
    }

    public class ChangePasswordResponse
    {
        public string MaNguoiDung { get; }
        public DateTime ChangedAt { get; } // giờ VN bạn đang dùng cho DB

        public ChangePasswordResponse(string maNguoiDung, DateTime changedAt)
        {
            MaNguoiDung = maNguoiDung;
            ChangedAt = changedAt;
        }
    }
    public class RefreshPasswordResponse
    {
        public string MaNguoiDung { get; }
        public string TenDangNhap { get; }
        public string NewPassword { get; }     // plaintext để hiển thị cho user
        public DateTime ChangedAt { get; }     // giờ VN (bạn đang dùng DateTime)

        public RefreshPasswordResponse(string maNguoiDung, string tenDangNhap, string newPassword, DateTime changedAt)
        {
            MaNguoiDung = maNguoiDung;
            TenDangNhap = tenDangNhap;
            NewPassword = newPassword;
            ChangedAt = changedAt;
        }
    }
}
