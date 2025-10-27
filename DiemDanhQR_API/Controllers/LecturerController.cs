// File: Controllers/LecturersController.cs
// Bảng: GiangVien
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

        [HttpPost("create")]
        [Authorize(Roles = "ADMIN")]
        [RequestSizeLimit(5_000_000)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<CreateLecturerResponse>>> Create([FromForm] CreateLecturerRequest req)
        {
            // Mặc định MaNguoiDung = MaGiangVien nếu không truyền
            req.MaNguoiDung ??= req.MaGiangVien;

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

        [HttpPut("profile")]
        [Authorize(Roles = "ADMIN")]
        [RequestSizeLimit(5_000_000)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<UpdateLecturerResponse>>> Update([FromForm] UpdateLecturerRequest req)
        {
            var data = await _svc.UpdateAsync(req);
            return Ok(new ApiResponse<UpdateLecturerResponse>
            {
                Status = 200,
                Message = "Cập nhật thông tin giảng viên thành công.",
                Data = data
            });
        }
    }
}
