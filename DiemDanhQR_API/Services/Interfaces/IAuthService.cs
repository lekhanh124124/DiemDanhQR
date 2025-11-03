// File: Services/Interfaces/IAuthService.cs
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;

namespace DiemDanhQR_API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<LogoutResponse> LogoutAsync(string tenDangNhap);
        Task<RefreshAccessTokenResponse> RefreshAccessTokenAsync(RefreshTokenRequest request);
        Task<ChangePasswordResponse> ChangePasswordAsync(string tenDangNhapFromClaims, ChangePasswordRequest request);
        Task<RefreshPasswordResponse> RefreshPasswordToUserIdAsync(RefreshPasswordRequest request);
    }
}
