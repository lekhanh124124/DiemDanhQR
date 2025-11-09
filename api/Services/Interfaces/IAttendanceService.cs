// File: Services/Interfaces/IAttendanceService.cs
using api.DTOs;

namespace api.Services.Interfaces
{
    public interface IAttendanceService
    {
        Task<CreateQrResponse> CreateQrAsync(CreateQrRequest req, string? currentUsername);
        Task<CreateAttendanceResponse> CheckInByQrAsync(CheckInRequest req, string? currentUsername);

        Task<PagedResult<AttendanceStatusListItem>> GetStatusListAsync(AttendanceStatusListRequest req);
        Task<PagedResult<AttendanceListItem>> GetAttendanceListAsync(AttendanceListRequest req);

        Task<AttendanceStatusListItem> CreateStatusAsync(CreateAttendanceStatusRequest req);
        Task<AttendanceStatusListItem> UpdateStatusAsync(UpdateAttendanceStatusRequest req);
        Task<bool> DeleteStatusAsync(int maTrangThai);

        Task<CreateAttendanceResponse> CreateAttendanceAsync(CreateAttendanceRequest req, string? currentUsername);
        Task<UpdateAttendanceResponse> UpdateAttendanceAsync(UpdateAttendanceRequest req, string? currentUsername);
    }
}
