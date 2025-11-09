// File: Repositories/Implementations/CourseRepository.cs
using api.Data;
using api.Helpers;
using api.Models;
using api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories.Implementations
{
    public class CourseRepository : ICourseRepository
    {
        private readonly AppDbContext _db;
        public CourseRepository(AppDbContext db) => _db = db;

        public async Task<(List<(LopHocPhan Lhp, MonHoc Mh, GiangVien Gv, HocKy Hk, DateOnly? NgayThamGia, bool? TrangThaiThamGia)> Items, int Total)>
            SearchCoursesAsync(
                string? maLopHocPhan,
                string? tenLopHocPhan,
                bool? trangThai,
                string? maMonHoc,
                byte? soTinChi,
                string? maGiangVien,
                int? maHocKy,
                short? namHoc,
                byte? ky,
                string? maSinhVien,
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
                join gv in _db.GiangVien.AsNoTracking() on l.MaGiangVien equals gv.MaGiangVien into gj
                from gv in gj.DefaultIfEmpty()
                join hk in _db.HocKy.AsNoTracking() on l.MaHocKy equals hk.MaHocKy
                select new { l, m, gv, hk };

            if (!string.IsNullOrWhiteSpace(maLopHocPhan))
            {
                var code = maLopHocPhan.Trim().Replace(" ", "");
                baseQ = baseQ.Where(x => ((x.l.MaLopHocPhan ?? "").Replace(" ", "")) == code);
            }
            if (!string.IsNullOrWhiteSpace(tenLopHocPhan))
                baseQ = baseQ.Where(x => (x.l.TenLopHocPhan ?? "").ToLower().Contains(tenLopHocPhan.Trim().ToLower()));

            if (trangThai.HasValue) baseQ = baseQ.Where(x => (x.l.TrangThai) == trangThai.Value);

            if (!string.IsNullOrWhiteSpace(maMonHoc))
            {
                var code = maMonHoc.Trim().Replace(" ", "");
                baseQ = baseQ.Where(x => ((x.m.MaMonHoc ?? "").Replace(" ", "")) == code);
            }
            if (soTinChi.HasValue) baseQ = baseQ.Where(x => x.m.SoTinChi == soTinChi.Value);

            if (!string.IsNullOrWhiteSpace(maGiangVien))
            {
                var code = maGiangVien.Trim().Replace(" ", "");
                baseQ = baseQ.Where(x => ((x.gv.MaGiangVien ?? "").Replace(" ", "")) == code);
            }

            if (maHocKy.HasValue) baseQ = baseQ.Where(x => x.hk.MaHocKy == maHocKy.Value);
            if (namHoc.HasValue) baseQ = baseQ.Where(x => x.hk.NamHoc == namHoc.Value);
            if (ky.HasValue) baseQ = baseQ.Where(x => x.hk.Ky == ky.Value);

            var key = (sortBy ?? "MaLopHocPhan").Trim().ToLowerInvariant();
            baseQ = key switch
            {
                "tenlophocphan" => desc ? baseQ.OrderByDescending(x => x.l.TenLopHocPhan) : baseQ.OrderBy(x => x.l.TenLopHocPhan),
                "trangthai" => desc ? baseQ.OrderByDescending(x => x.l.TrangThai) : baseQ.OrderBy(x => x.l.TrangThai),
                "mamonhoc" => desc ? baseQ.OrderByDescending(x => x.m.MaMonHoc) : baseQ.OrderBy(x => x.m.MaMonHoc),
                "sotinchi" => desc ? baseQ.OrderByDescending(x => x.m.SoTinChi) : baseQ.OrderBy(x => x.m.SoTinChi),
                "magiangvien" => desc ? baseQ.OrderByDescending(x => x.gv.MaGiangVien) : baseQ.OrderBy(x => x.gv.MaGiangVien),
                "namhoc" => desc ? baseQ.OrderByDescending(x => x.hk.NamHoc) : baseQ.OrderBy(x => x.hk.NamHoc),
                "ky" => desc ? baseQ.OrderByDescending(x => x.hk.Ky) : baseQ.OrderBy(x => x.hk.Ky),
                "malophocphan" or _ => desc ? baseQ.OrderByDescending(x => x.l.MaLopHocPhan) : baseQ.OrderBy(x => x.l.MaLopHocPhan),
            };

            var qFinal = !string.IsNullOrWhiteSpace(maSinhVien)
                ? (
                    from x in baseQ
                    join t in _db.ThamGiaLop.AsNoTracking()
                        on x.l.MaLopHocPhan equals t.MaLopHocPhan
                    where (t.MaSinhVien ?? "").Replace(" ", "") == (maSinhVien!.Trim().Replace(" ", ""))
                    select new { x.l, x.m, x.gv, x.hk, Ngay = (DateOnly?)t.NgayThamGia, TrangThai = (bool?)t.TrangThai }
                  )
                : (
                    from x in baseQ
                    select new { x.l, x.m, x.gv, x.hk, Ngay = (DateOnly?)null, TrangThai = (bool?)null }
                  );

            var total = await qFinal.CountAsync();

            var list = await qFinal
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = list.Select(x => (x.l, x.m, x.gv, x.hk, x.Ngay, x.TrangThai)).ToList();
            return (items, total);
        }

        public async Task<(List<MonHoc> Items, int Total)> SearchSubjectsAsync(
            string? maMonHoc,
            string? tenMonHoc,
            byte? soTinChi,
            byte? soTiet,
            bool? trangThai,
            string? sortBy,
            bool desc,
            int page,
            int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

            var q = _db.MonHoc.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(maMonHoc))
            {
                var code = maMonHoc.Trim().Replace(" ", "");
                q = q.Where(m => ((m.MaMonHoc ?? "").Replace(" ", "")) == code);
            }

            if (!string.IsNullOrWhiteSpace(tenMonHoc))
                q = q.Where(m => (m.TenMonHoc ?? "").ToLower().Contains(tenMonHoc.Trim().ToLower()));

            if (soTinChi.HasValue) q = q.Where(m => m.SoTinChi == soTinChi.Value);
            if (soTiet.HasValue) q = q.Where(m => m.SoTiet == soTiet.Value);
            if (trangThai.HasValue) q = q.Where(m => m.TrangThai == trangThai.Value);

            var key = (sortBy ?? "MaMonHoc").Trim().ToLowerInvariant();
            q = key switch
            {
                "tenmonhoc" => desc ? q.OrderByDescending(m => m.TenMonHoc) : q.OrderBy(m => m.TenMonHoc),
                "sotinchi" => desc ? q.OrderByDescending(m => m.SoTinChi) : q.OrderBy(m => m.SoTinChi),
                "sotiet" => desc ? q.OrderByDescending(m => m.SoTiet) : q.OrderBy(m => m.SoTiet),
                "trangthai" => desc ? q.OrderByDescending(m => m.TrangThai) : q.OrderBy(m => m.TrangThai),
                "mamonhoc" or _ => desc ? q.OrderByDescending(m => m.MaMonHoc) : q.OrderBy(m => m.MaMonHoc),
            };

            var total = await q.CountAsync();
            var items = await q.Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

            return (items, total);
        }

        public async Task<bool> SubjectExistsAsync(string maMonHoc)
        {
            if (string.IsNullOrWhiteSpace(maMonHoc)) return false;
            var code = maMonHoc.Trim();
            return await _db.MonHoc.AsNoTracking().AnyAsync(m => (m.MaMonHoc ?? "") == code);
        }

        public async Task AddSubjectAsync(MonHoc subject)
        {
            _db.MonHoc.Add(subject);
            await _db.SaveChangesAsync();
        }

        public Task<MonHoc?> GetSubjectByCodeAsync(string maMonHoc)
        {
            var code = (maMonHoc ?? "").Trim();
            return _db.MonHoc.FirstOrDefaultAsync(m => (m.MaMonHoc ?? "") == code);
        }

        public async Task UpdateSubjectAsync(MonHoc subject)
        {
            _db.MonHoc.Update(subject);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> CourseExistsAsync(string maLopHocPhan)
        {
            if (string.IsNullOrWhiteSpace(maLopHocPhan)) return false;
            var code = maLopHocPhan.Trim();
            return await _db.LopHocPhan.AsNoTracking().AnyAsync(l => (l.MaLopHocPhan ?? "") == code);
        }

        public async Task AddCourseAsync(LopHocPhan course)
        {
            _db.LopHocPhan.Add(course);
            await _db.SaveChangesAsync();
        }

        public Task<LopHocPhan?> GetCourseByCodeAsync(string maLopHocPhan)
        {
            var code = (maLopHocPhan ?? "").Trim();
            return _db.LopHocPhan.FirstOrDefaultAsync(l => (l.MaLopHocPhan ?? "") == code);
        }

        public async Task UpdateCourseAsync(LopHocPhan course)
        {
            _db.LopHocPhan.Update(course);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> LecturerExistsByCodeAsync(string maGiangVien)
        {
            if (string.IsNullOrWhiteSpace(maGiangVien)) return false;
            var code = maGiangVien.Trim();
            return await _db.GiangVien.AsNoTracking().AnyAsync(g => (g.MaGiangVien ?? "") == code);
        }

        public async Task<bool> SemesterExistsByIdAsync(int maHocKy)
        {
            return await _db.HocKy.AsNoTracking().AnyAsync(h => h.MaHocKy == maHocKy);
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
                ThoiGian = TimeHelper.UtcToVietnam(DateTime.UtcNow)
            });
            await _db.SaveChangesAsync();
        }

        public async Task<(List<HocKy> Items, int Total)> SearchSemestersAsync(
            short? namHoc,
            byte? ky,
            string? sortBy,
            bool desc,
            int page,
            int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

            var q = _db.HocKy.AsNoTracking().AsQueryable();

            if (namHoc.HasValue) q = q.Where(x => x.NamHoc == namHoc.Value);
            if (ky.HasValue) q = q.Where(x => x.Ky == ky.Value);

            var key = (sortBy ?? "MaHocKy").Trim().ToLowerInvariant();
            q = key switch
            {
                "namhoc" => (desc ? q.OrderByDescending(x => x.NamHoc) : q.OrderBy(x => x.NamHoc)),
                "ky" => (desc ? q.OrderByDescending(x => x.Ky) : q.OrderBy(x => x.Ky)),
                "mahocky" or _ => (desc ? q.OrderByDescending(x => x.MaHocKy) : q.OrderBy(x => x.MaHocKy)),
            };

            var total = await q.CountAsync();
            var items = await q.Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

            return (items, total);
        }

        public async Task<bool> ExistsSemesterAsync(short? namHoc, byte? ky, int? excludeId = null)
        {
            var q = _db.HocKy.AsNoTracking().Where(x => x.NamHoc == namHoc && x.Ky == ky);
            if (excludeId.HasValue) q = q.Where(x => x.MaHocKy != excludeId.Value);
            return await q.AnyAsync();
        }

        public async Task AddSemesterAsync(HocKy hk)
        {
            _db.HocKy.Add(hk);
            await _db.SaveChangesAsync();
        }

        public Task<HocKy?> GetSemesterByIdAsync(int maHocKy)
            => _db.HocKy.FirstOrDefaultAsync(x => x.MaHocKy == maHocKy);

        public async Task UpdateSemesterAsync(HocKy hk)
        {
            _db.HocKy.Update(hk);
            await _db.SaveChangesAsync();
        }
    }
}
