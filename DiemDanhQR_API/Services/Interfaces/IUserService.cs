// File: Services/Interfaces/IUserService.cs
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;

namespace DiemDanhQR_API.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<CreateUsertResponse>> CreateAsync(CreateUserRequest request);
        Task<ApiResponse<object>> GetInfoAsync(GetUserInfoRequest request);
    }
}
