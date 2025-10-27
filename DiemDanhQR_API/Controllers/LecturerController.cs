// File: Controllers/LecturersController.cs
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;
using DiemDanhQR_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiemDanhQR_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LecturerController : ControllerBase
    {
        private readonly ILecturerService _svc;
        public LecturerController(ILecturerService svc) => _svc = svc;

        [HttpPost("Create")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<CreateLecturerResponse>>> Create([FromBody] CreateLecturerRequest req)
        {
            var result = await _svc.CreateAsync(req);
            return Ok(new ApiResponse<CreateLecturerResponse>
            {
                Status = 200,
                Message = "Tạo giảng viên thành công.",
                Data = result
            });
        }
        [HttpGet("List")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PagedResult<LecturerListItemResponse>>>> List([FromQuery] GetLecturersRequest req)
        {
            var result = await _svc.GetListAsync(req);
            return Ok(new ApiResponse<PagedResult<LecturerListItemResponse>>
            {
                Status = 200,
                Message = "Lấy danh sách giảng viên thành công.",
                Data = result
            });
        }
    }
}
