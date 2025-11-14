// File: Controllers/StudentController.cs
// Bảng: SinhVien
using api.DTOs;
using api.Helpers;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _svc;
        private readonly IWebHostEnvironment _env;

        public StudentController(IStudentService svc, IWebHostEnvironment env)
        {
            _svc = svc;
            _env = env;
        }

        [HttpPost("create")]
        [Authorize(Roles = "ADMIN")]
        [RequestSizeLimit(5_000_000)]
        public async Task<ActionResult<ApiResponse<CreateStudentResponse>>> Create([FromForm] CreateStudentRequest req)
        {
            var data = await _svc.CreateAsync(req);
            return Ok(new ApiResponse<CreateStudentResponse> { Status = "200", Message = "Tạo sinh viên thành công.", Data = data });
        }

        [HttpGet("list")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PagedResult<StudentListItemResponse>>>> List([FromQuery] GetStudentsRequest req)
        {
            var result = await _svc.GetListAsync(req);
            return Ok(new ApiResponse<PagedResult<StudentListItemResponse>> { Status = "200", Message = "Lấy danh sách sinh viên thành công.", Data = result });
        }

        [HttpPut("update")]
        [Authorize(Roles = "ADMIN")]
        [RequestSizeLimit(5_000_000)]
        public async Task<ActionResult<ApiResponse<UpdateStudentResponse>>> Update([FromForm] UpdateStudentRequest req)
        {
            var data = await _svc.UpdateAsync(req);
            return Ok(new ApiResponse<UpdateStudentResponse> { Status = "200", Message = "Cập nhật sinh viên thành công.", Data = data });
        }

        // POST: /api/student/add-to-course
        [HttpPost("add-to-course")]
        [Authorize(Roles = "ADMIN,GV")]
        public async Task<ActionResult<ApiResponse<AddStudentToCourseResponse>>> AddToCourse([FromForm] AddStudentToCourseRequest req)
        {
            var currentUser = JwtHelper.GetUsername(User) ?? string.Empty;
            var data = await _svc.AddStudentToCourseAsync(req, currentUser);
            return Ok(new ApiResponse<AddStudentToCourseResponse> { Status = "200", Message = "Thêm sinh viên vào lớp học phần thành công.", Data = data });
        }

        // PUT: /api/student/remove-from-course
        [HttpPut("remove-from-course")]
        [Authorize(Roles = "ADMIN,GV")]
        public async Task<ActionResult<ApiResponse<RemoveStudentFromCourseResponse>>> RemoveFromCourse([FromForm] RemoveStudentFromCourseRequest req)
        {
            var currentUser = JwtHelper.GetUsername(User) ?? string.Empty;
            var data = await _svc.RemoveStudentFromCourseAsync(req, currentUser);
            return Ok(new ApiResponse<RemoveStudentFromCourseResponse> { Status = "200", Message = "Gỡ sinh viên khỏi lớp học phần thành công.", Data = data });
        }

        // POST: /api/student/bulk-import
        [HttpPost("bulk-import")]
        [Authorize(Roles = "ADMIN")]
        [RequestSizeLimit(20_000_000)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<BulkImportStudentsResponse>>> BulkImport([FromForm] BulkImportStudentsRequest req)
        {
            var result = await _svc.BulkImportAsync(req);
            return Ok(new ApiResponse<BulkImportStudentsResponse> { Status = "200", Message = "Import sinh viên hoàn tất.", Data = result });
        }
        
        // POST: /api/student/add-to-course-bulk
        [HttpPost("add-to-course-bulk")]
        [Authorize(Roles = "ADMIN,GV")]
        [RequestSizeLimit(20_000_000)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<BulkImportStudentsResponse>>> AddToCourseBulk([FromForm] BulkAddStudentsToCourseRequest req)
        {
            var currentUser = JwtHelper.GetUsername(User) ?? string.Empty;
            var result = await _svc.BulkAddStudentsToCourseAsync(req, currentUser);
            return Ok(new ApiResponse<BulkImportStudentsResponse> { Status = "200", Message = "Import tham gia lớp hoàn tất.", Data = result });
        }
    }
}
