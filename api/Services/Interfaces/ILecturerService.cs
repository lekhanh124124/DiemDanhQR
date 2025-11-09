// File: Services/Interfaces/ILecturerService.cs
using api.DTOs;

namespace api.Services.Interfaces
{
    public interface ILecturerService
    {
        Task<CreateLecturerResponse> CreateAsync(CreateLecturerRequest request);
        Task<PagedResult<LecturerListItemResponse>> GetListAsync(GetLecturersRequest request);
        Task<UpdateLecturerResponse> UpdateAsync(UpdateLecturerRequest request);
    }
}
