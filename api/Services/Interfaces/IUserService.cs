// File: Services/Interfaces/IUserService.cs
using api.DTOs;

namespace api.Services.Interfaces
{
    public interface IUserService
    {
        Task<CreateUserResponse> CreateAsync(CreateUserRequest request);
        Task<UpdateUserProfileResponse> UpdateProfileAsync(UpdateUserProfileRequest request, string currentUsername);
        Task<PagedResult<UserItem>> GetListAsync(UserListRequest request);
        Task<object> GetInfoAsync(string tenDangNhap);
        Task<PagedResult<UserActivityItem>> GetActivityAsync(UserActivityListRequest req);
    }
}
