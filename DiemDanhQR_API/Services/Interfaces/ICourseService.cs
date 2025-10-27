// File: Services/Interfaces/ICourseService.cs
using Azure;
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiemDanhQR_API.Services.Interfaces
{
    public interface ICourseService
    {
        Task<PagedResult<CourseListItem>> GetListAsync(CourseListRequest req);
        Task<PagedResult<CourseParticipantItem>> GetParticipantsAsync(CourseParticipantsRequest req);
        Task<PagedResult<SubjectListItem>> GetSubjectsAsync(SubjectListRequest req);
    }
}
