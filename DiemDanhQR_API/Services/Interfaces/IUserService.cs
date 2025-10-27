// File: Services/Interfaces/IUserService.cs
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;

namespace DiemDanhQR_API.Services.Interfaces
{
    public interface IUserService
    {
        Task<CreateUserResponse> CreateAsync(CreateUserRequest request);
        Task<object> GetInfoAsync(string maNguoiDung);
        Task<PagedResult<UserActivityItem>> GetActivityAsync(UserActivityListRequest req);
        Task<UpdateUserProfileResponse> UpdateProfileAsync(string maNguoiDungFromToken, UpdateUserProfileRequest req);

    }
}
