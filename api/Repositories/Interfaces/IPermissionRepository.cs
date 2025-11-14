using api.Models;

namespace api.Repositories.Interfaces
{
    public interface IPermissionRepository
    {
        // Roles
        Task<(List<PhanQuyen> Items, int Total)> SearchAsync(
            int? maQuyen, string? codeQuyen, string? tenQuyen, string? moTa, int? maChucNang,
            string? sortBy, bool desc, int page, int pageSize);

        Task<bool> RoleCodeExistsAsync(string codeQuyen, int? excludeId = null);
        Task<PhanQuyen?> GetRoleByIdAsync(int maQuyen);
        Task<PhanQuyen?> GetRoleByCodeAsync(string codeQuyen);
        Task AddRoleAsync(PhanQuyen role);
        Task UpdateRoleAsync(PhanQuyen role);
        Task DeleteRoleAsync(PhanQuyen role);
        Task<int> CountUsersByRoleAsync(int maQuyen);
        Task<bool> AnyRoleFunctionMappingsAsync(int maQuyen);

        // Functions
        Task<(List<ChucNang> Items, int Total)> SearchFunctionsAsync(
            int? maChucNang, string? codeChucNang, string? tenChucNang, string? moTa,
            int? maQuyen, int? parentChucNangId, string? sortBy, bool desc, int page, int pageSize);
        Task<bool> FunctionCodeExistsAsync(string codeChucNang, int? excludeId = null);
        Task<ChucNang?> GetFunctionByIdAsync(int maChucNang);
        Task<ChucNang?> GetFunctionByCodeAsync(string codeChucNang);
        Task AddFunctionAsync(ChucNang fn);
        Task UpdateFunctionAsync(ChucNang fn);
        Task DeleteFunctionAsync(ChucNang fn);
        Task<bool> AnyFunctionRoleMappingsAsync(int maChucNang);
        Task<bool> AnyFunctionChildrenAsync(int maChucNang);

        // Role-Function mappings (NhomChucNang có TrangThai)
        Task<bool> RoleFunctionExistsAsync(int maQuyen, int maChucNang);
        Task AddRoleFunctionAsync(NhomChucNang mapping);
        Task DeleteRoleFunctionAsync(int maQuyen, int maChucNang);
        Task UpdateRoleFunctionStatusAsync(int maQuyen, int maChucNang, bool trangThai);
        Task<NhomChucNang?> GetRoleFunctionAsync(int maQuyen, int maChucNang);

        // Activity log by username
        Task LogActivityAsync(string? tenDangNhap, string hanhDong);

        Task<List<PhanQuyen>> GetAllRolesAsync();
        Task<List<ChucNang>> GetAllFunctionsAsync();
        // Bulk insert mappings
        Task AddRoleFunctionsBulkAsync(IEnumerable<NhomChucNang> mappings);

        // Tìm danh sách mapping (join ra Role + Function)
        Task<(List<(PhanQuyen Role, ChucNang Func, NhomChucNang Map)> Items, int Total)> SearchRoleFunctionsAsync(
            int? maQuyen, int? maChucNang,
            string? sortBy, bool desc, int page, int pageSize);
    }
}
