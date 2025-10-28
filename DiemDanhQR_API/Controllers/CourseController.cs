// File: Controllers/CourseController.cs
// Bảng LopHocPhan + ThamGiaLop + MonHoc
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

        // POST: /api/course/create-subject
        [HttpPost("create-subject")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<CreateSubjectResponse>>> CreateSubject([FromForm] CreateSubjectRequest req)
        {
            // ModelState invalid -> Middleware sẽ chuẩn hoá về ValidationError với message từ DataAnnotations
            var currentUserId = HelperFunctions.GetUserIdFromClaims(User);
            var data = await _svc.CreateSubjectAsync(req, currentUserId);

            return Ok(new ApiResponse<CreateSubjectResponse>
            {
                Status = 200,
                Message = "Tạo môn học thành công.",
                Data = data
            });
        }


        // POST: /api/course/create-course
        [HttpPost("create-course")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<CreateCourseResponse>>> CreateCourse([FromForm] CreateCourseRequest req)
        {
            var currentUserId = HelperFunctions.GetUserIdFromClaims(User);
            var data = await _svc.CreateCourseAsync(req, currentUserId);

            return Ok(new ApiResponse<CreateCourseResponse>
            {
                Status = 200,
                Message = "Tạo lớp học phần thành công.",
                Data = data
            });
        }

        // POST: /api/course/add-student
        [HttpPost("add-student")]
        [Authorize(Roles = "ADMIN,GV")]
        public async Task<ActionResult<ApiResponse<AddStudentToCourseResponse>>> AddStudentToCourse([FromForm] AddStudentToCourseRequest req)
        {
            var currentUserId = HelperFunctions.GetUserIdFromClaims(User);
            var data = await _svc.AddStudentToCourseAsync(req, currentUserId);

            return Ok(new ApiResponse<AddStudentToCourseResponse>
            {
                Status = 200,
                Message = "Thêm sinh viên vào lớp học phần thành công.",
                Data = data
            });
        }
    }
}
