// File: Controllers/StudentController.cs
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;
using DiemDanhQR_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiemDanhQR_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _svc;
        public StudentController(IStudentService svc) => _svc = svc;

        [HttpPost("Create")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<CreateStudentResponse>>> Create([FromBody] CreateStudentRequest req)
        {
            var result = await _svc.CreateAsync(req);
            return Ok(new ApiResponse<CreateStudentResponse>
            {
                Status = 200,
                Message = "Tạo sinh viên thành công.",
                Data = result
            });
        }

        [HttpGet("List")]
        [Authorize(Roles = "ADMIN")]
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
    }


}
