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
        Task<PagedResult<SubjectListItem>> GetSubjectsAsync(SubjectListRequest req);
        Task<CreateSubjectResponse> CreateSubjectAsync(CreateSubjectRequest req, string? currentUserId);
        Task<CreateCourseResponse> CreateCourseAsync(CreateCourseRequest req, string? currentUserId);

        // Semesters
        Task<PagedResult<SemesterListItem>> GetSemestersAsync(SemesterListRequest req);
        Task<CreateSemesterResponse> CreateSemesterAsync(CreateSemesterRequest req, string? currentUserId);
        Task<UpdateSemesterResponse> UpdateSemesterAsync(UpdateSemesterRequest req, string? currentUserId);

        // Updates
        Task<UpdateSubjectResponse> UpdateSubjectAsync(UpdateSubjectRequest req, string? currentUserId);
        Task<UpdateCourseResponse> UpdateCourseAsync(UpdateCourseRequest req, string? currentUserId);
    }
}
