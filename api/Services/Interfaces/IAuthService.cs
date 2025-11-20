// File: Services/Interfaces/IAuthService.cs
using api.DTOs;

namespace api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<LogoutResponse> LogoutAsync(string tenDangNhap);
        Task<RefreshAccessTokenResponse> RefreshAccessTokenAsync(RefreshTokenRequest request);
        Task<ChangePasswordResponse> ChangePasswordAsync(string tenDangNhapFromClaims, ChangePasswordRequest request);
        Task<RefreshPasswordResponse> RefreshPasswordToUserIdAsync(RefreshPasswordRequest request);
        Task<UserRoleFunctionsResponse> GetCurrentUserRoleFunctionsAsync(string tenDangNhapFromClaims);

    }
}
