// File: Controllers/UsersController.cs
// Bảng: NguoiDung + LichSuHoatDong
using System.Security.Claims;
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
                // Không truyền: lấy thông tin bản thân theo tên đăng nhập trong token
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
                // Có truyền: chỉ ADMIN được phép xem người khác theo tên đăng nhập
                if (!isAdmin)
                    ApiExceptionHelper.Throw(ApiErrorCode.Forbidden, "Chỉ ADMIN mới được tra cứu theo tên đăng nhập.");
                result = await _svc.GetInfoAsync(tenDangNhap!);
            }

            return Ok(new ApiResponse<object>
            {
                Status = 200,
                Message = "Lấy thông tin người dùng thành công.",
                Data = result,
            });
        }

        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<CreateUserResponse>>> Create([FromBody] CreateUserRequest req)
        {
            var result = await _svc.CreateAsync(req);
            return Ok(new ApiResponse<CreateUserResponse>
            {
                Status = 200,
                Message = "Tạo người dùng thành công.",
                Data = result,
            });
        }

        // File: Controllers/UserController.cs
        [Authorize]
        [HttpGet("activity")]
        public async Task<ActionResult<ApiResponse<PagedResult<UserActivityItem>>>> GetActivity([FromQuery] UserActivityListRequest req)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            var isAdmin = string.Equals(role, "ADMIN", StringComparison.OrdinalIgnoreCase);

            // Lấy username từ token để ràng buộc user thường
            var usernameFromToken =
                    User.FindFirst("TenDangNhap")?.Value
                    ?? User.FindFirst(ClaimTypes.Name)?.Value
                    ?? User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(usernameFromToken))
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Phiên không hợp lệ.");

            if (!isAdmin)
            {
                // User thường: chỉ được xem lịch sử của chính mình
                req.TenDangNhap = usernameFromToken;
            }
            else
            {
                // ADMIN: có thể xem tất cả nếu không truyền; nếu truyền thì lọc theo TenDangNhap
                req.TenDangNhap = string.IsNullOrWhiteSpace(req.TenDangNhap)
                    ? null
                    : HelperFunctions.NormalizeCode(req.TenDangNhap);
            }

            var data = await _svc.GetActivityAsync(req);
            return Ok(new ApiResponse<PagedResult<UserActivityItem>>
            {
                Status = 200,
                Message = "Lấy danh sách lịch sử hoạt động thành công.",
                Data = data
            });
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<ActionResult<ApiResponse<UpdateUserProfileResponse>>> UpdateProfile(
                    [FromBody] UpdateUserProfileRequest req)
        {
            var currentUserId = HelperFunctions.GetUserIdFromClaims(User);
            if (string.IsNullOrWhiteSpace(currentUserId))
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Phiên không hợp lệ.");

            var data = await _svc.UpdateProfileAsync(currentUserId!, req);
            return Ok(new ApiResponse<UpdateUserProfileResponse>
            {
                Status = 200,
                Message = "Cập nhật thông tin cá nhân thành công.",
                Data = data
            });
        }
    }
}
