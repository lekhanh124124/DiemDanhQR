// File: Controllers/CourseController.cs
// Bảng LopHocPhan + ThamGiaLop + MonHoc
using api.DTOs;
using api.Helpers;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
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
            return Ok(new ApiResponse<PagedResult<CourseListItem>> { Status = "200", Message = "Lấy danh sách lớp học phần thành công.", Data = data });
        }

        // GET: /api/course/subjects
        [HttpGet("subjects")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<PagedResult<SubjectListItem>>>> GetSubjects([FromQuery] SubjectListRequest req)
        {
            var data = await _svc.GetSubjectsAsync(req);
            return Ok(new ApiResponse<PagedResult<SubjectListItem>> { Status = "200", Message = "Lấy danh sách môn học thành công.", Data = data });
        }

        // POST: /api/course/create-subject
        [HttpPost("create-subject")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<CreateSubjectResponse>>> CreateSubject([FromForm] CreateSubjectRequest req)
        {
            var currentUserLogin = JwtHelper.GetUsername(User);
            var data = await _svc.CreateSubjectAsync(req, currentUserLogin);
            return Ok(new ApiResponse<CreateSubjectResponse> { Status = "200", Message = "Tạo môn học thành công.", Data = data });
        }

        // POST: /api/course/create-course
        [HttpPost("create-course")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<CreateCourseResponse>>> CreateCourse([FromForm] CreateCourseRequest req)
        {
            var currentUserLogin = JwtHelper.GetUsername(User);
            var data = await _svc.CreateCourseAsync(req, currentUserLogin);
            return Ok(new ApiResponse<CreateCourseResponse> { Status = "200", Message = "Tạo lớp học phần thành công.", Data = data });
        }

        // GET: /api/course/semesters
        [HttpGet("semesters")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<PagedResult<SemesterListItem>>>> GetSemesters([FromQuery] SemesterListRequest req)
        {
            var data = await _svc.GetSemestersAsync(req);
            return Ok(new ApiResponse<PagedResult<SemesterListItem>> { Status = "200", Message = "Lấy danh sách học kỳ thành công.", Data = data });
        }

        // POST: /api/course/create-semester
        [HttpPost("create-semester")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<CreateSemesterResponse>>> CreateSemester([FromForm] CreateSemesterRequest req)
        {
            var currentUserLogin = JwtHelper.GetUsername(User);
            var data = await _svc.CreateSemesterAsync(req, currentUserLogin);
            return Ok(new ApiResponse<CreateSemesterResponse> { Status = "200", Message = "Tạo học kỳ thành công.", Data = data });
        }

        // PUT: /api/course/update-semester
        [HttpPut("update-semester")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<UpdateSemesterResponse>>> UpdateSemester([FromForm] UpdateSemesterRequest req)
        {
            var currentUserLogin = JwtHelper.GetUsername(User);
            var data = await _svc.UpdateSemesterAsync(req, currentUserLogin);
            return Ok(new ApiResponse<UpdateSemesterResponse> { Status = "200", Message = "Cập nhật học kỳ thành công.", Data = data });
        }

        // PUT: /api/course/update-subject
        [HttpPut("update-subject")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<UpdateSubjectResponse>>> UpdateSubject([FromForm] UpdateSubjectRequest req)
        {
            var currentUserLogin = JwtHelper.GetUsername(User);
            var data = await _svc.UpdateSubjectAsync(req, currentUserLogin);
            return Ok(new ApiResponse<UpdateSubjectResponse> { Status = "200", Message = "Cập nhật môn học thành công.", Data = data });
        }

        // PUT: /api/course/update-course
        [HttpPut("update-course")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<UpdateCourseResponse>>> UpdateCourse([FromForm] UpdateCourseRequest req)
        {
            var currentUserLogin = JwtHelper.GetUsername(User);
            var data = await _svc.UpdateCourseAsync(req, currentUserLogin);
            return Ok(new ApiResponse<UpdateCourseResponse> { Status = "200", Message = "Cập nhật lớp học phần thành công.", Data = data });
        }
    }
}
