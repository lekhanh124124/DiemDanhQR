// File: Services/Interfaces/IUserService.cs
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;

namespace DiemDanhQR_API.Services.Interfaces
{
    public interface IUserService
    {
        Task<CreateUsertResponse> CreateAsync(CreateUserRequest request);
        Task<object> GetInfoAsync(string maNguoiDung);
        Task<PagedResult<UserActivityItem>> GetActivityAsync(UserActivityListRequest req);
    }
}
