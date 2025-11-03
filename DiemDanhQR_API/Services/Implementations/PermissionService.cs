// File: Services/Implementations/PermissionService.cs
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;
using DiemDanhQR_API.Repositories.Interfaces;
using DiemDanhQR_API.Services.Interfaces;

namespace DiemDanhQR_API.Services.Implementations
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _repo;
        public PermissionService(IPermissionRepository repo) => _repo = repo;

        public async Task<PagedResult<PermissionListItem>> GetListAsync(PermissionListRequest request)
        {
            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 200);

            var sortBy = request.SortBy ?? "MaQuyen";
            var sortDir = (request.SortDir ?? "ASC").Trim().ToUpperInvariant();
            var desc = sortDir == "DESC";

            var (items, total) = await _repo.SearchAsync(
                request.MaQuyen,
                request.CodeQuyen,
                request.TenQuyen,
                request.MoTa,
                // 🔹 truyền tham số lọc theo mã chức năng
                request.MaChucNang,
                sortBy,
                desc,
                page,
                pageSize
            );

            var list = items.Select(x => new PermissionListItem(
                x.MaQuyen ?? 0,
                x.CodeQuyen,
                x.TenQuyen,
                x.MoTa
            )).ToList();

            return new PagedResult<PermissionListItem>
            {
                Page = page,
                PageSize = pageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Items = list
            };
        }

        public async Task<PagedResult<FunctionListItem>> GetFunctionListAsync(FunctionListRequest request)
        {
            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 200);

            var sortBy = request.SortBy ?? "MaChucNang";
            var sortDir = (request.SortDir ?? "ASC").Trim().ToUpperInvariant();
            var desc = sortDir == "DESC";

            var (items, total) = await _repo.SearchFunctionsAsync(
                request.MaChucNang,
                request.CodeChucNang,
                request.TenChucNang,
                request.MoTa,
                request.TrangThai,
                request.MaQuyen,
                sortBy,
                desc,
                page,
                pageSize
            );

            var list = items.Select(x => new FunctionListItem(
                (int)x.MaChucNang!,
                x.CodeChucNang,
                x.TenChucNang,
                x.MoTa,
                x.TrangThai ?? true
            )).ToList();

            return new PagedResult<FunctionListItem>
            {
                Page = page,
                PageSize = pageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Items = list
            };
        }
    }
}
