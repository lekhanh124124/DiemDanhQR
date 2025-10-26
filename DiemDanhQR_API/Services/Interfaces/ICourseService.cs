// File: Services/Interfaces/ICourseService.cs
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;

namespace DiemDanhQR_API.Services.Interfaces
{
    public interface ICourseService
    {
        Task<CourseListResponse> GetListAsync(CourseListRequest req);
    }
}
