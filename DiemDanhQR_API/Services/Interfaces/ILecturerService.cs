// File: Services/Interfaces/ILecturerService.cs
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;

namespace DiemDanhQR_API.Services.Interfaces
{
    public interface ILecturerService
    {
        Task<CreateLecturerResponse> CreateAsync(CreateLecturerRequest request);
        Task<PagedResult<LecturerListItemResponse>> GetListAsync(GetLecturersRequest request);
    }
}
