// File: Controllers/UsersController.cs
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;
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

        [HttpGet("info")]
        public async Task<ActionResult<ApiResponse<object>>> GetInfo([FromBody] GetUserInfoRequest req)
        {
            var result = await _svc.GetInfoAsync(req);
            return Ok(result);
        }

        /// <summary>
        /// Tạo người dùng mới. TenDangNhap = MaNguoiDung, HoTen = MaNguoiDung,
        /// mật khẩu khởi tạo = MaNguoiDung (được băm BCrypt).
        /// </summary>
        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<CreateUsertResponse>>> Create([FromBody] CreateUserRequest req)
        {
            var result = await _svc.CreateAsync(req);
            return Ok(result);
        }
    }
}
