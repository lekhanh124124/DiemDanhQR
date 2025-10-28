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
    }
}
