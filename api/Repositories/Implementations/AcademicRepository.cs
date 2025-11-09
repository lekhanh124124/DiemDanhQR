// File: Repositories/Implementations/AcademicRepository.cs
using api.Data;
using api.Models;
using api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories.Implementations
{
    public class AcademicRepository : IAcademicRepository
    {
        private readonly AppDbContext _db;
        public AcademicRepository(AppDbContext db) => _db = db;

        // ===== KHOA =====
        public async Task<(List<Khoa> Items, int Total)> SearchKhoaAsync(
            int? maKhoa, string? codeKhoa, string? tenKhoa,
            string? sortBy, bool desc, int page, int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

            var q = _db.Khoa.AsNoTracking().AsQueryable();

            if (maKhoa.HasValue) q = q.Where(x => x.MaKhoa == maKhoa.Value);
            if (!string.IsNullOrWhiteSpace(codeKhoa)) q = q.Where(x => x.CodeKhoa == codeKhoa);
            if (!string.IsNullOrWhiteSpace(tenKhoa)) q = q.Where(x => x.TenKhoa.Contains(tenKhoa));

            var key = (sortBy ?? "MaKhoa").Trim().ToLowerInvariant();
            q = key switch
            {
                "codekhoa" => (desc ? q.OrderByDescending(x => x.CodeKhoa) : q.OrderBy(x => x.CodeKhoa)),
                "tenkhoa" => (desc ? q.OrderByDescending(x => x.TenKhoa) : q.OrderBy(x => x.TenKhoa)),
                _ => (desc ? q.OrderByDescending(x => x.MaKhoa) : q.OrderBy(x => x.MaKhoa))
            };

            var total = await q.CountAsync();
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }

        public async Task<bool> KhoaCodeExistsAsync(string codeKhoa, int? excludeId = null)
        {
            var code = (codeKhoa ?? "").Trim().ToLower();
            var q = _db.Khoa.AsNoTracking().Where(x => (x.CodeKhoa ?? "").ToLower() == code);
            if (excludeId.HasValue) q = q.Where(x => x.MaKhoa != excludeId.Value);
            return await q.AnyAsync();
        }

        public Task<Khoa?> GetKhoaByIdAsync(int maKhoa)
            => _db.Khoa.FirstOrDefaultAsync(x => x.MaKhoa == maKhoa);

        public async Task AddKhoaAsync(Khoa entity)
        {
            _db.Khoa.Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateKhoaAsync(Khoa entity)
        {
            _db.Khoa.Update(entity);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteKhoaAsync(Khoa entity)
        {
            _db.Khoa.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> AnyNganhInKhoaAsync(int maKhoa)
            => await _db.Nganh.AsNoTracking().AnyAsync(x => x.MaKhoa == maKhoa);

        // ===== NGÀNH =====
        public async Task<(List<Nganh> Items, int Total)> SearchNganhAsync(
            int? maNganh, string? codeNganh, string? tenNganh, int? maKhoa,
            string? sortBy, bool desc, int page, int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

            var q = _db.Nganh.AsNoTracking().AsQueryable();

            if (maNganh.HasValue) q = q.Where(x => x.MaNganh == maNganh.Value);
            if (!string.IsNullOrWhiteSpace(codeNganh)) q = q.Where(x => x.CodeNganh == codeNganh);
            if (!string.IsNullOrWhiteSpace(tenNganh)) q = q.Where(x => x.TenNganh.Contains(tenNganh));
            if (maKhoa.HasValue) q = q.Where(x => x.MaKhoa == maKhoa.Value);

            var key = (sortBy ?? "MaNganh").Trim().ToLowerInvariant();
            q = key switch
            {
                "codenganh" => (desc ? q.OrderByDescending(x => x.CodeNganh) : q.OrderBy(x => x.CodeNganh)),
                "tennganh" => (desc ? q.OrderByDescending(x => x.TenNganh) : q.OrderBy(x => x.TenNganh)),
                "makhoa" => (desc ? q.OrderByDescending(x => x.MaKhoa) : q.OrderBy(x => x.MaKhoa)),
                _ => (desc ? q.OrderByDescending(x => x.MaNganh) : q.OrderBy(x => x.MaNganh))
            };

            var total = await q.CountAsync();
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }

        public async Task<bool> NganhCodeExistsAsync(string codeNganh, int? excludeId = null)
        {
            var code = (codeNganh ?? "").Trim().ToLower();
            var q = _db.Nganh.AsNoTracking().Where(x => (x.CodeNganh ?? "").ToLower() == code);
            if (excludeId.HasValue) q = q.Where(x => x.MaNganh != excludeId.Value);
            return await q.AnyAsync();
        }

        public Task<Nganh?> GetNganhByIdAsync(int maNganh)
            => _db.Nganh.FirstOrDefaultAsync(x => x.MaNganh == maNganh);

        public async Task AddNganhAsync(Nganh entity)
        {
            _db.Nganh.Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateNganhAsync(Nganh entity)
        {
            _db.Nganh.Update(entity);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteNganhAsync(Nganh entity)
        {
            _db.Nganh.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> KhoaExistsAsync(int maKhoa)
            => await _db.Khoa.AsNoTracking().AnyAsync(x => x.MaKhoa == maKhoa);
    }
}
