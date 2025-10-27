// File: Repositories/Implementations/ScheduleRepository.cs
using DiemDanhQR_API.Data;
using DiemDanhQR_API.Models;
using DiemDanhQR_API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DiemDanhQR_API.Repositories.Implementations
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly AppDbContext _db;
        public ScheduleRepository(AppDbContext db) => _db = db;

        public async Task<(List<(BuoiHoc b, PhongHoc p, LopHocPhan l, MonHoc m, GiangVien gv, NguoiDung ndGv)> Items, int Total)>
            SearchSchedulesAsync(
                string? keyword,
                int? maBuoi,
                int? maPhong,
                string? tenPhong,
                string? maLopHocPhan,
                string? tenLop,
                string? tenMonHoc,
                DateTime? ngayHoc,
                byte? tietBatDau,
                byte? soTiet,
                string? ghiChu,
                string? maSinhVien,    // NEW
                string? maGiangVien,   // NEW
                string? sortBy,
                bool desc,
                int page,
                int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

            var q =
                from b in _db.BuoiHoc.AsNoTracking()
                join p in _db.PhongHoc.AsNoTracking() on b.MaPhong equals p.MaPhong
                join l in _db.LopHocPhan.AsNoTracking() on b.MaLopHocPhan equals l.MaLopHocPhan
                join m in _db.MonHoc.AsNoTracking() on l.MaMonHoc equals m.MaMonHoc
                join gv in _db.GiangVien.AsNoTracking() on l.MaGiangVien equals gv.MaGiangVien
                join ndGv in _db.NguoiDung.AsNoTracking() on gv.MaNguoiDung equals ndGv.MaNguoiDung
                select new { b, p, l, m, gv, ndGv };

            // ===== Filters =====
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                q = q.Where(x =>
                    (x.b.MaBuoi.HasValue ? x.b.MaBuoi.Value.ToString() : "").Contains(kw) ||
                    (x.p.TenPhong ?? "").ToLower().Contains(kw) ||
                    (x.l.MaLopHocPhan ?? "").ToLower().Contains(kw) ||
                    (x.l.TenLopHocPhan ?? "").ToLower().Contains(kw) ||
                    (x.m.TenMonHoc ?? "").ToLower().Contains(kw) ||
                    (x.ndGv.HoTen ?? "").ToLower().Contains(kw) ||
                    (x.b.GhiChu ?? "").ToLower().Contains(kw)
                );
            }

            if (maBuoi.HasValue) q = q.Where(x => x.b.MaBuoi == maBuoi.Value);
            if (maPhong.HasValue) q = q.Where(x => x.p.MaPhong == maPhong.Value);

            if (!string.IsNullOrWhiteSpace(tenPhong))
            {
                var s = tenPhong.Trim().ToLower();
                q = q.Where(x => (x.p.TenPhong ?? "").ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(maLopHocPhan))
            {
                var code = maLopHocPhan.Trim().Replace(" ", "");
                q = q.Where(x => ((x.l.MaLopHocPhan ?? "").Replace(" ", "")) == code);
            }

            if (!string.IsNullOrWhiteSpace(tenLop))
            {
                var s = tenLop.Trim().ToLower();
                q = q.Where(x => (x.l.TenLopHocPhan ?? "").ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(tenMonHoc))
            {
                var s = tenMonHoc.Trim().ToLower();
                q = q.Where(x => (x.m.TenMonHoc ?? "").ToLower().Contains(s));
            }

            if (ngayHoc.HasValue) q = q.Where(x => x.b.NgayHoc == ngayHoc.Value.Date);
            if (tietBatDau.HasValue) q = q.Where(x => (x.b.TietBatDau ?? 0) == tietBatDau.Value);
            if (soTiet.HasValue) q = q.Where(x => (x.b.SoTiet ?? 0) == soTiet.Value);

            if (!string.IsNullOrWhiteSpace(ghiChu))
            {
                var s = ghiChu.Trim().ToLower();
                q = q.Where(x => (x.b.GhiChu ?? "").ToLower().Contains(s));
            }
            
            // ===== NEW: lọc theo mã giảng viên (lịch dạy) =====
            if (!string.IsNullOrWhiteSpace(maGiangVien))
            {
                var code = maGiangVien.Trim().Replace(" ", "");
                q = q.Where(x => ((x.gv.MaGiangVien ?? "").Replace(" ", "")) == code);
            }

            // ===== NEW: lọc theo mã sinh viên (lịch học) — dùng EXISTS tránh join nhân bản bản ghi =====
            if (!string.IsNullOrWhiteSpace(maSinhVien))
            {
                var code = maSinhVien.Trim().Replace(" ", "");
                q = q.Where(x => _db.ThamGiaLop.AsNoTracking()
                                 .Any(t => ((t.MaSinhVien ?? "").Replace(" ", "")) == code
                                        && t.MaLopHocPhan == x.l.MaLopHocPhan));
            }

            // ===== Sorting =====
            var key = (sortBy ?? "MaBuoi").Trim().ToLowerInvariant();
            q = key switch
            {
                "maphong"      => desc ? q.OrderByDescending(x => x.p.MaPhong)        : q.OrderBy(x => x.p.MaPhong),
                "tenphong"     => desc ? q.OrderByDescending(x => x.p.TenPhong)       : q.OrderBy(x => x.p.TenPhong),
                "malophocphan" => desc ? q.OrderByDescending(x => x.l.MaLopHocPhan)   : q.OrderBy(x => x.l.MaLopHocPhan),
                "tenlop"       => desc ? q.OrderByDescending(x => x.l.TenLopHocPhan)  : q.OrderBy(x => x.l.TenLopHocPhan),
                "tenmonhoc"    => desc ? q.OrderByDescending(x => x.m.TenMonHoc)      : q.OrderBy(x => x.m.TenMonHoc),
                "ngayhoc"      => desc ? q.OrderByDescending(x => x.b.NgayHoc)        : q.OrderBy(x => x.b.NgayHoc),
                "tietbatdau"   => desc ? q.OrderByDescending(x => x.b.TietBatDau)     : q.OrderBy(x => x.b.TietBatDau),
                "sotiet"       => desc ? q.OrderByDescending(x => x.b.SoTiet)         : q.OrderBy(x => x.b.SoTiet),
                "ghichu"       => desc ? q.OrderByDescending(x => x.b.GhiChu)         : q.OrderBy(x => x.b.GhiChu),
                "trangthai"    => desc ? q.OrderByDescending(x => x.l.TrangThai)      : q.OrderBy(x => x.l.TrangThai),
                "sotinchi"     => desc ? q.OrderByDescending(x => x.m.SoTinChi)       : q.OrderBy(x => x.m.SoTinChi),
                "hocky"        => desc ? q.OrderByDescending(x => x.m.HocKy)          : q.OrderBy(x => x.m.HocKy),
                "tengiangvien" => desc ? q.OrderByDescending(x => x.ndGv.HoTen)       : q.OrderBy(x => x.ndGv.HoTen),
                "mabuoi" or _  => desc ? q.OrderByDescending(x => x.b.MaBuoi)         : q.OrderBy(x => x.b.MaBuoi),
            };

            // ===== Paging =====
            var total = await q.CountAsync();
            var list = await q.Skip((page - 1) * pageSize)
                              .Take(pageSize)
                              .Select(x => new { x.b, x.p, x.l, x.m, x.gv, x.ndGv })
                              .ToListAsync();

            var items = list.Select(x => (x.b, x.p, x.l, x.m, x.gv, x.ndGv)).ToList();
            return (items, total);
        }
    }
}
