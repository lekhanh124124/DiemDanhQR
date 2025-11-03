// File: ScheduleController.cs
// Bảng BuoiHoc + PhongHoc
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
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _svc;
        public ScheduleController(IScheduleService svc) => _svc = svc;

        // GET: /api/schedule/list
        [HttpGet("list")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PagedResult<ScheduleListItem>>>> GetList([FromQuery] ScheduleListRequest req)
        {
            var data = await _svc.GetListAsync(req);

            return Ok(new ApiResponse<PagedResult<ScheduleListItem>>
            {
                Status = 200,
                Message = "Lấy danh sách buổi học thành công.",
                Data = data
            });
        }

        // GET: /api/schedule/rooms
        [HttpGet("rooms")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PagedResult<RoomListItem>>>> GetRooms([FromQuery] RoomListRequest req)
        {
            var data = await _svc.GetRoomsAsync(req);
            return Ok(new ApiResponse<PagedResult<RoomListItem>>
            {
                Status = 200,
                Message = "Lấy danh sách phòng học thành công.",
                Data = data
            });
        }

        [HttpPost("create-room")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<CreateRoomResponse>>> CreateRoom([FromForm] CreateRoomRequest req)
        {
            var currentUserId = HelperFunctions.GetUserIdFromClaims(User);
            var data = await _svc.CreateRoomAsync(req, currentUserId);

            return Ok(new ApiResponse<CreateRoomResponse>
            {
                Status = 200,
                Message = "Tạo phòng học thành công.",
                Data = data
            });
        }

        [HttpPost("create-schedule")]
        [Authorize(Roles = "ADMIN,GV")]
        public async Task<ActionResult<ApiResponse<CreateScheduleResponse>>> CreateSchedule([FromForm] CreateScheduleRequest req)
        {
            var currentUserId = HelperFunctions.GetUserIdFromClaims(User);
            var data = await _svc.CreateScheduleAsync(req, currentUserId);

            return Ok(new ApiResponse<CreateScheduleResponse>
            {
                Status = 200,
                Message = "Tạo buổi học thành công.",
                Data = data
            });
        }

        [HttpPut("update-room")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<UpdateRoomResponse>>> UpdateRoom([FromForm] UpdateRoomRequest req)
        {
            var currentUserId = HelperFunctions.GetUserIdFromClaims(User);
            var data = await _svc.UpdateRoomAsync(req, currentUserId);

            return Ok(new ApiResponse<UpdateRoomResponse>
            {
                Status = 200,
                Message = "Cập nhật phòng học thành công.",
                Data = data
            });
        }

        [HttpPut("update-schedule")]
        [Authorize(Roles = "ADMIN,GV")]
        public async Task<ActionResult<ApiResponse<UpdateScheduleResponse>>> UpdateSchedule([FromForm] UpdateScheduleRequest req)
        {
            var currentUserId = HelperFunctions.GetUserIdFromClaims(User);
            var data = await _svc.UpdateScheduleAsync(req, currentUserId);

            return Ok(new ApiResponse<UpdateScheduleResponse>
            {
                Status = 200,
                Message = "Cập nhật buổi học thành công.",
                Data = data
            });
        }
    }
}
