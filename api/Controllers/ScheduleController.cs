// File: Controllers/ScheduleController.cs
// Bảng: BuoiHoc + PhongHoc
using api.DTOs;
using api.ErrorHandling;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _svc;
        public ScheduleController(IScheduleService svc) => _svc = svc;

        private string inputResponse(string input) => input ?? "null";

        [HttpGet("list")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PagedResult<ScheduleListItem>>>> GetList([FromQuery] ScheduleListRequest req)
        {
            var data = await _svc.GetListAsync(req);

            // PagedResult trong dự án dùng string -> đảm bảo đổ string
            var shaped = new PagedResult<ScheduleListItem>
            {
                Page = inputResponse(data.Page),
                PageSize = inputResponse(data.PageSize),
                TotalRecords = inputResponse(data.TotalRecords),
                TotalPages = inputResponse(data.TotalPages),
                Items = data.Items
            };

            return Ok(new ApiResponse<PagedResult<ScheduleListItem>>
            {
                Status = inputResponse("200"),
                Message = inputResponse("Lấy danh sách buổi học thành công."),
                Data = shaped
            });
        }

        [HttpGet("rooms")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PagedResult<RoomListItem>>>> GetRooms([FromQuery] RoomListRequest req)
        {
            var data = await _svc.GetRoomsAsync(req);

            var shaped = new PagedResult<RoomListItem>
            {
                Page = inputResponse(data.Page),
                PageSize = inputResponse(data.PageSize),
                TotalRecords = inputResponse(data.TotalRecords),
                TotalPages = inputResponse(data.TotalPages),
                Items = data.Items
            };

            return Ok(new ApiResponse<PagedResult<RoomListItem>>
            {
                Status = inputResponse("200"),
                Message = inputResponse("Lấy danh sách phòng học thành công."),
                Data = shaped
            });
        }

        [HttpPost("create-room")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<CreateRoomResponse>>> CreateRoom([FromForm] CreateRoomRequest req)
        {
            var tenDangNhap = User.FindFirst("TenDangNhap")?.Value
                              ?? User.FindFirst(ClaimTypes.Name)?.Value;

            var data = await _svc.CreateRoomAsync(req, tenDangNhap);

            return Ok(new ApiResponse<CreateRoomResponse>
            {
                Status = inputResponse("200"),
                Message = inputResponse("Tạo phòng học thành công."),
                Data = data
            });
        }

        [HttpPost("create-schedule")]
        [Authorize(Roles = "ADMIN,GV")]
        public async Task<ActionResult<ApiResponse<CreateScheduleResponse>>> CreateSchedule([FromForm] CreateScheduleRequest req)
        {
            var tenDangNhap = User.FindFirst("TenDangNhap")?.Value
                              ?? User.FindFirst(ClaimTypes.Name)?.Value;

            var data = await _svc.CreateScheduleAsync(req, tenDangNhap);

            return Ok(new ApiResponse<CreateScheduleResponse>
            {
                Status = inputResponse("200"),
                Message = inputResponse("Tạo buổi học thành công."),
                Data = data
            });
        }

        [HttpPut("update-room")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<UpdateRoomResponse>>> UpdateRoom([FromForm] UpdateRoomRequest req)
        {
            var tenDangNhap = User.FindFirst("TenDangNhap")?.Value
                              ?? User.FindFirst(ClaimTypes.Name)?.Value;

            var data = await _svc.UpdateRoomAsync(req, tenDangNhap);

            return Ok(new ApiResponse<UpdateRoomResponse>
            {
                Status = inputResponse("200"),
                Message = inputResponse("Cập nhật phòng học thành công."),
                Data = data
            });
        }

        [HttpPut("update-schedule")]
        [Authorize(Roles = "ADMIN,GV")]
        public async Task<ActionResult<ApiResponse<UpdateScheduleResponse>>> UpdateSchedule([FromForm] UpdateScheduleRequest req)
        {
            var tenDangNhap = User.FindFirst("TenDangNhap")?.Value
                              ?? User.FindFirst(ClaimTypes.Name)?.Value;

            var data = await _svc.UpdateScheduleAsync(req, tenDangNhap);

            return Ok(new ApiResponse<UpdateScheduleResponse>
            {
                Status = inputResponse("200"),
                Message = inputResponse("Cập nhật buổi học thành công."),
                Data = data
            });
        }
        // POST: /api/schedule/auto-generate
        [HttpPost("auto-generate")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<List<ScheduleListItem>>>> AutoGenerate([FromForm] AutoGenerateScheduleRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.MaLopHocPhan))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã lớp học phần là bắt buộc.");

            var tenDangNhap = User.FindFirst("TenDangNhap")?.Value
                              ?? User.FindFirst(ClaimTypes.Name)?.Value;

            var items = await _svc.AutoGenerateAsync(req.MaLopHocPhan!.Trim(), tenDangNhap);

            return Ok(new ApiResponse<List<ScheduleListItem>>
            {
                Status = inputResponse("200"),
                Message = inputResponse("Sinh buổi học tự động thành công."),
                Data = items
            });
        }
    }
}
