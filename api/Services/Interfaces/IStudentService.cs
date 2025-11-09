// File: Services/Interfaces/IStudentService.cs
using api.DTOs;

namespace api.Services.Interfaces
{
    public interface IStudentService
    {
        Task<CreateStudentResponse> CreateAsync(CreateStudentRequest request);
        Task<PagedResult<StudentListItemResponse>> GetListAsync(GetStudentsRequest request);
        Task<UpdateStudentResponse> UpdateAsync(UpdateStudentRequest request);

        Task<AddStudentToCourseResponse> AddStudentToCourseAsync(AddStudentToCourseRequest req, string? currentUserTenDangNhap);
        Task<RemoveStudentFromCourseResponse> RemoveStudentFromCourseAsync(RemoveStudentFromCourseRequest req, string? currentUserTenDangNhap);
    }
}
