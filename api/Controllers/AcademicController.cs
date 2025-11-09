// File: Controllers/AcademicController.cs
// Bảng: Khoa + Nganh
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.DTOs;
using api.Services.Interfaces;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AcademicController : ControllerBase
    {
        private readonly IAcademicService _svc;
        public AcademicController(IAcademicService svc) => _svc = svc;

        // ===== KHOA =====
        [HttpGet("departments")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PagedResult<KhoaDetailResponse>>>> GetKhoa([FromQuery] KhoaListRequest req)
        {
            var data = await _svc.GetKhoaListAsync(req);
            return Ok(new ApiResponse<PagedResult<KhoaDetailResponse>>
            {
                Status = "200",
                Message = "Lấy danh sách Khoa thành công.",
                Data = data
            });
        }

        [HttpPost("create-department")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<KhoaDetailResponse>>> CreateKhoa([FromForm] CreateKhoaRequest req)
        {
            var data = await _svc.CreateKhoaAsync(req);
            return Ok(new ApiResponse<KhoaDetailResponse>
            {
                Status = "200",
                Message = "Tạo Khoa thành công.",
                Data = data
            });
        }

        [HttpPut("update-department")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<KhoaDetailResponse>>> UpdateKhoa([FromForm] UpdateKhoaRequest req)
        {
            var data = await _svc.UpdateKhoaAsync(req);
            return Ok(new ApiResponse<KhoaDetailResponse>
            {
                Status = "200",
                Message = "Cập nhật Khoa thành công.",
                Data = data
            });
        }

        [HttpDelete("delete-department")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteKhoa([FromQuery] int maKhoa)
        {
            var ok = await _svc.DeleteKhoaAsync(maKhoa);
            return Ok(new ApiResponse<object>
            {
                Status = "200",
                Message = ok ? "Xoá Khoa thành công." : "Không thể xoá Khoa.",
                Data = null!
            });
        }

        // ===== NGÀNH =====
        [HttpGet("majors")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PagedResult<NganhDetailResponse>>>> GetNganh([FromQuery] NganhListRequest req)
        {
            var data = await _svc.GetNganhListAsync(req);
            return Ok(new ApiResponse<PagedResult<NganhDetailResponse>>
            {
                Status = "200",
                Message = "Lấy danh sách Ngành thành công.",
                Data = data
            });
        }

        [HttpPost("create-major")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<NganhDetailResponse>>> CreateNganh([FromForm] CreateNganhRequest req)
        {
            var data = await _svc.CreateNganhAsync(req);
            return Ok(new ApiResponse<NganhDetailResponse>
            {
                Status = "200",
                Message = "Tạo Ngành thành công.",
                Data = data
            });
        }

        [HttpPut("update-major")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<NganhDetailResponse>>> UpdateNganh([FromForm] UpdateNganhRequest req)
        {
            var data = await _svc.UpdateNganhAsync(req);
            return Ok(new ApiResponse<NganhDetailResponse>
            {
                Status = "200",
                Message = "Cập nhật Ngành thành công.",
                Data = data
            });
        }

        [HttpDelete("delete-major")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteNganh([FromQuery] int maNganh)
        {
            var ok = await _svc.DeleteNganhAsync(maNganh);
            return Ok(new ApiResponse<object>
            {
                Status = "200",
                Message = ok ? "Xoá Ngành thành công." : "Không thể xoá Ngành.",
                Data = null!
            });
        }
    }
}
