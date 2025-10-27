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
            string? maMonHoc,
            string? maGiangVien,
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
                    (x.m.MaMonHoc ?? "").ToLower().Contains(kw) ||     // keyword bắt cả mã MH
                    (x.gv.MaGiangVien ?? "").ToLower().Contains(kw) || // và mã GV
                    (x.nd.HoTen ?? "").ToLower().Contains(kw)
                );
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

            if (trangThai.HasValue)
                q = q.Where(x => (x.l.TrangThai ?? true) == trangThai.Value);

            if (!string.IsNullOrWhiteSpace(tenMonHoc))
            {
                var s = tenMonHoc.Trim().ToLower();
                q = q.Where(x => (x.m.TenMonHoc ?? "").ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(maMonHoc))                     // NEW
            {
                var code = maMonHoc.Trim().Replace(" ", "");
                q = q.Where(x => ((x.m.MaMonHoc ?? "").Replace(" ", "")) == code);
            }

            if (soTinChi.HasValue) q = q.Where(x => (x.m.SoTinChi ?? 0) == soTinChi.Value);
            if (soTiet.HasValue) q = q.Where(x => (x.m.SoTiet ?? 0) == soTiet.Value);
            if (hocKy.HasValue) q = q.Where(x => x.m.HocKy == hocKy.Value);

            if (!string.IsNullOrWhiteSpace(tenGiangVien))
            {
                var s = tenGiangVien.Trim().ToLower();
                q = q.Where(x => (x.nd.HoTen ?? "").ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(maGiangVien))                  // NEW
            {
                var code = maGiangVien.Trim().Replace(" ", "");
                q = q.Where(x => ((x.gv.MaGiangVien ?? "").Replace(" ", "")) == code);
            }

            // ===== Sorting =====
            var key = (sortBy ?? "MaLopHocPhan").Trim().ToLowerInvariant();
            q = key switch
            {
                "tenlophocphan" => desc ? q.OrderByDescending(x => x.l.TenLopHocPhan) : q.OrderBy(x => x.l.TenLopHocPhan),
                "trangthai" => desc ? q.OrderByDescending(x => x.l.TrangThai) : q.OrderBy(x => x.l.TrangThai),
                "mamonhoc" => desc ? q.OrderByDescending(x => x.m.MaMonHoc) : q.OrderBy(x => x.m.MaMonHoc),
                "tenmonhoc" => desc ? q.OrderByDescending(x => x.m.TenMonHoc) : q.OrderBy(x => x.m.TenMonHoc),
                "sotinchi" => desc ? q.OrderByDescending(x => x.m.SoTinChi) : q.OrderBy(x => x.m.SoTinChi),
                "sotiet" => desc ? q.OrderByDescending(x => x.m.SoTiet) : q.OrderBy(x => x.m.SoTiet),
                "hocky" => desc ? q.OrderByDescending(x => x.m.HocKy) : q.OrderBy(x => x.m.HocKy),
                "magiangvien" => desc ? q.OrderByDescending(x => x.gv.MaGiangVien) : q.OrderBy(x => x.gv.MaGiangVien),
                "tengiangvien" => desc ? q.OrderByDescending(x => x.nd.HoTen) : q.OrderBy(x => x.nd.HoTen),
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

        public async Task<(List<(ThamGiaLop Tgl, LopHocPhan Lhp, MonHoc Mh, SinhVien Sv, NguoiDung NdSv, GiangVien Gv, NguoiDung NdGv)> Items, int Total)>
            SearchCourseParticipantsAsync(
                string? keyword,
                string? maLopHocPhan,
                string? tenLopHocPhan,
                string? maMonHoc,
                string? tenMonHoc,
                byte? hocKy,
                string? maSinhVien,
                string? tenSinhVien,
                DateTime? ngayFrom,
                DateTime? ngayTo,
                bool? trangThaiThamGia,
                string? maGiangVien,
                string? tenGiangVien,
                string? sortBy,
                bool desc,
                int page,
                int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

            var q =
                from t in _db.ThamGiaLop.AsNoTracking()
                join l in _db.LopHocPhan.AsNoTracking() on t.MaLopHocPhan equals l.MaLopHocPhan
                join m in _db.MonHoc.AsNoTracking() on l.MaMonHoc equals m.MaMonHoc
                join sv in _db.SinhVien.AsNoTracking() on t.MaSinhVien equals sv.MaSinhVien
                join ndSv in _db.NguoiDung.AsNoTracking() on sv.MaNguoiDung equals ndSv.MaNguoiDung
                join gv in _db.GiangVien.AsNoTracking() on l.MaGiangVien equals gv.MaGiangVien
                join ndGv in _db.NguoiDung.AsNoTracking() on gv.MaNguoiDung equals ndGv.MaNguoiDung
                select new { t, l, m, sv, ndSv, gv, ndGv };

            // ===== Filters =====
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                q = q.Where(x =>
                    (x.l.MaLopHocPhan ?? "").ToLower().Contains(kw) ||
                    (x.l.TenLopHocPhan ?? "").ToLower().Contains(kw) ||
                    (x.m.MaMonHoc ?? "").ToLower().Contains(kw) ||
                    (x.m.TenMonHoc ?? "").ToLower().Contains(kw) ||
                    (x.sv.MaSinhVien ?? "").ToLower().Contains(kw) ||
                    (x.ndSv.HoTen ?? "").ToLower().Contains(kw) ||
                    (x.gv.MaGiangVien ?? "").ToLower().Contains(kw) ||
                    (x.ndGv.HoTen ?? "").ToLower().Contains(kw)
                );
            }

            if (!string.IsNullOrWhiteSpace(maLopHocPhan))
            {
                var code = maLopHocPhan.Trim().Replace(" ", "");
                q = q.Where(x => ((x.l.MaLopHocPhan ?? "").Replace(" ", "")) == code);
            }

            if (!string.IsNullOrWhiteSpace(tenLopHocPhan))
                q = q.Where(x => (x.l.TenLopHocPhan ?? "").ToLower().Contains(tenLopHocPhan.Trim().ToLower()));

            if (!string.IsNullOrWhiteSpace(maMonHoc))
            {
                var code = maMonHoc.Trim().Replace(" ", "");
                q = q.Where(x => ((x.m.MaMonHoc ?? "").Replace(" ", "")) == code);
            }

            if (!string.IsNullOrWhiteSpace(tenMonHoc))
                q = q.Where(x => (x.m.TenMonHoc ?? "").ToLower().Contains(tenMonHoc.Trim().ToLower()));

            if (hocKy.HasValue)
                q = q.Where(x => x.m.HocKy == hocKy.Value);

            if (!string.IsNullOrWhiteSpace(maSinhVien))
            {
                var code = maSinhVien.Trim().Replace(" ", "");
                q = q.Where(x => ((x.sv.MaSinhVien ?? "").Replace(" ", "")) == code);
            }

            if (!string.IsNullOrWhiteSpace(tenSinhVien))
                q = q.Where(x => (x.ndSv.HoTen ?? "").ToLower().Contains(tenSinhVien.Trim().ToLower()));

            if (ngayFrom.HasValue) q = q.Where(x => x.t.NgayThamGia >= ngayFrom.Value.Date);
            if (ngayTo.HasValue) q = q.Where(x => x.t.NgayThamGia <= ngayTo.Value.Date);

            if (trangThaiThamGia.HasValue)
                q = q.Where(x => (x.t.TrangThai ?? true) == trangThaiThamGia.Value);

            if (!string.IsNullOrWhiteSpace(maGiangVien))
            {
                var code = maGiangVien.Trim().Replace(" ", "");
                q = q.Where(x => ((x.gv.MaGiangVien ?? "").Replace(" ", "")) == code);
            }

            if (!string.IsNullOrWhiteSpace(tenGiangVien))
                q = q.Where(x => (x.ndGv.HoTen ?? "").ToLower().Contains(tenGiangVien.Trim().ToLower()));

            // ===== Sorting =====
            var key = (sortBy ?? "MaLopHocPhan").Trim().ToLowerInvariant();
            q = key switch
            {
                "tenlophocphan" => desc ? q.OrderByDescending(x => x.l.TenLopHocPhan) : q.OrderBy(x => x.l.TenLopHocPhan),
                "mamonhoc" => desc ? q.OrderByDescending(x => x.m.MaMonHoc) : q.OrderBy(x => x.m.MaMonHoc),
                "tenmonhoc" => desc ? q.OrderByDescending(x => x.m.TenMonHoc) : q.OrderBy(x => x.m.TenMonHoc),
                "hocky" => desc ? q.OrderByDescending(x => x.m.HocKy) : q.OrderBy(x => x.m.HocKy),   // NEW
                "masinhvien" => desc ? q.OrderByDescending(x => x.sv.MaSinhVien) : q.OrderBy(x => x.sv.MaSinhVien),
                "tensinhvien" => desc ? q.OrderByDescending(x => x.ndSv.HoTen) : q.OrderBy(x => x.ndSv.HoTen),
                "ngaythamgia" => desc ? q.OrderByDescending(x => x.t.NgayThamGia) : q.OrderBy(x => x.t.NgayThamGia),
                "trangthaithamgia" => desc ? q.OrderByDescending(x => x.t.TrangThai) : q.OrderBy(x => x.t.TrangThai),
                "magiangvien" => desc ? q.OrderByDescending(x => x.gv.MaGiangVien) : q.OrderBy(x => x.gv.MaGiangVien),
                "tengiangvien" => desc ? q.OrderByDescending(x => x.ndGv.HoTen) : q.OrderBy(x => x.ndGv.HoTen),
                "malophocphan" or _
                                   => desc ? q.OrderByDescending(x => x.l.MaLopHocPhan) : q.OrderBy(x => x.l.MaLopHocPhan),
            };


            // ===== Paging =====
            var total = await q.CountAsync();

            var list = await q.Skip((page - 1) * pageSize)
                              .Take(pageSize)
                              .Select(x => new { x.t, x.l, x.m, x.sv, x.ndSv, x.gv, x.ndGv })
                              .ToListAsync();

            var items = list.Select(x => (x.t, x.l, x.m, x.sv, x.ndSv, x.gv, x.ndGv)).ToList();
            return (items, total);
        }
    }
}
