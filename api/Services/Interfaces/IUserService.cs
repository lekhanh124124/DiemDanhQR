using api.DTOs.Requests;
using api.DTOs.Responses;

namespace api.Services.Interfaces
{
    public interface IUserService
    {
        Task<CreateUserResponse> CreateAsync(CreateUserRequest request);
        Task<object> GetInfoAsync(string tenDangNhap);
        Task<PagedResult<UserActivityItem>> GetActivityAsync(UserActivityListRequest req);
    }
}
