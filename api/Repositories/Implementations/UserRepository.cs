// File: Repositories/Implementations/UserRepository.cs
using api.Data;
using api.Models;
using api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db) => _db = db;

        public Task<bool> ExistsByTenDangNhapAsync(string tenDangNhap)
            => _db.NguoiDung.AnyAsync(x => x.TenDangNhap == tenDangNhap);

        public Task<PhanQuyen?> GetRoleAsync(int maQuyen)
            => _db.PhanQuyen.FirstOrDefaultAsync(r => r.MaQuyen == maQuyen);

        public async Task AddAsync(NguoiDung entity)
        {
            await _db.NguoiDung.AddAsync(entity);
        }

        public Task SaveChangesAsync() => _db.SaveChangesAsync();

        public Task<NguoiDung?> GetByIdAsync(int maNguoiDung)
            => _db.NguoiDung.FirstOrDefaultAsync(u => u.MaNguoiDung == maNguoiDung);

        public Task<NguoiDung?> GetByTenDangNhapAsync(string tenDangNhap)
            => _db.NguoiDung.FirstOrDefaultAsync(u => u.TenDangNhap == tenDangNhap);

        public Task<SinhVien?> GetStudentByMaNguoiDungAsync(int maNguoiDung)
            => _db.SinhVien.FirstOrDefaultAsync(s => s.MaNguoiDung == maNguoiDung);

        public Task<GiangVien?> GetLecturerByMaNguoiDungAsync(int maNguoiDung)
            => _db.GiangVien.FirstOrDefaultAsync(g => g.MaNguoiDung == maNguoiDung);

        public async Task<(List<(LichSuHoatDong Log, NguoiDung User)> Items, int Total)> SearchActivitiesAsync(
            string? tenDangNhap,
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

            if (!string.IsNullOrWhiteSpace(tenDangNhap))
            {
                var userFilter = tenDangNhap.Trim();
                q = q.Where(x => x.u.TenDangNhap == userFilter);
            }

            if (from.HasValue) q = q.Where(x => x.l.ThoiGian >= from.Value);
            if (to.HasValue) q = q.Where(x => x.l.ThoiGian <= to.Value);

            var key = (sortBy ?? "ThoiGian").Trim().ToLowerInvariant();
            q = key switch
            {
                "malichsu" => desc ? q.OrderByDescending(x => x.l.MaLichSu) : q.OrderBy(x => x.l.MaLichSu),
                "hanhdong" => desc ? q.OrderByDescending(x => x.l.HanhDong) : q.OrderBy(x => x.l.HanhDong),
                "manguoidung" => desc ? q.OrderByDescending(x => x.u.MaNguoiDung) : q.OrderBy(x => x.u.MaNguoiDung),
                "tendangnhap" => desc ? q.OrderByDescending(x => x.u.TenDangNhap) : q.OrderBy(x => x.u.TenDangNhap),
                "thoigian" or _ => desc ? q.OrderByDescending(x => x.l.ThoiGian) : q.OrderBy(x => x.l.ThoiGian),
            };

            var total = await q.CountAsync();
            var list = await q.Skip((page - 1) * pageSize)
                              .Take(pageSize)
                              .Select(x => new { x.l, x.u })
                              .ToListAsync();

            var items = list.Select(x => (x.l, x.u)).ToList();
            return (items, total);
        }

        public Task UpdateAsync(NguoiDung entity)
        {
            _db.NguoiDung.Update(entity);
            return Task.CompletedTask;
        }

        public async Task AddActivityAsync(LichSuHoatDong log)
        {
            await _db.LichSuHoatDong.AddAsync(log);
        }

        public async Task<(List<(NguoiDung User, PhanQuyen? Role)> Items, int Total)> SearchUsersAsync(
            string? tenDangNhap,
            string? hoTen,
            int? maQuyen,
            string? codeQuyen,
            bool? trangThai,
            string? sortBy,
            bool desc,
            int page,
            int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

            // LEFT JOIN NguoiDung - PhanQuyen
            var q =
                from u in _db.NguoiDung.AsNoTracking()
                join r in _db.PhanQuyen.AsNoTracking()
                    on u.MaQuyen equals r.MaQuyen into gj
                from r in gj.DefaultIfEmpty() // r có thể NULL (user không có quyền)
                select new { u, r };

            // chỉ lấy user KHÔNG có CodeQuyen = 'SV' hoặc 'GV'
            q = q.Where(x =>
                x.r == null ||                   
                (x.r.CodeQuyen != "SV" && x.r.CodeQuyen != "GV")
            );

            if (!string.IsNullOrWhiteSpace(tenDangNhap))
            {
                var s = tenDangNhap.Trim();
                q = q.Where(x => x.u.TenDangNhap.Contains(s));
            }
            if (!string.IsNullOrWhiteSpace(hoTen))
            {
                var s = hoTen.Trim();
                q = q.Where(x => (x.u.HoTen ?? "").Contains(s));
            }
            if (maQuyen.HasValue)
                q = q.Where(x => x.u.MaQuyen == maQuyen.Value);

            if (!string.IsNullOrWhiteSpace(codeQuyen))
            {
                var s = codeQuyen.Trim();
                q = q.Where(x => x.r != null && x.r.CodeQuyen.Contains(s));
            }

            if (trangThai.HasValue)
                q = q.Where(x => x.u.TrangThai == trangThai.Value);

            var key = (sortBy ?? "MaNguoiDung").Trim().ToLowerInvariant();
            q = key switch
            {
                "tendangnhap" => desc ? q.OrderByDescending(x => x.u.TenDangNhap) : q.OrderBy(x => x.u.TenDangNhap),
                "hoten" => desc ? q.OrderByDescending(x => x.u.HoTen) : q.OrderBy(x => x.u.HoTen),
                "maquyen" => desc ? q.OrderByDescending(x => x.u.MaQuyen) : q.OrderBy(x => x.u.MaQuyen),
                "trangthai" => desc ? q.OrderByDescending(x => x.u.TrangThai) : q.OrderBy(x => x.u.TrangThai),
                _ => desc ? q.OrderByDescending(x => x.u.MaNguoiDung) : q.OrderBy(x => x.u.MaNguoiDung),
            };

            var total = await q.CountAsync();
            var list = await q.Skip((page - 1) * pageSize)
                              .Take(pageSize)
                              .Select(x => new { x.u, x.r })
                              .ToListAsync();

            var items = list.Select(x => (x.u, x.r)).ToList();
            return (items, total)!;
        }


    }
}
