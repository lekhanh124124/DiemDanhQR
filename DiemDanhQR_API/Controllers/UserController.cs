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
        public async Task<ActionResult<ApiResponse<object>>> GetInfo([FromQuery] string? maNguoiDung)
        {
            var currentUserId = HelperFunctions.GetUserIdFromClaims(User);
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            var isAdmin = string.Equals(role, "ADMIN", StringComparison.OrdinalIgnoreCase);

            var requestedMaND = HelperFunctions.NormalizeCode(maNguoiDung);

            if (!isAdmin && !string.IsNullOrWhiteSpace(requestedMaND))
            {
                ApiExceptionHelper.Throw(ApiErrorCode.Forbidden, "Bạn không có quyền xem thông tin người dùng khác.");
            }

            var targetMaND = string.IsNullOrWhiteSpace(requestedMaND)
                ? currentUserId
                : requestedMaND;

            var result = await _svc.GetInfoAsync(targetMaND!);
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
            var currentUserId = HelperFunctions.GetUserIdFromClaims(User);
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            var isAdmin = string.Equals(role, "ADMIN", StringComparison.OrdinalIgnoreCase);

            var requestedMaND = HelperFunctions.NormalizeCode(req.MaNguoiDung);

            if (!isAdmin)
            {
                // user thường: không được xem all, không được xem người khác
                if (req.AllUsers == true || !string.IsNullOrWhiteSpace(requestedMaND))
                    ApiExceptionHelper.Throw(ApiErrorCode.Forbidden, "Bạn không có quyền xem lịch sử của người khác.");
                req.MaNguoiDung = currentUserId; // ép về chính mình
            }
            else
            {
                // ADMIN: nếu AllUsers=true -> bỏ lọc MaNguoiDung để lấy tất cả
                if (req.AllUsers == true) req.MaNguoiDung = null;
                else req.MaNguoiDung = string.IsNullOrWhiteSpace(requestedMaND) ? currentUserId : requestedMaND;
            }

            var data = await _svc.GetActivityAsync(req);
            return Ok(new ApiResponse<PagedResult<UserActivityItem>>
            {
                Status = 200,
                Message = req.AllUsers == true ? "Lấy toàn bộ lịch sử hoạt động thành công." : "Lấy danh sách lịch sử hoạt động thành công.",
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
