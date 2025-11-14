using api.DTOs;

namespace api.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<PagedResult<PermissionListItem>> GetListAsync(PermissionListRequest request);
        Task<PagedResult<FunctionListItem>> GetFunctionListAsync(FunctionListRequest request);

        // CRUD Roles
        Task<RoleDetailResponse> CreateRoleAsync(CreateRoleRequest req, string? currentUsername);
        Task<RoleDetailResponse> UpdateRoleAsync(UpdateRoleRequest req, string? currentUsername);
        Task<bool> DeleteRoleAsync(int maQuyen, string? currentUsername);

        // CRUD Functions
        Task<FunctionDetailResponse> CreateFunctionAsync(CreateFunctionRequest req, string? currentUsername);
        Task<FunctionDetailResponse> UpdateFunctionAsync(UpdateFunctionRequest req, string? currentUsername);
        Task<bool> DeleteFunctionAsync(int maChucNang, string? currentUsername);

        // CRUD Role-Function by Codes
        Task<RoleFunctionDetailResponse> CreateRoleFunctionByCodeAsync(CreateRoleFunctionByCodeRequest req, string? currentUsername);
        Task<RoleFunctionDetailResponse> UpdateRoleFunctionByCodeAsync(UpdateRoleFunctionByCodeRequest req, string? currentUsername);
        Task<bool> DeleteRoleFunctionByCodeAsync(int maQuyen, int maChucNang, string? currentUsername);
        Task<PagedResult<RoleFunctionListItem>> GetRoleFunctionListAsync(RoleFunctionListRequest request);

    }
}
