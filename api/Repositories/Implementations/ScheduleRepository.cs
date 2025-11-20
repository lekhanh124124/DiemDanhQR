// File: Repositories/Implementations/ScheduleRepository.cs
using api.Data;
using api.Helpers;
using api.Models;
using api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories.Implementations
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly AppDbContext _db;
        public ScheduleRepository(AppDbContext db) => _db = db;

        public async Task<(List<(BuoiHoc b, PhongHoc? p, LopHocPhan l, MonHoc m, GiangVien? gv)> Items, int Total)>
    SearchSchedulesAsync(
        int? maBuoi,
        int? maPhong,
        string? tenPhong,
        string? maLopHocPhan,
        string? tenLopHocPhan,
        string? tenMonHoc,
        int? maHocKy,           // ⬅️ mới thêm
        DateOnly? ngayHoc,
        int? nam,
        int? tuan,
        int? thang,
        byte? tietBatDau,
        byte? soTiet,
        bool? trangThai,
        string? maSinhVien,
        string? maGiangVien,
        string? sortBy,
        bool desc,
        int page,
        int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

            var q =
                from b in _db.BuoiHoc.AsNoTracking()
                join p0 in _db.PhongHoc.AsNoTracking() on b.MaPhong equals p0.MaPhong into jp
                from p in jp.DefaultIfEmpty()
                join l in _db.LopHocPhan.AsNoTracking() on b.MaLopHocPhan equals l.MaLopHocPhan
                join m in _db.MonHoc.AsNoTracking() on l.MaMonHoc equals m.MaMonHoc
                join gv0 in _db.GiangVien.AsNoTracking() on l.MaGiangVien equals gv0.MaGiangVien into jgv
                from gv in jgv.DefaultIfEmpty()
                select new { b, p, l, m, gv };

            if (maBuoi.HasValue) q = q.Where(x => x.b.MaBuoi == maBuoi.Value);
            if (maPhong.HasValue) q = q.Where(x => x.p != null && x.p.MaPhong == maPhong.Value);

            if (!string.IsNullOrWhiteSpace(tenPhong))
            {
                var s = tenPhong.Trim().ToLower();
                q = q.Where(x => (x.p != null ? (x.p.TenPhong ?? "") : "").ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(maLopHocPhan))
            {
                var code = maLopHocPhan.Trim().Replace(" ", "");
                q = q.Where(x => ((x.l.MaLopHocPhan ?? "").Replace(" ", "")) == code);
            }

            if (!string.IsNullOrWhiteSpace(tenLopHocPhan))
            {
                var s = tenLopHocPhan.Trim().ToLower();
                q = q.Where(x => (x.l.TenLopHocPhan ?? "").ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(tenMonHoc))
            {
                var s = tenMonHoc.Trim().ToLower();
                q = q.Where(x => (x.m.TenMonHoc ?? "").ToLower().Contains(s));
            }

            // ⬇️ Filter theo Mã học kỳ
            if (maHocKy.HasValue)
            {
                q = q.Where(x => x.l.MaHocKy == maHocKy.Value);
            }

            if (ngayHoc.HasValue) q = q.Where(x => x.b.NgayHoc == ngayHoc.Value);

            if (nam.HasValue) q = q.Where(x => x.b.NgayHoc.Year == nam.Value);
            if (thang.HasValue) q = q.Where(x => x.b.NgayHoc.Month == thang.Value);

            if (tuan.HasValue)
            {
                var year = nam ?? DateTime.Now.Year;
                var startOfYear = new DateOnly(year, 1, 1);
                var startOfWeek = startOfYear.AddDays((tuan.Value - 1) * 7);
                var endOfWeekExclusive = startOfWeek.AddDays(7);

                q = q.Where(x =>
                    x.b.NgayHoc.Year == year &&
                    x.b.NgayHoc >= startOfWeek &&
                    x.b.NgayHoc < endOfWeekExclusive);
            }

            if (tietBatDau.HasValue) q = q.Where(x => x.b.TietBatDau == tietBatDau.Value);
            if (soTiet.HasValue) q = q.Where(x => x.b.SoTiet == soTiet.Value);
            if (trangThai.HasValue) q = q.Where(x => x.b.TrangThai == trangThai.Value);

            if (!string.IsNullOrWhiteSpace(maGiangVien))
            {
                var code = maGiangVien.Trim().Replace(" ", "");
                q = q.Where(x => (x.gv != null ? (x.gv.MaGiangVien ?? "").Replace(" ", "") : "") == code);
            }

            if (!string.IsNullOrWhiteSpace(maSinhVien))
            {
                var code = maSinhVien.Trim().Replace(" ", "");
                q = q.Where(x => _db.ThamGiaLop.AsNoTracking()
                                  .Any(t => (t.MaSinhVien ?? "").Replace(" ", "") == code
                                         && t.MaLopHocPhan == x.l.MaLopHocPhan));
            }

            var key = (sortBy ?? "MaBuoi").Trim().ToLowerInvariant();

            q = key switch
            {
                "maphong" => desc ? q.OrderByDescending(x => x.p!.MaPhong) : q.OrderBy(x => x.p!.MaPhong),
                "tenphong" => desc ? q.OrderByDescending(x => x.p!.TenPhong) : q.OrderBy(x => x.p!.TenPhong),
                "malophocphan" => desc ? q.OrderByDescending(x => x.l.MaLopHocPhan) : q.OrderBy(x => x.l.MaLopHocPhan),
                "tenlop" => desc ? q.OrderByDescending(x => x.l.TenLopHocPhan) : q.OrderBy(x => x.l.TenLopHocPhan),
                "tenmonhoc" => desc ? q.OrderByDescending(x => x.m.TenMonHoc) : q.OrderBy(x => x.m.TenMonHoc),
                "ngayhoc" => desc ? q.OrderByDescending(x => x.b.NgayHoc) : q.OrderBy(x => x.b.NgayHoc),
                "tietbatdau" => desc ? q.OrderByDescending(x => x.b.TietBatDau) : q.OrderBy(x => x.b.TietBatDau),
                "sotiet" => desc ? q.OrderByDescending(x => x.b.SoTiet) : q.OrderBy(x => x.b.SoTiet),
                "trangthai" => desc ? q.OrderByDescending(x => x.b.TrangThai) : q.OrderBy(x => x.b.TrangThai),
                "thang" => desc ? q.OrderByDescending(x => x.b.NgayHoc.Month) : q.OrderBy(x => x.b.NgayHoc.Month),
                "tuan" => desc ? q.OrderByDescending(x => x.b.NgayHoc) : q.OrderBy(x => x.b.NgayHoc),
                "nam" => desc ? q.OrderByDescending(x => x.b.NgayHoc.Year) : q.OrderBy(x => x.b.NgayHoc.Year),
                "mabuoi" or _ => desc ? q.OrderByDescending(x => x.b.MaBuoi) : q.OrderBy(x => x.b.MaBuoi)
            };

            var total = await q.CountAsync();

            var list = await q.Skip((page - 1) * pageSize)
                              .Take(pageSize)
                              .Select(x => new { x.b, x.p, x.l, x.m, x.gv })
                              .ToListAsync();

            var items = list.Select(x => (x.b, x.p, x.l, x.m, x.gv)).ToList();
            return (items, total);
        }

        public async Task<(List<PhongHoc> Items, int Total)> SearchRoomsAsync(
            int? maPhong,
            string? tenPhong,
            string? toaNha,
            byte? tang,
            byte? sucChua,
            bool? trangThai,
            string? sortBy,
            bool desc,
            int page,
            int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

            var q = _db.PhongHoc.AsNoTracking().AsQueryable();

            if (maPhong.HasValue) q = q.Where(p => p.MaPhong == maPhong.Value);
            if (!string.IsNullOrWhiteSpace(tenPhong))
            {
                var s = tenPhong.Trim().ToLower();
                q = q.Where(p => (p.TenPhong ?? "").ToLower().Contains(s));
            }
            if (!string.IsNullOrWhiteSpace(toaNha))
            {
                var s = toaNha.Trim().ToLower();
                q = q.Where(p => (p.ToaNha ?? "").ToLower().Contains(s));
            }
            if (tang.HasValue) q = q.Where(p => p.Tang == tang.Value);
            if (sucChua.HasValue) q = q.Where(p => p.SucChua == sucChua.Value);
            if (trangThai.HasValue) q = q.Where(p => p.TrangThai == trangThai.Value);

            var key = (sortBy ?? "MaPhong").Trim().ToLowerInvariant();
            q = key switch
            {
                "tenphong" => desc ? q.OrderByDescending(p => p.TenPhong) : q.OrderBy(p => p.TenPhong),
                "toanha" => desc ? q.OrderByDescending(p => p.ToaNha) : q.OrderBy(p => p.ToaNha),
                "tang" => desc ? q.OrderByDescending(p => p.Tang) : q.OrderBy(p => p.Tang),
                "succhua" => desc ? q.OrderByDescending(p => p.SucChua) : q.OrderBy(p => p.SucChua),
                "trangthai" => desc ? q.OrderByDescending(p => p.TrangThai) : q.OrderBy(p => p.TrangThai),
                "maphong" or _ => desc ? q.OrderByDescending(p => p.MaPhong) : q.OrderBy(p => p.MaPhong)
            };

            var total = await q.CountAsync();
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }

        public async Task<bool> RoomNameExistsAsync(string tenPhong)
        {
            var name = (tenPhong ?? "").Trim().ToLower();
            return await _db.PhongHoc.AsNoTracking()
                .AnyAsync(p => (p.TenPhong ?? "").ToLower() == name);
        }

        public async Task AddRoomAsync(PhongHoc room)
        {
            _db.PhongHoc.Add(room);
            await _db.SaveChangesAsync();
        }

        public async Task LogActivityAsync(string? tenDangNhap, string hanhDong)
        {
            if (string.IsNullOrWhiteSpace(tenDangNhap)) return;

            var user = await _db.NguoiDung.AsNoTracking()
                .FirstOrDefaultAsync(u => u.TenDangNhap == tenDangNhap);
            if (user == null) return;

            _db.LichSuHoatDong.Add(new LichSuHoatDong
            {
                MaNguoiDung = user.MaNguoiDung,
                HanhDong = hanhDong,
                ThoiGian = TimeHelper.UtcToVietnam(DateTime.UtcNow) // ghi UTC->VN, không format
            });
            await _db.SaveChangesAsync();
        }

        public async Task<bool> CourseExistsByCodeAsync(string maLopHocPhan)
        {
            var code = (maLopHocPhan ?? "").Trim();
            return await _db.LopHocPhan.AsNoTracking().AnyAsync(l => (l.MaLopHocPhan ?? "") == code);
        }

        public async Task<bool> RoomExistsByIdAsync(int maPhong)
        {
            return await _db.PhongHoc.AsNoTracking().AnyAsync(p => p.MaPhong == maPhong);
        }

        public async Task<bool> ScheduleExistsAsync(string maLopHocPhan, DateOnly ngayHoc, byte tietBatDau)
        {
            var code = (maLopHocPhan ?? "").Trim();
            return await _db.BuoiHoc.AsNoTracking()
                .AnyAsync(b => (b.MaLopHocPhan ?? "") == code
                               && b.NgayHoc == ngayHoc
                               && b.TietBatDau == tietBatDau);
        }

        public async Task AddScheduleAsync(BuoiHoc buoi)
        {
            _db.BuoiHoc.Add(buoi);
            await _db.SaveChangesAsync();
        }

        public async Task<PhongHoc?> GetRoomByIdAsync(int maPhong)
        {
            return await _db.PhongHoc.AsNoTracking().FirstOrDefaultAsync(p => p.MaPhong == maPhong);
        }

        public async Task<PhongHoc?> GetRoomForUpdateAsync(int maPhong)
        {
            return await _db.PhongHoc.FirstOrDefaultAsync(p => p.MaPhong == maPhong);
        }

        public async Task<bool> RoomNameExistsExceptIdAsync(string tenPhong, int excludeMaPhong)
        {
            var name = (tenPhong ?? "").Trim().ToLower();
            return await _db.PhongHoc.AsNoTracking()
                .AnyAsync(p => (p.TenPhong ?? "").ToLower() == name && p.MaPhong != excludeMaPhong);
        }

        public async Task UpdateRoomAsync(PhongHoc room)
        {
            _db.PhongHoc.Update(room);
            await _db.SaveChangesAsync();
        }

        public async Task<BuoiHoc?> GetScheduleByIdAsync(int maBuoi)
        {
            return await _db.BuoiHoc.FirstOrDefaultAsync(b => b.MaBuoi == maBuoi);
        }

        public async Task UpdateScheduleAsync(BuoiHoc buoi)
        {
            _db.BuoiHoc.Update(buoi);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> ScheduleExistsExceptAsync(string maLopHocPhan, DateOnly ngayHoc, byte tietBatDau, int excludeMaBuoi)
        {
            var code = (maLopHocPhan ?? "").Trim();
            return await _db.BuoiHoc.AsNoTracking()
                .AnyAsync(b => (b.MaLopHocPhan ?? "") == code
                               && b.NgayHoc == ngayHoc
                               && b.TietBatDau == tietBatDau
                               && b.MaBuoi != excludeMaBuoi);
        }
        public async Task<(LopHocPhan Lhp, MonHoc Mh, HocKy Hk, GiangVien? Gv)?> GetCourseBundleAsync(string maLopHocPhan)
        {
            var code = (maLopHocPhan ?? "").Trim();
            var q =
                from l in _db.LopHocPhan.AsNoTracking()
                join m in _db.MonHoc.AsNoTracking() on l.MaMonHoc equals m.MaMonHoc
                join hk in _db.HocKy.AsNoTracking() on l.MaHocKy equals hk.MaHocKy
                join gv0 in _db.GiangVien.AsNoTracking() on l.MaGiangVien equals gv0.MaGiangVien into jgv
                from gv in jgv.DefaultIfEmpty()
                where (l.MaLopHocPhan ?? "") == code
                select new { l, m, hk, gv };
            var row = await q.FirstOrDefaultAsync();
            if (row == null) return null;
            return (row.l, row.m, row.hk, row.gv);
        }

        public Task<List<PhongHoc>> GetActiveRoomsAsync()
            => _db.PhongHoc.AsNoTracking()
               .Where(p => p.TrangThai == true)
               .OrderBy(p => p.MaPhong)
               .ToListAsync();

        public Task<bool> AnyRoomConflictAsync(int maPhong, DateOnly ngayHoc, byte tietBatDau, byte soTiet)
        {
            var start = tietBatDau;
            var end = (byte)(tietBatDau + soTiet - 1);
            return _db.BuoiHoc.AsNoTracking().AnyAsync(b =>
                b.MaPhong == maPhong
                && b.NgayHoc == ngayHoc
                && !((b.TietBatDau + b.SoTiet - 1) < start || b.TietBatDau > end)  // giao nhau
            );
        }

        public Task<bool> AnyCourseConflictAsync(string maLopHocPhan, DateOnly ngayHoc, byte tietBatDau, byte soTiet)
        {
            var code = (maLopHocPhan ?? "").Trim();
            var start = tietBatDau;
            var end = (byte)(tietBatDau + soTiet - 1);
            return _db.BuoiHoc.AsNoTracking().AnyAsync(b =>
                (b.MaLopHocPhan ?? "") == code
                && b.NgayHoc == ngayHoc
                && !((b.TietBatDau + b.SoTiet - 1) < start || b.TietBatDau > end)
            );
        }

        public async Task AddSchedulesAsync(IEnumerable<BuoiHoc> items)
        {
            _db.BuoiHoc.AddRange(items);
            await _db.SaveChangesAsync();
        }

        public async Task<NguoiDung?> GetUserByIdAsync(int maNguoiDung)
        {
            return await _db.NguoiDung.AsNoTracking().FirstOrDefaultAsync(p => p.MaNguoiDung == maNguoiDung);
        }
    }
}
