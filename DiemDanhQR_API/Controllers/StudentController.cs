// File: Controllers/StudentController.cs
// Bảng: SinhVien
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;
using DiemDanhQR_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DiemDanhQR_API.Helpers;

namespace DiemDanhQR_API.Controllers
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
            req.MaNguoiDung ??= req.MaSinhVien; // mặc định
            var data = await _svc.CreateAsync(req); // service sẽ tự xử lý file
            return Ok(new ApiResponse<CreateStudentResponse> { Status = 200, Message = "Tạo sinh viên thành công.", Data = data });

        }

        [HttpGet("List")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PagedResult<StudentListItemResponse>>>> List([FromQuery] GetStudentsRequest req)
        {
            var result = await _svc.GetListAsync(req);
            return Ok(new ApiResponse<PagedResult<StudentListItemResponse>>
            {
                Status = 200,
                Message = "Lấy danh sách sinh viên thành công.",
                Data = result
            });
        }

        [HttpPut("update")]
        [Authorize(Roles = "ADMIN")]
        [RequestSizeLimit(5_000_000)]
        public async Task<ActionResult<ApiResponse<UpdateStudentResponse>>> Update([FromForm] UpdateStudentRequest req, IFormFile? Avatar)
        {
            var data = await _svc.UpdateAsync(req); // service sẽ tự xử lý file
            return Ok(new ApiResponse<UpdateStudentResponse> { Status = 200, Message = "Cập nhật sinh viên thành công.", Data = data });

        }
    }


}
