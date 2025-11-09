// File: Services/Interfaces/ICourseService.cs
using api.DTOs;

namespace api.Services.Interfaces
{
    public interface ICourseService
    {
        Task<PagedResult<CourseListItem>> GetListAsync(CourseListRequest req);
        Task<PagedResult<SubjectListItem>> GetSubjectsAsync(SubjectListRequest req);
        Task<CreateSubjectResponse> CreateSubjectAsync(CreateSubjectRequest req, string? currentUserLogin);
        Task<CreateCourseResponse> CreateCourseAsync(CreateCourseRequest req, string? currentUserLogin);

        Task<PagedResult<SemesterListItem>> GetSemestersAsync(SemesterListRequest req);
        Task<CreateSemesterResponse> CreateSemesterAsync(CreateSemesterRequest req, string? currentUserLogin);
        Task<UpdateSemesterResponse> UpdateSemesterAsync(UpdateSemesterRequest req, string? currentUserLogin);

        Task<UpdateSubjectResponse> UpdateSubjectAsync(UpdateSubjectRequest req, string? currentUserLogin);
        Task<UpdateCourseResponse> UpdateCourseAsync(UpdateCourseRequest req, string? currentUserLogin);
    }
}
