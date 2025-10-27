// File: Repositories/Implementations/UserRepository.cs
using DiemDanhQR_API.Data;
using DiemDanhQR_API.Models;
using DiemDanhQR_API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DiemDanhQR_API.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db) => _db = db;

        public Task<bool> ExistsByMaNguoiDungAsync(string maNguoiDung)
            => _db.NguoiDung.AnyAsync(x => x.MaNguoiDung == maNguoiDung);

        public Task<bool> ExistsByTenDangNhapAsync(string tenDangNhap)
            => _db.NguoiDung.AnyAsync(x => x.TenDangNhap == tenDangNhap);

        public Task<PhanQuyen?> GetRoleAsync(int maQuyen)
            => _db.PhanQuyen.FirstOrDefaultAsync(r => r.MaQuyen == maQuyen);

        public async Task AddAsync(NguoiDung entity)
        {
            await _db.NguoiDung.AddAsync(entity);
        }

        public Task SaveChangesAsync() => _db.SaveChangesAsync();

        public Task<NguoiDung?> GetByMaNguoiDungAsync(string maNguoiDung)
            => _db.NguoiDung.FirstOrDefaultAsync(u => u.MaNguoiDung == maNguoiDung);

        public Task<NguoiDung?> GetByTenDangNhapAsync(string tenDangNhap)
            => _db.NguoiDung.FirstOrDefaultAsync(u => u.TenDangNhap == tenDangNhap);

        public Task<SinhVien?> GetStudentByMaNguoiDungAsync(string maNguoiDung)
            => _db.SinhVien.FirstOrDefaultAsync(s => s.MaNguoiDung == maNguoiDung);

        public Task<GiangVien?> GetLecturerByMaNguoiDungAsync(string maNguoiDung)
            => _db.GiangVien.FirstOrDefaultAsync(g => g.MaNguoiDung == maNguoiDung);

        public async Task<(List<(LichSuHoatDong Log, NguoiDung User)> Items, int Total)> SearchActivitiesAsync(
            string? keyword,
            string? maNguoiDung,
            DateTime? from,
            DateTime? to,
            string? sortBy,
            bool desc,
            int page,
            int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

            var q =
                from l in _db.LichSuHoatDong.AsNoTracking()
                join u in _db.NguoiDung.AsNoTracking() on l.MaNguoiDung equals u.MaNguoiDung
                select new { l, u };

            // Filters
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                q = q.Where(x =>
                    (x.l.HanhDong ?? "").ToLower().Contains(kw) ||
                    (x.u.TenDangNhap ?? "").ToLower().Contains(kw) ||
                    (x.u.MaNguoiDung ?? "").ToLower().Contains(kw));
            }

            if (!string.IsNullOrWhiteSpace(maNguoiDung))
            {
                var code = maNguoiDung.Trim().Replace(" ", "");
                q = q.Where(x => ((x.u.MaNguoiDung ?? "").Replace(" ", "")) == code);
            }

            if (from.HasValue) q = q.Where(x => x.l.ThoiGian >= from.Value);
            if (to.HasValue) q = q.Where(x => x.l.ThoiGian <= to.Value);

            // Sorting
            var key = (sortBy ?? "ThoiGian").Trim().ToLowerInvariant();
            q = key switch
            {
                "malichsu" => desc ? q.OrderByDescending(x => x.l.MaLichSu) : q.OrderBy(x => x.l.MaLichSu),
                "hanhdong" => desc ? q.OrderByDescending(x => x.l.HanhDong) : q.OrderBy(x => x.l.HanhDong),
                "manguoidung" => desc ? q.OrderByDescending(x => x.u.MaNguoiDung) : q.OrderBy(x => x.u.MaNguoiDung),
                "tendangnhap" => desc ? q.OrderByDescending(x => x.u.TenDangNhap) : q.OrderBy(x => x.u.TenDangNhap),
                "thoigian" or _
                              => desc ? q.OrderByDescending(x => x.l.ThoiGian) : q.OrderBy(x => x.l.ThoiGian),
            };

            // Paging
            var total = await q.CountAsync();
            var list = await q.Skip((page - 1) * pageSize)
                              .Take(pageSize)
                              .Select(x => new { x.l, x.u })
                              .ToListAsync();

            var items = list.Select(x => (x.l, x.u)).ToList();
            return (items, total);
        }

    }
}
