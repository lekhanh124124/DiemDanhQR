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

        public async Task<(List<(LopHocPhan Lhp, MonHoc Mh, GiangVien Gv, NguoiDung Nd, DateTime? NgayThamGia, bool? TrangThaiThamGia)> Items, int Total)>
                    SearchCoursesAsync(
                        string? keyword,
                        string? maLopHocPhan,
                        string? tenLopHocPhan,
                        bool? trangThai,
                        string? tenMonHoc,
                        byte? soTinChi,
                        byte? soTiet,
                        byte? hocKy,
                        string? tenGiangVien,
                        string? maMonHoc,
                        string? maGiangVien,
                        string? maSinhVien,     // NEW
                        string? sortBy,
                        bool desc,
                        int page,
                        int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

            var baseQ =
                from l in _db.LopHocPhan.AsNoTracking()
                join m in _db.MonHoc.AsNoTracking() on l.MaMonHoc equals m.MaMonHoc
                join gv in _db.GiangVien.AsNoTracking() on l.MaGiangVien equals gv.MaGiangVien
                join nd in _db.NguoiDung.AsNoTracking() on gv.MaNguoiDung equals nd.MaNguoiDung
                select new { l, m, gv, nd };

            // ===== Filters gốc ===== (giữ nguyên như trước)
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                baseQ = baseQ.Where(x =>
                    (x.l.MaLopHocPhan ?? "").ToLower().Contains(kw) ||
                    (x.l.TenLopHocPhan ?? "").ToLower().Contains(kw) ||
                    (x.m.TenMonHoc ?? "").ToLower().Contains(kw) ||
                    (x.m.MaMonHoc ?? "").ToLower().Contains(kw) ||
                    (x.gv.MaGiangVien ?? "").ToLower().Contains(kw) ||
                    (x.nd.HoTen ?? "").ToLower().Contains(kw)
                );
            }
            if (!string.IsNullOrWhiteSpace(maLopHocPhan))
            {
                var code = maLopHocPhan.Trim().Replace(" ", "");
                baseQ = baseQ.Where(x => ((x.l.MaLopHocPhan ?? "").Replace(" ", "")) == code);
            }
            if (!string.IsNullOrWhiteSpace(tenLopHocPhan))
                baseQ = baseQ.Where(x => (x.l.TenLopHocPhan ?? "").ToLower().Contains(tenLopHocPhan.Trim().ToLower()));

            if (trangThai.HasValue) baseQ = baseQ.Where(x => (x.l.TrangThai ?? true) == trangThai.Value);

            if (!string.IsNullOrWhiteSpace(tenMonHoc))
                baseQ = baseQ.Where(x => (x.m.TenMonHoc ?? "").ToLower().Contains(tenMonHoc.Trim().ToLower()));

            if (!string.IsNullOrWhiteSpace(maMonHoc))
            {
                var code = maMonHoc.Trim().Replace(" ", "");
                baseQ = baseQ.Where(x => ((x.m.MaMonHoc ?? "").Replace(" ", "")) == code);
            }
            if (soTinChi.HasValue) baseQ = baseQ.Where(x => (x.m.SoTinChi ?? 0) == soTinChi.Value);
            if (soTiet.HasValue) baseQ = baseQ.Where(x => (x.m.SoTiet ?? 0) == soTiet.Value);
            if (hocKy.HasValue) baseQ = baseQ.Where(x => x.m.HocKy == hocKy.Value);

            if (!string.IsNullOrWhiteSpace(tenGiangVien))
                baseQ = baseQ.Where(x => (x.nd.HoTen ?? "").ToLower().Contains(tenGiangVien.Trim().ToLower()));

            if (!string.IsNullOrWhiteSpace(maGiangVien))
            {
                var code = maGiangVien.Trim().Replace(" ", "");
                baseQ = baseQ.Where(x => ((x.gv.MaGiangVien ?? "").Replace(" ", "")) == code);
            }

            // ===== Sorting (giữ như cũ) =====
            var key = (sortBy ?? "MaLopHocPhan").Trim().ToLowerInvariant();
            baseQ = key switch
            {
                "tenlophocphan" => desc ? baseQ.OrderByDescending(x => x.l.TenLopHocPhan) : baseQ.OrderBy(x => x.l.TenLopHocPhan),
                "trangthai" => desc ? baseQ.OrderByDescending(x => x.l.TrangThai) : baseQ.OrderBy(x => x.l.TrangThai),
                "mamonhoc" => desc ? baseQ.OrderByDescending(x => x.m.MaMonHoc) : baseQ.OrderBy(x => x.m.MaMonHoc),
                "tenmonhoc" => desc ? baseQ.OrderByDescending(x => x.m.TenMonHoc) : baseQ.OrderBy(x => x.m.TenMonHoc),
                "sotinchi" => desc ? baseQ.OrderByDescending(x => x.m.SoTinChi) : baseQ.OrderBy(x => x.m.SoTinChi),
                "sotiet" => desc ? baseQ.OrderByDescending(x => x.m.SoTiet) : baseQ.OrderBy(x => x.m.SoTiet),
                "hocky" => desc ? baseQ.OrderByDescending(x => x.m.HocKy) : baseQ.OrderBy(x => x.m.HocKy),
                "magiangvien" => desc ? baseQ.OrderByDescending(x => x.gv.MaGiangVien) : baseQ.OrderBy(x => x.gv.MaGiangVien),
                "tengiangvien" => desc ? baseQ.OrderByDescending(x => x.nd.HoTen) : baseQ.OrderBy(x => x.nd.HoTen),
                "malophocphan" or _ => desc ? baseQ.OrderByDescending(x => x.l.MaLopHocPhan) : baseQ.OrderBy(x => x.l.MaLopHocPhan),
            };

            // ===== Tham gia: nếu có MaSinhVien -> join ThamGiaLop (lọc theo SV); nếu không -> null fields =====
            var qFinalQuery = !string.IsNullOrWhiteSpace(maSinhVien)
                ? (
                    from x in baseQ
                    join t in _db.ThamGiaLop.AsNoTracking()
                         on x.l.MaLopHocPhan equals t.MaLopHocPhan
                    where (t.MaSinhVien ?? "").Replace(" ", "") == maSinhVien.Trim().Replace(" ", "")
                    select new { l = x.l, m = x.m, gv = x.gv, nd = x.nd, Ngay = t.NgayThamGia, TrangThai = t.TrangThai }
                  )
                : (
                    from x in baseQ
                    select new { l = x.l, m = x.m, gv = x.gv, nd = x.nd, Ngay = (DateTime?)null, TrangThai = (bool?)null }
                  );
            
            var total = await qFinalQuery.CountAsync();
            
            var list = await qFinalQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            var items = list
                .Select(x => (x.l, x.m, x.gv, x.nd, x.Ngay, x.TrangThai)) // (Lhp, Mh, Gv, Nd, NgayThamGia?, TrangThaiThamGia?)
                .ToList();

            return (items, total);
        }

        public async Task<(List<MonHoc> Items, int Total)> SearchSubjectsAsync(
            string? keyword,
            string? maMonHoc,
            string? tenMonHoc,
            byte? soTinChi,
            byte? soTiet,
            byte? hocKy,
            bool? trangThai,
            string? sortBy,
            bool desc,
            int page,
            int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

            var q = _db.MonHoc.AsNoTracking().AsQueryable();

            // ===== Filters =====
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                q = q.Where(m =>
                    (m.MaMonHoc ?? "").ToLower().Contains(kw) ||
                    (m.TenMonHoc ?? "").ToLower().Contains(kw) ||
                    (m.MoTa ?? "").ToLower().Contains(kw)
                );
            }

            if (!string.IsNullOrWhiteSpace(maMonHoc))
            {
                var code = maMonHoc.Trim().Replace(" ", "");
                q = q.Where(m => ((m.MaMonHoc ?? "").Replace(" ", "")) == code);
            }

            if (!string.IsNullOrWhiteSpace(tenMonHoc))
                q = q.Where(m => (m.TenMonHoc ?? "").ToLower().Contains(tenMonHoc.Trim().ToLower()));

            if (soTinChi.HasValue) q = q.Where(m => (m.SoTinChi ?? 0) == soTinChi.Value);
            if (soTiet.HasValue) q = q.Where(m => (m.SoTiet ?? 0) == soTiet.Value);
            if (hocKy.HasValue) q = q.Where(m => m.HocKy == hocKy.Value);
            if (trangThai.HasValue) q = q.Where(m => (m.TrangThai ?? true) == trangThai.Value);

            // ===== Sorting =====
            var key = (sortBy ?? "MaMonHoc").Trim().ToLowerInvariant();
            q = key switch
            {
                "tenmonhoc" => desc ? q.OrderByDescending(m => m.TenMonHoc) : q.OrderBy(m => m.TenMonHoc),
                "sotinchi" => desc ? q.OrderByDescending(m => m.SoTinChi) : q.OrderBy(m => m.SoTinChi),
                "sotiet" => desc ? q.OrderByDescending(m => m.SoTiet) : q.OrderBy(m => m.SoTiet),
                "hocky" => desc ? q.OrderByDescending(m => m.HocKy) : q.OrderBy(m => m.HocKy),
                "trangthai" => desc ? q.OrderByDescending(m => m.TrangThai) : q.OrderBy(m => m.TrangThai),
                "mamonhoc" or _
                             => desc ? q.OrderByDescending(m => m.MaMonHoc) : q.OrderBy(m => m.MaMonHoc),
            };

            // ===== Paging =====
            var total = await q.CountAsync();
            var items = await q.Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

            return (items, total);
        }
    }
}
