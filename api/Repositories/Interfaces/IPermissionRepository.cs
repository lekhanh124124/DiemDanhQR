using api.Models;

namespace api.Repositories.Interfaces
{
    public interface IPermissionRepository
    {
        Task<(List<PhanQuyen> Items, int Total)> SearchAsync(
            int? maQuyen, string? codeQuyen, string? tenQuyen, string? moTa, int? maChucNang,
            string? sortBy, bool desc, int page, int pageSize);

        Task<(List<ChucNang> Items, int Total)> SearchFunctionsAsync(
            int? maChucNang, string? codeChucNang, string? tenChucNang, string? moTa,
            int? maQuyen, string? sortBy, bool desc, int page, int pageSize);

        // Roles
        Task<bool> RoleCodeExistsAsync(string codeQuyen, int? excludeId = null);
        Task<PhanQuyen?> GetRoleByIdAsync(int maQuyen);
        Task<PhanQuyen?> GetRoleByCodeAsync(string codeQuyen);
        Task AddRoleAsync(PhanQuyen role);
        Task UpdateRoleAsync(PhanQuyen role);
        Task DeleteRoleAsync(PhanQuyen role);
        Task<int> CountUsersByRoleAsync(int maQuyen);
        Task<bool> AnyRoleFunctionMappingsAsync(int maQuyen);

        // Functions
        Task<bool> FunctionCodeExistsAsync(string codeChucNang, int? excludeId = null);
        Task<ChucNang?> GetFunctionByIdAsync(int maChucNang);
        Task<ChucNang?> GetFunctionByCodeAsync(string codeChucNang);
        Task AddFunctionAsync(ChucNang fn);
        Task UpdateFunctionAsync(ChucNang fn);
        Task DeleteFunctionAsync(ChucNang fn);
        Task<bool> AnyFunctionRoleMappingsAsync(int maChucNang);

        // Role-Function mappings (NhomChucNang có TrangThai)
        Task<bool> RoleFunctionExistsAsync(int maQuyen, int maChucNang);
        Task AddRoleFunctionAsync(NhomChucNang mapping);
        Task DeleteRoleFunctionAsync(int maQuyen, int maChucNang);
        Task UpdateRoleFunctionStatusAsync(int maQuyen, int maChucNang, bool trangThai);
        Task<NhomChucNang?> GetRoleFunctionAsync(int maQuyen, int maChucNang);

        // Activity log by username
        Task LogActivityAsync(string? tenDangNhap, string hanhDong);
    }
}
