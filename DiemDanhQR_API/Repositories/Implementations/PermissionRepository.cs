// File: Repositories/Implementations/PermissionRepository.cs
using DiemDanhQR_API.Data;
using DiemDanhQR_API.Models;
using DiemDanhQR_API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DiemDanhQR_API.Repositories.Implementations
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly AppDbContext _db;
        public PermissionRepository(AppDbContext db) => _db = db;

        public async Task<(List<PhanQuyen> Items, int Total)> SearchAsync(
            // string? keyword, // removed
            int? maQuyen,
            string? codeQuyen,
            string? tenQuyen,
            string? moTa,
            int? maChucNang,
            string? sortBy,
            bool desc,
            int page,
            int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

            var q = _db.PhanQuyen.AsNoTracking().AsQueryable();

            if (maQuyen.HasValue)
                q = q.Where(x => x.MaQuyen == maQuyen.Value);

            if (!string.IsNullOrWhiteSpace(codeQuyen))
                q = q.Where(x => x.CodeQuyen == codeQuyen);

            if (!string.IsNullOrWhiteSpace(tenQuyen))
                q = q.Where(x => x.TenQuyen!.Contains(tenQuyen));

            if (!string.IsNullOrWhiteSpace(moTa))
                q = q.Where(x => (x.MoTa ?? "").Contains(moTa));

            // 🔹 Mới: chỉ lấy các quyền có gán MaChucNang trong NhomChucNang
            if (maChucNang.HasValue)
            {
                var fnId = maChucNang.Value;
                q = q.Where(role => _db.NhomChucNang.AsNoTracking()
                            .Any(n => n.MaQuyen == role.MaQuyen && n.MaChucNang == fnId));
            }

            var key = (sortBy ?? "MaQuyen").Trim().ToLowerInvariant();
            q = key switch
            {
                "codequyen" => (desc ? q.OrderByDescending(x => x.CodeQuyen) : q.OrderBy(x => x.CodeQuyen)),
                "tenquyen" => (desc ? q.OrderByDescending(x => x.TenQuyen) : q.OrderBy(x => x.TenQuyen)),
                "mota" => (desc ? q.OrderByDescending(x => x.MoTa) : q.OrderBy(x => x.MoTa)),
                _ => (desc ? q.OrderByDescending(x => x.MaQuyen) : q.OrderBy(x => x.MaQuyen)),
            };

            var total = await q.CountAsync();
            var items = await q.Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

            return (items, total);
        }


        public async Task<(List<ChucNang> Items, int Total)> SearchFunctionsAsync(
            // string? keyword, // removed
            int? maChucNang,
            string? codeChucNang,
            string? tenChucNang,
            string? moTa,
            bool? trangThai,
            int? maQuyen,
            string? sortBy,
            bool desc,
            int page,
            int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

            var q = _db.ChucNang.AsNoTracking().AsQueryable();

            if (maChucNang.HasValue)
                q = q.Where(x => x.MaChucNang == maChucNang.Value);

            if (!string.IsNullOrWhiteSpace(codeChucNang))
                q = q.Where(x => x.CodeChucNang == codeChucNang);

            if (!string.IsNullOrWhiteSpace(tenChucNang))
                q = q.Where(x => x.TenChucNang!.Contains(tenChucNang));

            if (!string.IsNullOrWhiteSpace(moTa))
                q = q.Where(x => (x.MoTa ?? "").Contains(moTa));

            if (trangThai.HasValue)
                q = q.Where(x => (x.TrangThai ?? true) == trangThai.Value);

            if (maQuyen.HasValue)
            {
                var roleId = maQuyen.Value;
                q = q.Where(cn => _db.NhomChucNang.Any(nc => nc.MaChucNang == cn.MaChucNang && nc.MaQuyen == roleId));
            }

            var key = (sortBy ?? "MaChucNang").Trim().ToLowerInvariant();
            q = key switch
            {
                "codechucnang" => (desc ? q.OrderByDescending(x => x.CodeChucNang) : q.OrderBy(x => x.CodeChucNang)),
                "tenchucnang" => (desc ? q.OrderByDescending(x => x.TenChucNang) : q.OrderBy(x => x.TenChucNang)),
                "mota" => (desc ? q.OrderByDescending(x => x.MoTa) : q.OrderBy(x => x.MoTa)),
                "trangthai" => (desc ? q.OrderByDescending(x => x.TrangThai) : q.OrderBy(x => x.TrangThai)),
                "machucnang" or _ => (desc ? q.OrderByDescending(x => x.MaChucNang) : q.OrderBy(x => x.MaChucNang)),
            };

            var total = await q.CountAsync();

            var items = await q.Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

            return (items, total);
        }
    }
}
