// File: Services/Interfaces/IAuthService.cs
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;

namespace DiemDanhQR_API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);
        Task<ApiResponse<LogoutResponse>> LogoutAsync(string maNguoiDung);
        Task<ApiResponse<RefreshAccessTokenResponse>> RefreshAccessTokenAsync(RefreshTokenRequest request);
        Task<ApiResponse<ChangePasswordResponse>> ChangePasswordAsync(string maNguoiDungFromClaims, ChangePasswordRequest request);
        Task<ApiResponse<RefreshPasswordResponse>> RefreshPasswordToUserIdAsync(RefreshPasswordRequest request);
    }
}
