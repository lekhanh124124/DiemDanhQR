// File: Controllers/UserController.cs
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
    public class UserController : ControllerBase
    {
        private readonly IUserService _svc;

        public UserController(IUserService svc)
        {
            _svc = svc;
        }

        [Authorize]
        [HttpGet("info")]
        public async Task<ActionResult<ApiResponse<object>>> GetInfo([FromQuery] string? tenDangNhap)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            var isAdmin = string.Equals(role, "ADMIN", StringComparison.OrdinalIgnoreCase);

            object result;

            if (string.IsNullOrWhiteSpace(tenDangNhap))
            {
                var usernameFromToken =
                        User.FindFirst("TenDangNhap")?.Value
                        ?? User.FindFirst(ClaimTypes.Name)?.Value
                        ?? User.Identity?.Name;

                if (string.IsNullOrWhiteSpace(usernameFromToken))
                    ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Phiên không hợp lệ.");

                result = await _svc.GetInfoAsync(usernameFromToken!);
            }
            else
            {
                if (!isAdmin)
                    ApiExceptionHelper.Throw(ApiErrorCode.Forbidden, "Chỉ ADMIN mới được tra cứu theo tên đăng nhập.");
                result = await _svc.GetInfoAsync(tenDangNhap!);
            }

            return Ok(new ApiResponse<object>
            {
                Status = "200",
                Message = "Lấy thông tin người dùng thành công.",
                Data = result,
            });
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost("create")]
        [RequestSizeLimit(5_000_000)]
        public async Task<ActionResult<ApiResponse<CreateUserResponse>>> Create([FromForm] CreateUserRequest req)
        {
            var result = await _svc.CreateAsync(req);
            return Ok(new ApiResponse<CreateUserResponse>
            {
                Status = "200",
                Message = "Tạo người dùng thành công.",
                Data = result,
            });
        }

        [Authorize]
        [HttpPut("update")]
        [RequestSizeLimit(5_000_000)]
        public async Task<ActionResult<ApiResponse<UpdateUserProfileResponse>>> UpdateProfile([FromForm] UpdateUserProfileRequest req)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            var isAdmin = string.Equals(role, "ADMIN", StringComparison.OrdinalIgnoreCase);

            var usernameFromToken =
                    User.FindFirst("TenDangNhap")?.Value
                    ?? User.FindFirst(ClaimTypes.Name)?.Value
                    ?? User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(usernameFromToken))
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Phiên không hợp lệ.");

            if (!isAdmin && !string.Equals(usernameFromToken, req.TenDangNhap, StringComparison.OrdinalIgnoreCase))
                ApiExceptionHelper.Throw(ApiErrorCode.Forbidden, "Bạn không thể cập nhật hồ sơ của người khác.");

            var data = await _svc.UpdateProfileAsync(req, usernameFromToken);
            return Ok(new ApiResponse<UpdateUserProfileResponse>
            {
                Status = "200",
                Message = "Cập nhật người dùng thành công.",
                Data = data
            });
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("list")]
        public async Task<ActionResult<ApiResponse<PagedResult<UserItem>>>> List([FromQuery] UserListRequest req)
        {
            var data = await _svc.GetListAsync(req);
            return Ok(new ApiResponse<PagedResult<UserItem>>
            {
                Status = "200",
                Message = "Lấy danh sách người dùng thành công.",
                Data = data
            });
        }

        [Authorize]
        [HttpGet("activity")]
        public async Task<ActionResult<ApiResponse<PagedResult<UserActivityItem>>>> GetActivity([FromQuery] UserActivityListRequest req)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            var isAdmin = string.Equals(role, "ADMIN", StringComparison.OrdinalIgnoreCase);

            var usernameFromToken =
                    User.FindFirst("TenDangNhap")?.Value
                    ?? User.FindFirst(ClaimTypes.Name)?.Value
                    ?? User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(usernameFromToken))
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Phiên không hợp lệ.");

            if (!isAdmin)
            {
                req.TenDangNhap = usernameFromToken;
            }

            var data = await _svc.GetActivityAsync(req);
            return Ok(new ApiResponse<PagedResult<UserActivityItem>>
            {
                Status = "200",
                Message = "Lấy danh sách lịch sử hoạt động thành công.",
                Data = data
            });
        }
    }
}
