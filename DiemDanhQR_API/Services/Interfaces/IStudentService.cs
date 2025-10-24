// File: Services/Interfaces/IStudentService.cs
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;

namespace DiemDanhQR_API.Services.Interfaces
{
    public interface IStudentService
    {
        Task<CreateStudentResponse> CreateAsync(CreateStudentRequest request);
    }
}
