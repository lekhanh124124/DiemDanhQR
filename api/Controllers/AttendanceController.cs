// File: Controllers/AttendanceController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.DTOs;
using api.Helpers;
using api.Services.Interfaces;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _svc;
        public AttendanceController(IAttendanceService svc) => _svc = svc;

        // POST: /api/attendance/qr
        [HttpPost("qr")]
        [Authorize(Roles = "GV,ADMIN")]
        public async Task<ActionResult<ApiResponse<CreateQrResponse>>> CreateQr([FromQuery] CreateQrRequest req)
        {
            var username = JwtHelper.GetUsername(User);
            var data = await _svc.CreateQrAsync(req, username);
            return Ok(new ApiResponse<CreateQrResponse> { Status = "200", Message = "OK", Data = data });
        }

        // POST: /api/attendance/checkin
        [HttpPost("checkin")]
        [Authorize(Roles = "SV")]
        public async Task<ActionResult<ApiResponse<CreateAttendanceResponse>>> CheckInByQr([FromQuery] CheckInRequest req)
        {
            var username = JwtHelper.GetUsername(User);
            var data = await _svc.CheckInByQrAsync(req, username);
            return Ok(new ApiResponse<CreateAttendanceResponse> { Status = "200", Message = "Điểm danh thành công.", Data = data });
        }

        // GET: /api/attendance/statuses
        [HttpGet("statuses")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PagedResult<AttendanceStatusListItem>>>> GetStatuses([FromQuery] AttendanceStatusListRequest req)
        {
            var data = await _svc.GetStatusListAsync(req);
            return Ok(new ApiResponse<PagedResult<AttendanceStatusListItem>> { Status = "200", Message = "OK", Data = data });
        }

        // GET: /api/attendance/records
        [HttpGet("records")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PagedResult<AttendanceListItem>>>> GetAttendances([FromQuery] AttendanceListRequest req)
        {
            var data = await _svc.GetAttendanceListAsync(req);
            return Ok(new ApiResponse<PagedResult<AttendanceListItem>> { Status = "200", Message = "OK", Data = data });
        }

        // POST: /api/attendance/create-record
        [HttpPost("create-record")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<CreateAttendanceResponse>>> CreateAttendance([FromForm] CreateAttendanceRequest req)
        {
            var username = JwtHelper.GetUsername(User);
            var data = await _svc.CreateAttendanceAsync(req, username);
            return Ok(new ApiResponse<CreateAttendanceResponse> { Status = "200", Message = "OK", Data = data });
        }

        // PUT: /api/attendance/update-record 
        [HttpPut("update-record")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UpdateAttendanceResponse>>> UpdateAttendance([FromForm] UpdateAttendanceRequest req)
        {
            var username = JwtHelper.GetUsername(User);
            var data = await _svc.UpdateAttendanceAsync(req, username);
            return Ok(new ApiResponse<UpdateAttendanceResponse> { Status = "200", Message = "OK", Data = data });
        }

        // POST: /api/attendance/create-status
        [HttpPost("create-status")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<AttendanceStatusListItem>>> CreateStatus([FromForm] CreateAttendanceStatusRequest req)
        {
            var data = await _svc.CreateStatusAsync(req);
            return Ok(new ApiResponse<AttendanceStatusListItem> { Status = "200", Message = "OK", Data = data });
        }

        // PUT: /api/attendance/update-status
        [HttpPut("update-status")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<AttendanceStatusListItem>>> UpdateStatus([FromForm] UpdateAttendanceStatusRequest req)
        {
            var data = await _svc.UpdateStatusAsync(req);
            return Ok(new ApiResponse<AttendanceStatusListItem> { Status = "200", Message = "OK", Data = data });
        }

        // DELETE: /api/attendance/delete-status
        [HttpDelete("delete-status")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteStatus([FromQuery] int MaTrangThai)
        {
            var ok = await _svc.DeleteStatusAsync(MaTrangThai);
            return Ok(new ApiResponse<object> { Status = "200", Message = ok ? "Deleted" : "NotFound", Data = new { MaTrangThai } });
        }

        [HttpGet("ratio-by-khoa")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AttendanceFacultyRatioItem>>>> GetRatioByKhoa([FromQuery] int? MaHocKy)
        {
            var data = await _svc.GetFacultyAttendanceRatioAsync(MaHocKy);
            return Ok(new ApiResponse<IEnumerable<AttendanceFacultyRatioItem>>
            {
                Status = "200",
                Message = "OK",
                Data = data
            });
        }

        [HttpGet("ratio-by-lophocphan/gv")]
        [Authorize(Roles = "GV")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AttendanceLopHocPhanRatioItem>>>> GetRatioByLopHocPhanForGiangVien([FromQuery] int? MaHocKy)
        {
            var username = JwtHelper.GetUsername(User);
            var data = await _svc.GetTeacherAttendanceRatioAsync(username, MaHocKy);
            return Ok(new ApiResponse<IEnumerable<AttendanceLopHocPhanRatioItem>>
            {
                Status = "200",
                Message = "OK",
                Data = data
            });
        }

        [HttpGet("ratio-by-lophocphan/sv")]
        [Authorize(Roles = "SV")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AttendanceLopHocPhanRatioItem>>>> GetRatioByLopHocPhanForSinhVien([FromQuery] int? MaHocKy)
        {
            var username = JwtHelper.GetUsername(User);
            var data = await _svc.GetStudentAttendanceRatioAsync(username, MaHocKy);
            return Ok(new ApiResponse<IEnumerable<AttendanceLopHocPhanRatioItem>>
            {
                Status = "200",
                Message = "OK",
                Data = data
            });
        }

    }
}
