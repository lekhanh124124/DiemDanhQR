// File: Controllers/AuthController.cs
// Bảng NguoiDung 
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;
using DiemDanhQR_API.Helpers;
using DiemDanhQR_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiemDanhQR_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService) => _authService = authService;

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest req)
        {
            var result = await _authService.LoginAsync(req);
            return Ok(new ApiResponse<LoginResponse>
            {
                Status = 200,
                Message = "Đăng nhập thành công.",
                Data = result,
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<LogoutResponse>>> Logout()
        {
            var tenDangNhap = HelperFunctions.GetUserIdFromClaims(User);
            if (string.IsNullOrWhiteSpace(tenDangNhap))
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Không xác định được người dùng hiện tại.");

            var res = await _authService.LogoutAsync(tenDangNhap!);
            return Ok(new ApiResponse<LogoutResponse>
            {
                Status = 200,
                Message = "Đăng xuất thành công.",
                Data = res,             
            });
        }

        [HttpPost("refreshtoken")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<RefreshAccessTokenResponse>>> Refresh([FromBody] RefreshTokenRequest req)
        {
            var res = await _authService.RefreshAccessTokenAsync(req);
            return Ok(new ApiResponse<RefreshAccessTokenResponse>
            {
                Status = 200,
                Message = "Làm mới token thành công.",
                Data = res,
            });
        }

        [HttpPost("changepassword")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ChangePasswordResponse>>> ChangePassword([FromBody] ChangePasswordRequest req)
        {
            var tenDangNhap = HelperFunctions.GetUserIdFromClaims(User);
            if (string.IsNullOrWhiteSpace(tenDangNhap))
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Không xác định được người dùng hiện tại.");

            var res = await _authService.ChangePasswordAsync(tenDangNhap!, req);
            return Ok(new ApiResponse<ChangePasswordResponse>
            {
                Status = 200,
                Message = "Đổi mật khẩu thành công.",
                Data = res,
            });
        }

        [HttpPost("refreshpassword")]
        [Authorize(Roles="ADMIN")] 
        public async Task<ActionResult<ApiResponse<RefreshPasswordResponse>>> RefreshPassword([FromBody] RefreshPasswordRequest req)
        {
            var res = await _authService.RefreshPasswordToUserIdAsync(req);
            return Ok(new ApiResponse<RefreshPasswordResponse>
            {
                Status = 200,
                Message = "Làm mới mật khẩu thành công.",
                Data = res,
            });
        }
    }
}
