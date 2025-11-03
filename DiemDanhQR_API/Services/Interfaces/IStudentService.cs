// File: Services/Interfaces/IStudentService.cs
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;

namespace DiemDanhQR_API.Services.Interfaces
{
    public interface IStudentService
    {
        Task<CreateStudentResponse> CreateAsync(CreateStudentRequest request);
        Task<PagedResult<StudentListItemResponse>> GetListAsync(GetStudentsRequest request);
        Task<UpdateStudentResponse> UpdateAsync(UpdateStudentRequest request);

        // Moved from CourseService
        Task<AddStudentToCourseResponse> AddStudentToCourseAsync(AddStudentToCourseRequest req, string? currentUserTenDangNhap);

        // New: soft-remove participation
        Task<RemoveStudentFromCourseResponse> RemoveStudentFromCourseAsync(RemoveStudentFromCourseRequest req, string? currentUserTenDangNhap);
    }
}
