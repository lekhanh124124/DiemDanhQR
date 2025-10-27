// File: Services/Interfaces/IPermissionService.cs
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;

namespace DiemDanhQR_API.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<PagedResult<PermissionListItem>> GetListAsync(PermissionListRequest request);
        Task<PagedResult<FunctionListItem>> GetFunctionListAsync(FunctionListRequest request);
    }
}
