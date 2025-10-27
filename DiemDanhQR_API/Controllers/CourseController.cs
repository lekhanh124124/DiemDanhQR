// File: Controllers/CourseController.cs
// Bảng LopHocPhan + ThamGiaLop + MonHoc
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
        public async Task<ActionResult<ApiResponse<PagedResult<CourseListItem>>>> GetList([FromQuery] CourseListRequest req)
        {
            var data = await _svc.GetListAsync(req);

            return Ok(new ApiResponse<PagedResult<CourseListItem>>
            {
                Status = 200,
                Message = "Lấy danh sách lớp học phần thành công.",
                Data = data
            });
        }

        // GET: /api/course/subjects
        [HttpGet("subjects")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PagedResult<SubjectListItem>>>> GetSubjects([FromQuery] SubjectListRequest req)
        {
            var data = await _svc.GetSubjectsAsync(req);
            return Ok(new ApiResponse<PagedResult<SubjectListItem>>
            {
                Status = 200,
                Message = "Lấy danh sách môn học thành công.",
                Data = data
            });
        }

    }
}
