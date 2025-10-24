// File: Controllers/UsersController.cs
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
        public async Task<ActionResult<ApiResponse<CreateUsertResponse>>> Create([FromBody] CreateUserRequest req)
        {
            var result = await _svc.CreateAsync(req);
            return Ok(new ApiResponse<CreateUsertResponse>
            {
                Status = 200,
                Message = "Tạo người dùng thành công.",
                Data = result,
            });
        }
    }
}
