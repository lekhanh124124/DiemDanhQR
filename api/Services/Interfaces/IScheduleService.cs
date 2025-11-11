// File: Services/Interfaces/IScheduleService.cs
using api.DTOs;

namespace api.Services.Interfaces
{
    public interface IScheduleService
    {
        Task<PagedResult<ScheduleListItem>> GetListAsync(ScheduleListRequest req);
        Task<PagedResult<RoomListItem>> GetRoomsAsync(RoomListRequest req);
        Task<CreateRoomResponse> CreateRoomAsync(CreateRoomRequest req, string? tenDangNhap);
        Task<CreateScheduleResponse> CreateScheduleAsync(CreateScheduleRequest req, string? tenDangNhap);
        Task<UpdateRoomResponse> UpdateRoomAsync(UpdateRoomRequest req, string? tenDangNhap);
        Task<UpdateScheduleResponse> UpdateScheduleAsync(UpdateScheduleRequest req, string? tenDangNhap);
        Task<List<ScheduleListItem>> AutoGenerateAsync(string maLopHocPhan, string? tenDangNhap);
    }
}
