// File: Services/Interfaces/IAuthService.cs
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;

namespace DiemDanhQR_API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<LogoutResponse> LogoutAsync(string maNguoiDung);
        Task<RefreshAccessTokenResponse> RefreshAccessTokenAsync(RefreshTokenRequest request);
        Task<ChangePasswordResponse> ChangePasswordAsync(string maNguoiDungFromClaims, ChangePasswordRequest request);
        Task<RefreshPasswordResponse> RefreshPasswordToUserIdAsync(RefreshPasswordRequest request);
    }
}
