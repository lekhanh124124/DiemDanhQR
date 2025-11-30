// File: Controllers/LecturerController.cs
// Bảng: GiangVien
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.Services.Interfaces;
using api.DTOs;
using api.Helpers;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LecturerController : ControllerBase
    {
        private readonly ILecturerService _svc;
        public LecturerController(ILecturerService svc) => _svc = svc;

        [HttpPost("create")]
        [Authorize]
        [RequestSizeLimit(5_000_000)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<CreateLecturerResponse>>> Create([FromForm] CreateLecturerRequest req)
        {
            if (!JwtHelper.IsAdmin(User))
                return Forbid("Chỉ ADMIN mới được phép thực hiện thao tác này.");

            var data = await _svc.CreateAsync(req);
            return Ok(new ApiResponse<CreateLecturerResponse>
            {
                Status = "200",
                Message = "Tạo giảng viên thành công.",
                Data = data
            });
        }

        [HttpGet("list")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PagedResult<LecturerListItemResponse>>>> List([FromQuery] GetLecturersRequest req)
        {
            if (!JwtHelper.IsAdmin(User))
                return Forbid("Chỉ ADMIN mới được phép thực hiện thao tác này.");

            var data = await _svc.GetListAsync(req);
            return Ok(new ApiResponse<PagedResult<LecturerListItemResponse>>
            {
                Status = "200",
                Message = "Lấy danh sách giảng viên thành công.",
                Data = data
            });
        }

        [HttpPut("profile")]
        [Authorize]
        [RequestSizeLimit(5_000_000)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<UpdateLecturerResponse>>> Update([FromForm] UpdateLecturerRequest req)
        {
            if (!JwtHelper.IsAdmin(User))
                return Forbid("Chỉ ADMIN mới được phép thực hiện thao tác này.");

            var data = await _svc.UpdateAsync(req);
            return Ok(new ApiResponse<UpdateLecturerResponse>
            {
                Status = "200",
                Message = "Cập nhật thông tin giảng viên thành công.",
                Data = data
            });
        }
    }
}
