using api.DTOs.Requests;
using api.DTOs.Responses;

namespace api.Services.Interfaces
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
