// File: Repositories/Implementations/CourseRepository.cs
using DiemDanhQR_API.Data;
using DiemDanhQR_API.Models;
using DiemDanhQR_API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DiemDanhQR_API.Repositories.Implementations
{
    public class CourseRepository : ICourseRepository
    {
        private readonly AppDbContext _db;
        public CourseRepository(AppDbContext db) => _db = db;

        public async Task<(List<(LopHocPhan Lhp, MonHoc Mh, GiangVien Gv, NguoiDung Nd)> Items, int Total)> SearchCoursesAsync(
            string? keyword,
            string? maLopHocPhan,
            string? tenLopHocPhan,
            bool? trangThai,
            string? tenMonHoc,
            byte? soTinChi,
            byte? soTiet,
            byte? hocKy,
            string? tenGiangVien,
            string? maMonHoc,        // NEW
            string? maGiangVien,     // NEW
            string? sortBy,
            bool desc,
            int page,
            int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

            var q =
                from l in _db.LopHocPhan.AsNoTracking()
                join m in _db.MonHoc.AsNoTracking() on l.MaMonHoc equals m.MaMonHoc
                join gv in _db.GiangVien.AsNoTracking() on l.MaGiangVien equals gv.MaGiangVien
                join nd in _db.NguoiDung.AsNoTracking() on gv.MaNguoiDung equals nd.MaNguoiDung
                select new { l, m, gv, nd };

            // ===== Filters =====
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                q = q.Where(x =>
                    (x.l.MaLopHocPhan ?? "").ToLower().Contains(kw) ||
                    (x.l.TenLopHocPhan ?? "").ToLower().Contains(kw) ||
                    (x.m.TenMonHoc ?? "").ToLower().Contains(kw) ||
                    (x.m.MaMonHoc ?? "").ToLower().Contains(kw) ||     // cho keyword bắt cả mã MH
                    (x.gv.MaGiangVien ?? "").ToLower().Contains(kw) || // và mã GV
                    (x.nd.HoTen ?? "").ToLower().Contains(kw)
                );
            }

            if (!string.IsNullOrWhiteSpace(maLopHocPhan))
            {
                var code = maLopHocPhan.Trim();
                q = q.Where(x => (x.l.MaLopHocPhan ?? "").Replace(" ", "").Equals(code.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(tenLopHocPhan))
            {
                var s = tenLopHocPhan.Trim().ToLower();
                q = q.Where(x => (x.l.TenLopHocPhan ?? "").ToLower().Contains(s));
            }

            if (trangThai.HasValue)
                q = q.Where(x => (x.l.TrangThai ?? true) == trangThai.Value);

            if (!string.IsNullOrWhiteSpace(tenMonHoc))
            {
                var s = tenMonHoc.Trim().ToLower();
                q = q.Where(x => (x.m.TenMonHoc ?? "").ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(maMonHoc))                     // NEW
            {
                var code = maMonHoc.Trim();
                q = q.Where(x => (x.m.MaMonHoc ?? "").Replace(" ", "").Equals(code.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));
            }

            if (soTinChi.HasValue) q = q.Where(x => (x.m.SoTinChi ?? 0) == soTinChi.Value);
            if (soTiet.HasValue)   q = q.Where(x => (x.m.SoTiet   ?? 0) == soTiet.Value);
            if (hocKy.HasValue)    q = q.Where(x => x.m.HocKy == hocKy.Value);

            if (!string.IsNullOrWhiteSpace(tenGiangVien))
            {
                var s = tenGiangVien.Trim().ToLower();
                q = q.Where(x => (x.nd.HoTen ?? "").ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(maGiangVien))                  // NEW
            {
                var code = maGiangVien.Trim();
                q = q.Where(x => (x.gv.MaGiangVien ?? "").Replace(" ", "").Equals(code.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));
            }

            // ===== Sorting =====
            var key = (sortBy ?? "MaLopHocPhan").Trim().ToLowerInvariant();
            q = key switch
            {
                "tenlophocphan" => desc ? q.OrderByDescending(x => x.l.TenLopHocPhan) : q.OrderBy(x => x.l.TenLopHocPhan),
                "trangthai"     => desc ? q.OrderByDescending(x => x.l.TrangThai)     : q.OrderBy(x => x.l.TrangThai),
                "mamonhoc"      => desc ? q.OrderByDescending(x => x.m.MaMonHoc)      : q.OrderBy(x => x.m.MaMonHoc),     // NEW
                "tenmonhoc"     => desc ? q.OrderByDescending(x => x.m.TenMonHoc)     : q.OrderBy(x => x.m.TenMonHoc),
                "sotinchi"      => desc ? q.OrderByDescending(x => x.m.SoTinChi)      : q.OrderBy(x => x.m.SoTinChi),
                "sotiet"        => desc ? q.OrderByDescending(x => x.m.SoTiet)        : q.OrderBy(x => x.m.SoTiet),
                "hocky"         => desc ? q.OrderByDescending(x => x.m.HocKy)         : q.OrderBy(x => x.m.HocKy),
                "magiangvien"   => desc ? q.OrderByDescending(x => x.gv.MaGiangVien)  : q.OrderBy(x => x.gv.MaGiangVien), // NEW
                "tengiangvien"  => desc ? q.OrderByDescending(x => x.nd.HoTen)        : q.OrderBy(x => x.nd.HoTen),
                "malophocphan" or _ 
                                => desc ? q.OrderByDescending(x => x.l.MaLopHocPhan) : q.OrderBy(x => x.l.MaLopHocPhan),
            };

            // ===== Paging =====
            var total = await q.CountAsync();

            var list = await q.Skip((page - 1) * pageSize)
                              .Take(pageSize)
                              .Select(x => new { x.l, x.m, x.gv, x.nd })
                              .ToListAsync();

            var items = list.Select(x => (x.l, x.m, x.gv, x.nd)).ToList();
            return (items, total);
        }
    }
}
