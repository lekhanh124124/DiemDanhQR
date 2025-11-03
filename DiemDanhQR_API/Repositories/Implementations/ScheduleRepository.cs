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
                // string? keyword, // removed
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
                join p in _db.PhongHoc.AsNoTracking() on b.MaPhong equals p.MaPhong
                join l in _db.LopHocPhan.AsNoTracking() on b.MaLopHocPhan equals l.MaLopHocPhan
                join m in _db.MonHoc.AsNoTracking() on l.MaMonHoc equals m.MaMonHoc
                join gv in _db.GiangVien.AsNoTracking() on l.MaGiangVien equals gv.MaGiangVien
                join ndGv in _db.NguoiDung.AsNoTracking() on gv.MaNguoiDung equals ndGv.MaNguoiDung
                select new { b, p, l, m, gv, ndGv };

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
                "maphong" => desc ? q.OrderByDescending(x => x.p.MaPhong) : q.OrderBy(x => x.p.MaPhong),
                "tenphong" => desc ? q.OrderByDescending(x => x.p.TenPhong) : q.OrderBy(x => x.p.TenPhong),
                "malophocphan" => desc ? q.OrderByDescending(x => x.l.MaLopHocPhan) : q.OrderBy(x => x.l.MaLopHocPhan),
                "tenlop" => desc ? q.OrderByDescending(x => x.l.TenLopHocPhan) : q.OrderBy(x => x.l.TenLopHocPhan),
                "tenmonhoc" => desc ? q.OrderByDescending(x => x.m.TenMonHoc) : q.OrderBy(x => x.m.TenMonHoc),
                "ngayhoc" => desc ? q.OrderByDescending(x => x.b.NgayHoc) : q.OrderBy(x => x.b.NgayHoc),
                "tietbatdau" => desc ? q.OrderByDescending(x => x.b.TietBatDau) : q.OrderBy(x => x.b.TietBatDau),
                "sotiet" => desc ? q.OrderByDescending(x => x.b.SoTiet) : q.OrderBy(x => x.b.SoTiet),
                "ghichu" => desc ? q.OrderByDescending(x => x.b.GhiChu) : q.OrderBy(x => x.b.GhiChu),
                "magiangvien" => desc ? q.OrderByDescending(x => x.gv.MaGiangVien) : q.OrderBy(x => x.gv.MaGiangVien),
                "trangthai" => desc ? q.OrderByDescending(x => x.b.TrangThai) : q.OrderBy(x => x.b.TrangThai),
                "mabuoi" or _ => desc ? q.OrderByDescending(x => x.b.MaBuoi) : q.OrderBy(x => x.b.MaBuoi),
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

        public async Task<(List<PhongHoc> Items, int Total)> SearchRoomsAsync(
            // string? keyword, // removed
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
                q = q.Where(p => (p.TenPhong ?? "").ToLower().Contains(tenPhong.Trim().ToLower()));
            if (!string.IsNullOrWhiteSpace(toaNha))
                q = q.Where(p => (p.ToaNha ?? "").ToLower().Contains(toaNha.Trim().ToLower()));

            if (tang.HasValue) q = q.Where(p => (p.Tang ?? 0) == tang.Value);     // byte compare
            if (sucChua.HasValue) q = q.Where(p => (p.SucChua ?? 0) == sucChua.Value);  // byte compare
            if (trangThai.HasValue) q = q.Where(p => (p.TrangThai ?? true) == trangThai.Value);

            var key = (sortBy ?? "MaPhong").Trim().ToLowerInvariant();
            q = key switch
            {
                "tenphong" => desc ? q.OrderByDescending(p => p.TenPhong) : q.OrderBy(p => p.TenPhong),
                "toanha" => desc ? q.OrderByDescending(p => p.ToaNha) : q.OrderBy(p => p.ToaNha),
                "tang" => desc ? q.OrderByDescending(p => p.Tang) : q.OrderBy(p => p.Tang),
                "succhua" => desc ? q.OrderByDescending(p => p.SucChua) : q.OrderBy(p => p.SucChua),
                "trangthai" => desc ? q.OrderByDescending(p => p.TrangThai) : q.OrderBy(p => p.TrangThai),
                "maphong" or _
                            => desc ? q.OrderByDescending(p => p.MaPhong) : q.OrderBy(p => p.MaPhong),
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

        // filepath change: thay WriteActivityLogAsync bằng LogActivityAsync với TenDangNhap
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
                ThoiGian = DateTime.Now
            });
            await _db.SaveChangesAsync();
        }

        public async Task<bool> CourseExistsByCodeAsync(string maLopHocPhan)
        {
            var code = (maLopHocPhan ?? "").Trim();
            return await _db.LopHocPhan.AsNoTracking()
                .AnyAsync(l => (l.MaLopHocPhan ?? "") == code);
        }

        public async Task<bool> RoomExistsByIdAsync(int maPhong)
        {
            return await _db.PhongHoc.AsNoTracking()
                .AnyAsync(p => (p.MaPhong ?? 0) == maPhong);
        }

        public async Task<bool> ScheduleExistsAsync(string maLopHocPhan, DateTime ngayHoc, byte tietBatDau)
        {
            var code = (maLopHocPhan ?? "").Trim();
            var d = ngayHoc.Date;
            return await _db.BuoiHoc.AsNoTracking()
                .AnyAsync(b => (b.MaLopHocPhan ?? "") == code
                               && b.NgayHoc == d
                               && (b.TietBatDau ?? 0) == tietBatDau);
        }

        public async Task AddScheduleAsync(BuoiHoc buoi)
        {
            _db.BuoiHoc.Add(buoi);
            await _db.SaveChangesAsync();
        }

        public async Task<PhongHoc?> GetRoomByIdAsync(int maPhong)
        {
            return await _db.PhongHoc.AsNoTracking()
                .FirstOrDefaultAsync(p => (p.MaPhong ?? 0) == maPhong);
        }

        public async Task<PhongHoc?> GetRoomForUpdateAsync(int maPhong)
        {
            return await _db.PhongHoc.FirstOrDefaultAsync(p => (p.MaPhong ?? 0) == maPhong);
        }

        public async Task<bool> RoomNameExistsExceptIdAsync(string tenPhong, int excludeMaPhong)
        {
            var name = (tenPhong ?? "").Trim().ToLower();
            return await _db.PhongHoc.AsNoTracking()
                .AnyAsync(p => (p.TenPhong ?? "").ToLower() == name && (p.MaPhong ?? 0) != excludeMaPhong);
        }

        public async Task UpdateRoomAsync(PhongHoc room)
        {
            _db.PhongHoc.Update(room);
            await _db.SaveChangesAsync();
        }

        public async Task<BuoiHoc?> GetScheduleByIdAsync(int maBuoi)
        {
            return await _db.BuoiHoc.FirstOrDefaultAsync(b => (b.MaBuoi ?? 0) == maBuoi);
        }

        public async Task UpdateScheduleAsync(BuoiHoc buoi)
        {
            _db.BuoiHoc.Update(buoi);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> ScheduleExistsAsync(string maLopHocPhan, DateTime ngayHoc, byte tietBatDau, int excludeMaBuoi)
        {
            var code = (maLopHocPhan ?? "").Trim();
            var d = ngayHoc.Date;
            return await _db.BuoiHoc.AsNoTracking()
                .AnyAsync(b => (b.MaLopHocPhan ?? "") == code
                               && b.NgayHoc == d
                               && (b.TietBatDau ?? 0) == tietBatDau
                               && (b.MaBuoi ?? 0) != excludeMaBuoi);
        }
    }
}
