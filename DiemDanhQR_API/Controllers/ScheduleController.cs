// File: ScheduleController.cs
// Bảng BuoiHoc + PhongHoc
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;
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

    }
}
