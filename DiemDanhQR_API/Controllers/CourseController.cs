// File: Controllers/CourseController.cs
// Bảng LopHocPhan + ThamGiaLop
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;
using DiemDanhQR_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiemDanhQR_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _svc;
        public CourseController(ICourseService svc) => _svc = svc;

        // GET: /api/course/list
        [HttpGet("list")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<CourseListResponse>>> GetList([FromQuery] CourseListRequest req)
        {
            var data = await _svc.GetListAsync(req);

            var res = new ApiResponse<CourseListResponse>
            {
                Status = 200,
                Message = "Lấy danh sách lớp học phần thành công.",
                Data = data
            };
            return Ok(res);
        }
    }
}
