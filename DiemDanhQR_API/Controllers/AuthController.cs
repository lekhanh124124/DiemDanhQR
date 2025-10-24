// File: Controllers/AuthController.cs
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
            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<LogoutResponse>>> Logout()
        {
            var userId = HelperFunctions.GetUserIdFromClaims(User);
            if (string.IsNullOrWhiteSpace(userId))
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Không xác định được người dùng hiện tại.");

            var res = await _authService.LogoutAsync(userId!);
            return Ok(res);
        }

        [HttpPost("refreshtoken")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<RefreshAccessTokenResponse>>> Refresh([FromBody] RefreshTokenRequest req)
        {
            var res = await _authService.RefreshAccessTokenAsync(req);
            return Ok(res);
        }

        [HttpPost("changepassword")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ChangePasswordResponse>>> ChangePassword([FromBody] ChangePasswordRequest req)
        {
            var userId = HelperFunctions.GetUserIdFromClaims(User);
            if (string.IsNullOrWhiteSpace(userId))
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Không xác định được người dùng hiện tại.");

            var res = await _authService.ChangePasswordAsync(userId!, req);
            return Ok(res);
        }

        [HttpPost("refreshpassword")]
        [Authorize(Roles="ADMIN")] 
        public async Task<ActionResult<ApiResponse<RefreshPasswordResponse>>> RefreshPassword([FromBody] RefreshPasswordRequest req)
        {
            var res = await _authService.RefreshPasswordToUserIdAsync(req);
            return Ok(res);
        }
    }
}
