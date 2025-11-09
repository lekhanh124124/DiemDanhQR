using System.Security.Claims;
using api.DTOs;
using api.ErrorHandling;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService) => _authService = authService;

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromForm] LoginRequest req)
        {
            var result = await _authService.LoginAsync(req);
            return Ok(new ApiResponse<LoginResponse>
            {
                Status = "200",
                Message = "Đăng nhập thành công.",
                Data = result,
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<LogoutResponse>>> Logout()
        {
            // Lấy TenDangNhap từ claims
            var tenDangNhap =
                User.FindFirst("TenDangNhap")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(tenDangNhap))
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Không xác định được người dùng hiện tại.");

            var res = await _authService.LogoutAsync(tenDangNhap!);
            return Ok(new ApiResponse<LogoutResponse>
            {
                Status = "200",
                Message = "Đăng xuất thành công.",
                Data = res,
            });
        }

        [HttpPost("refreshtoken")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<RefreshAccessTokenResponse>>> Refresh([FromForm] RefreshTokenRequest req)
        {
            var res = await _authService.RefreshAccessTokenAsync(req);
            return Ok(new ApiResponse<RefreshAccessTokenResponse>
            {
                Status = "200",
                Message = "Làm mới token thành công.",
                Data = res,
            });
        }

        [HttpPost("changepassword")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ChangePasswordResponse>>> ChangePassword([FromForm] ChangePasswordRequest req)
        {
            var tenDangNhap =
                User.FindFirst("TenDangNhap")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(tenDangNhap))
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Không xác định được người dùng hiện tại.");

            var res = await _authService.ChangePasswordAsync(tenDangNhap!, req);
            return Ok(new ApiResponse<ChangePasswordResponse>
            {
                Status = "200",
                Message = "Đổi mật khẩu thành công.",
                Data = res,
            });
        }

        [HttpPost("refreshpassword")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<RefreshPasswordResponse>>> RefreshPassword([FromForm] RefreshPasswordRequest req)
        {
            var res = await _authService.RefreshPasswordToUserIdAsync(req);
            return Ok(new ApiResponse<RefreshPasswordResponse>
            {
                Status = "200",
                Message = "Làm mới mật khẩu thành công.",
                Data = res,
            });
        }
    }
}
