// File: Services/Interfaces/IScheduleService.cs
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;

namespace DiemDanhQR_API.Services.Interfaces
{
    public interface IScheduleService
    {
        Task<PagedResult<ScheduleListItem>> GetListAsync(ScheduleListRequest req);
        Task<PagedResult<RoomListItem>> GetRoomsAsync(RoomListRequest req);
    }
}
