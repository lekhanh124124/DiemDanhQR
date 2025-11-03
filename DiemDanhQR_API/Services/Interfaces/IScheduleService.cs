// File: Services/Interfaces/IScheduleService.cs
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;

namespace DiemDanhQR_API.Services.Interfaces
{
    public interface IScheduleService
    {
        Task<PagedResult<ScheduleListItem>> GetListAsync(ScheduleListRequest req);
        Task<PagedResult<RoomListItem>> GetRoomsAsync(RoomListRequest req);
        Task<CreateRoomResponse> CreateRoomAsync(CreateRoomRequest req, string? currentUserId);
        Task<CreateScheduleResponse> CreateScheduleAsync(CreateScheduleRequest req, string? currentUserId);

        // Update
        Task<UpdateRoomResponse> UpdateRoomAsync(UpdateRoomRequest req, string? currentUserId);
        Task<UpdateScheduleResponse> UpdateScheduleAsync(UpdateScheduleRequest req, string? currentUserId);


    }
}
