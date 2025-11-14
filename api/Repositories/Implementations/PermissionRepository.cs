using api.Data;
using api.Helpers;
using api.Models;
using api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories.Implementations
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly AppDbContext _db;
        public PermissionRepository(AppDbContext db) => _db = db;

        public async Task<(List<PhanQuyen> Items, int Total)> SearchAsync(
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

            if (maQuyen.HasValue) q = q.Where(x => x.MaQuyen == maQuyen.Value);
            if (!string.IsNullOrWhiteSpace(codeQuyen)) q = q.Where(x => x.CodeQuyen == codeQuyen);
            if (!string.IsNullOrWhiteSpace(tenQuyen)) q = q.Where(x => x.TenQuyen!.Contains(tenQuyen));
            if (!string.IsNullOrWhiteSpace(moTa)) q = q.Where(x => (x.MoTa ?? "").Contains(moTa));

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
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }

        public async Task<(List<ChucNang> Items, int Total)> SearchFunctionsAsync(
            int? maChucNang,
            string? codeChucNang,
            string? tenChucNang,
            string? moTa,
            int? maQuyen,
            int? parentChucNangId,       // ⭐ NEW
            string? sortBy,
            bool desc,
            int page,
            int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

            var q = _db.ChucNang.AsNoTracking().AsQueryable();

            if (maChucNang.HasValue) q = q.Where(x => x.MaChucNang == maChucNang.Value);
            if (!string.IsNullOrWhiteSpace(codeChucNang)) q = q.Where(x => x.CodeChucNang == codeChucNang);
            if (!string.IsNullOrWhiteSpace(tenChucNang)) q = q.Where(x => x.TenChucNang!.Contains(tenChucNang));
            if (!string.IsNullOrWhiteSpace(moTa)) q = q.Where(x => (x.MoTa ?? "").Contains(moTa));

            // ⭐ Lọc theo Parent (null = root; nếu không truyền thì bỏ qua)
            if (parentChucNangId.HasValue)
            {
                var pid = parentChucNangId.Value;
                q = q.Where(x => x.ParentChucNangId == pid);
            }

            if (maQuyen.HasValue)
            {
                var roleId = maQuyen.Value;
                q = q.Where(cn => _db.NhomChucNang.AsNoTracking()
                            .Any(nc => nc.MaChucNang == cn.MaChucNang && nc.MaQuyen == roleId));
            }

            var key = (sortBy ?? "MaChucNang").Trim().ToLowerInvariant();
            q = key switch
            {
                "codechucnang" => (desc ? q.OrderByDescending(x => x.CodeChucNang) : q.OrderBy(x => x.CodeChucNang)),
                "tenchucnang" => (desc ? q.OrderByDescending(x => x.TenChucNang) : q.OrderBy(x => x.TenChucNang)),
                "mota" => (desc ? q.OrderByDescending(x => x.MoTa) : q.OrderBy(x => x.MoTa)),
                _ => (desc ? q.OrderByDescending(x => x.MaChucNang) : q.OrderBy(x => x.MaChucNang)),
            };

            var total = await q.CountAsync();
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }

        public async Task<bool> RoleCodeExistsAsync(string codeQuyen, int? excludeId = null)
        {
            var code = (codeQuyen ?? "").Trim().ToLower();
            var q = _db.PhanQuyen.AsNoTracking().Where(x => (x.CodeQuyen ?? "").ToLower() == code);
            if (excludeId.HasValue) q = q.Where(x => x.MaQuyen != excludeId.Value);
            return await q.AnyAsync();
        }

        public Task<PhanQuyen?> GetRoleByIdAsync(int maQuyen)
            => _db.PhanQuyen.FirstOrDefaultAsync(x => x.MaQuyen == maQuyen);

        public Task<PhanQuyen?> GetRoleByCodeAsync(string codeQuyen)
        {
            var code = (codeQuyen ?? "").Trim();
            return _db.PhanQuyen.FirstOrDefaultAsync(x => (x.CodeQuyen ?? "") == code);
        }

        public Task<ChucNang?> GetFunctionByCodeAsync(string codeChucNang)
        {
            var code = (codeChucNang ?? "").Trim();
            return _db.ChucNang.FirstOrDefaultAsync(x => (x.CodeChucNang ?? "") == code);
        }

        public async Task<bool> RoleFunctionExistsAsync(int maQuyen, int maChucNang)
            => await _db.NhomChucNang.AsNoTracking().AnyAsync(x => x.MaQuyen == maQuyen && x.MaChucNang == maChucNang);

        public async Task<NhomChucNang?> GetRoleFunctionAsync(int maQuyen, int maChucNang)
            => await _db.NhomChucNang.AsNoTracking()
                   .FirstOrDefaultAsync(x => x.MaQuyen == maQuyen && x.MaChucNang == maChucNang);

        public async Task AddRoleFunctionAsync(NhomChucNang mapping)
        {
            _db.NhomChucNang.Add(mapping);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteRoleFunctionAsync(int maQuyen, int maChucNang)
        {
            var entity = await _db.NhomChucNang
                .FirstOrDefaultAsync(x => x.MaQuyen == maQuyen && x.MaChucNang == maChucNang);
            if (entity != null)
            {
                _db.NhomChucNang.Remove(entity);
                await _db.SaveChangesAsync();
            }
        }

        public async Task UpdateRoleFunctionStatusAsync(int maQuyen, int maChucNang, bool trangThai)
        {
            var entity = await _db.NhomChucNang
                .FirstOrDefaultAsync(x => x.MaQuyen == maQuyen && x.MaChucNang == maChucNang);
            if (entity != null)
            {
                entity.TrangThai = trangThai;
                await _db.SaveChangesAsync();
            }
        }

        public async Task AddRoleAsync(PhanQuyen role)
        {
            _db.PhanQuyen.Add(role);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateRoleAsync(PhanQuyen role)
        {
            _db.PhanQuyen.Update(role);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteRoleAsync(PhanQuyen role)
        {
            _db.PhanQuyen.Remove(role);
            await _db.SaveChangesAsync();
        }

        public async Task<int> CountUsersByRoleAsync(int maQuyen)
            => await _db.NguoiDung.AsNoTracking().CountAsync(x => x.MaQuyen == maQuyen);

        public async Task<bool> AnyRoleFunctionMappingsAsync(int maQuyen)
            => await _db.NhomChucNang.AsNoTracking().AnyAsync(x => x.MaQuyen == maQuyen);

        public async Task<bool> FunctionCodeExistsAsync(string codeChucNang, int? excludeId = null)
        {
            var code = (codeChucNang ?? "").Trim().ToLower();
            var q = _db.ChucNang.AsNoTracking().Where(x => (x.CodeChucNang ?? "").ToLower() == code);
            if (excludeId.HasValue) q = q.Where(x => x.MaChucNang != excludeId.Value);
            return await q.AnyAsync();
        }

        public Task<ChucNang?> GetFunctionByIdAsync(int maChucNang)
            => _db.ChucNang.FirstOrDefaultAsync(x => x.MaChucNang == maChucNang);

        public async Task AddFunctionAsync(ChucNang fn)
        {
            _db.ChucNang.Add(fn);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateFunctionAsync(ChucNang fn)
        {
            _db.ChucNang.Update(fn);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteFunctionAsync(ChucNang fn)
        {
            _db.ChucNang.Remove(fn);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> AnyFunctionRoleMappingsAsync(int maChucNang)
            => await _db.NhomChucNang.AsNoTracking().AnyAsync(x => x.MaChucNang == maChucNang);
        public async Task<bool> AnyFunctionChildrenAsync(int maChucNang)
            => await _db.ChucNang.AsNoTracking().AnyAsync(x => x.ParentChucNangId == maChucNang);

        public async Task LogActivityAsync(string? tenDangNhap, string hanhDong)
        {
            if (string.IsNullOrWhiteSpace(tenDangNhap)) return;
            var username = tenDangNhap.Trim();
            var user = await _db.NguoiDung.AsNoTracking().FirstOrDefaultAsync(x => (x.TenDangNhap ?? "") == username);
            if (user == null) return;

            _db.LichSuHoatDong.Add(new LichSuHoatDong
            {
                MaNguoiDung = user.MaNguoiDung,
                HanhDong = hanhDong,
                ThoiGian = TimeHelper.UtcToVietnam(DateTime.UtcNow) // ghi DB: VN time, không format
            });
            await _db.SaveChangesAsync();
        }

        public async Task<List<PhanQuyen>> GetAllRolesAsync()
           => await _db.PhanQuyen.AsNoTracking().ToListAsync();

        public async Task<List<ChucNang>> GetAllFunctionsAsync()
            => await _db.ChucNang.AsNoTracking().ToListAsync();

        public async Task AddRoleFunctionsBulkAsync(IEnumerable<NhomChucNang> mappings)
        {
            if (mappings == null) return;
            _db.NhomChucNang.AddRange(mappings);
            await _db.SaveChangesAsync();
        }

        public async Task<(List<(PhanQuyen Role, ChucNang Func, NhomChucNang Map)> Items, int Total)> SearchRoleFunctionsAsync(
            int? maQuyen, int? maChucNang,
            string? sortBy, bool desc, int page, int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

            var q =
                from map in _db.NhomChucNang.AsNoTracking()
                join r in _db.PhanQuyen.AsNoTracking() on map.MaQuyen equals r.MaQuyen
                join f in _db.ChucNang.AsNoTracking() on map.MaChucNang equals f.MaChucNang
                select new { r, f, map };

            if (maQuyen.HasValue) q = q.Where(x => x.r.MaQuyen == maQuyen.Value);
            if (maChucNang.HasValue) q = q.Where(x => x.f.MaChucNang == maChucNang.Value);

            var key = (sortBy ?? "MaQuyen").Trim().ToLowerInvariant();
            q = key switch
            {
                "codequyen" => (desc ? q.OrderByDescending(x => x.r.CodeQuyen) : q.OrderBy(x => x.r.CodeQuyen)),
                "tenquyen" => (desc ? q.OrderByDescending(x => x.r.TenQuyen) : q.OrderBy(x => x.r.TenQuyen)),
                "machucnang" => (desc ? q.OrderByDescending(x => x.f.MaChucNang) : q.OrderBy(x => x.f.MaChucNang)),
                "codechucnang" => (desc ? q.OrderByDescending(x => x.f.CodeChucNang) : q.OrderBy(x => x.f.CodeChucNang)),
                "tenchucnang" => (desc ? q.OrderByDescending(x => x.f.TenChucNang) : q.OrderBy(x => x.f.TenChucNang)),
                "trangthai" => (desc ? q.OrderByDescending(x => x.map.TrangThai) : q.OrderBy(x => x.map.TrangThai)),
                _ => (desc ? q.OrderByDescending(x => x.r.MaQuyen) : q.OrderBy(x => x.r.MaQuyen)),
            };

            var total = await q.CountAsync();
            var rows = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var items = rows.Select(x => (x.r, x.f, x.map)).ToList();
            return (items, total);
        }
    }
}
