// File: Repositories/Implementations/AttendanceRepository.cs
using api.Data;
using api.Models;
using api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories.Implementations
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly AppDbContext _db;
        public AttendanceRepository(AppDbContext db) => _db = db;

        public async Task<BuoiHoc?> GetActiveBuoiByIdAsync(int maBuoi)
            => await _db.BuoiHoc.AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaBuoi == maBuoi && x.TrangThai == true);

        public async Task<BuoiHoc?> GetBuoiByIdAsync(int maBuoi)
            => await _db.BuoiHoc.FirstOrDefaultAsync(x => x.MaBuoi == maBuoi);

        public async Task<LopHocPhan?> GetLopHocPhanByIdAsync(string maLopHocPhan)
            => await _db.LopHocPhan.AsNoTracking().FirstOrDefaultAsync(x => x.MaLopHocPhan == maLopHocPhan);

        public async Task<bool> SaveChangesAsync() => (await _db.SaveChangesAsync()) > 0;

        public async Task<bool> IsLopHocPhanActiveAsync(string maLopHocPhan)
            => await _db.LopHocPhan.AsNoTracking()
                .AnyAsync(x => x.MaLopHocPhan == maLopHocPhan && x.TrangThai == true);

        public async Task<NguoiDung?> GetNguoiDungByUsernameAsync(string username)
            => await _db.NguoiDung.AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenDangNhap == username && x.TrangThai == true);

        public async Task<SinhVien?> GetSinhVienByMaNguoiDungAsync(int maNguoiDung)
            => await _db.SinhVien.AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaNguoiDung == maNguoiDung);

        public async Task<bool> IsSinhVienInActiveLopAsync(string maLopHocPhan, string maSinhVien)
            => await _db.ThamGiaLop.AsNoTracking()
                .AnyAsync(x => x.MaLopHocPhan == maLopHocPhan && x.MaSinhVien == maSinhVien && x.TrangThai == true);

        public async Task<ThamGiaLop?> GetThamGiaLopAsync(string maLopHocPhan, string maSinhVien)
            => await _db.ThamGiaLop.AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaLopHocPhan == maLopHocPhan && x.MaSinhVien == maSinhVien);


        public async Task<bool> AttendanceExistsAsync(int maBuoi, string maSinhVien)
            => await _db.DiemDanh.AsNoTracking()
                .AnyAsync(x => x.MaBuoi == maBuoi && x.MaSinhVien == maSinhVien);

        public async Task<DiemDanh> CreateAttendanceAsync(DiemDanh entity)
        {
            _db.DiemDanh.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<DiemDanh?> GetAttendanceByIdAsync(int id)
            => await _db.DiemDanh.FirstOrDefaultAsync(x => x.MaDiemDanh == id);

        public async Task UpdateAttendanceAsync(DiemDanh entity)
        {
            _db.DiemDanh.Update(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<int?> TryGetTrangThaiIdByCodeAsync(string code)
        {
            var item = await _db.TrangThaiDiemDanh.AsNoTracking()
                .FirstOrDefaultAsync(x => x.CodeTrangThai == code);
            return item?.MaTrangThai;
        }

        public async Task<TrangThaiDiemDanh?> GetStatusByIdAsync(int id)
            => await _db.TrangThaiDiemDanh.FirstOrDefaultAsync(x => x.MaTrangThai == id);

        public async Task<bool> StatusCodeExistsAsync(string code, int? excludeId = null)
        {
            var q = _db.TrangThaiDiemDanh.AsNoTracking().Where(x => x.CodeTrangThai == code);
            if (excludeId.HasValue) q = q.Where(x => x.MaTrangThai != excludeId.Value);
            return await q.AnyAsync();
        }

        public async Task<bool> IsStatusInUseAsync(int id)
            => await _db.DiemDanh.AsNoTracking().AnyAsync(x => x.MaTrangThai == id);

        public async Task<TrangThaiDiemDanh> CreateStatusAsync(TrangThaiDiemDanh entity)
        {
            _db.TrangThaiDiemDanh.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateStatusAsync(TrangThaiDiemDanh entity)
        {
            _db.TrangThaiDiemDanh.Update(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> DeleteStatusAsync(int id)
        {
            var found = await _db.TrangThaiDiemDanh.FirstOrDefaultAsync(x => x.MaTrangThai == id);
            if (found == null) return false;
            _db.TrangThaiDiemDanh.Remove(found);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task LogHistoryAsync(int? maNguoiDung, string action)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(action)) return;
                var ls = new LichSuHoatDong
                {
                    // Ghi DB = giờ Việt Nam (không format)
                    ThoiGian = Helpers.TimeHelper.UtcToVietnam(DateTime.UtcNow),
                    HanhDong = action,
                    MaNguoiDung = maNguoiDung
                };
                _db.LichSuHoatDong.Add(ls);
                await _db.SaveChangesAsync();
            }
            catch { /* không chặn flow chính */ }
        }
        public async Task<(List<TrangThaiDiemDanh> Items, int Total)> SearchStatusesAsync(
                   int? maTrangThai,
                   string? tenTrangThai,
                   string? codeTrangThai,
                   string? sortBy,
                   bool desc,
                   int page,
                   int pageSize)
        {
            var q = _db.TrangThaiDiemDanh.AsNoTracking().AsQueryable();

            if (maTrangThai.HasValue)
                q = q.Where(x => x.MaTrangThai == maTrangThai.Value);

            if (!string.IsNullOrWhiteSpace(tenTrangThai))
                q = q.Where(x => x.TenTrangThai != null && EF.Functions.Like(x.TenTrangThai, $"%{tenTrangThai}%"));

            if (!string.IsNullOrWhiteSpace(codeTrangThai))
                q = q.Where(x => x.CodeTrangThai != null && EF.Functions.Like(x.CodeTrangThai, $"%{codeTrangThai}%"));

            var key = (sortBy ?? "MaTrangThai").Trim().ToLowerInvariant();
            q = key switch
            {
                "tentrangthai" => desc ? q.OrderByDescending(x => x.TenTrangThai) : q.OrderBy(x => x.TenTrangThai),
                "codetrangthai" => desc ? q.OrderByDescending(x => x.CodeTrangThai) : q.OrderBy(x => x.CodeTrangThai),
                "matrangthai" or _ => desc ? q.OrderByDescending(x => x.MaTrangThai) : q.OrderBy(x => x.MaTrangThai),
            };

            var total = await q.CountAsync();
            var items = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
        public async Task<(List<(DiemDanh d, TrangThaiDiemDanh? t, BuoiHoc b, SinhVien s, LopHocPhan lhp)> Items, int Total)>
            SearchAttendancesAsync(
                int? maDiemDanh,
                DateOnly? thoiGianQuetDateOnly,
                int? maTrangThai,
                bool? trangThai,
                int? maBuoi,
                string? maSinhVien,
                string? maLopHocPhan,
                string? sortBy,
                bool desc,
                int page,
                int pageSize)
        {
            var q = from d in _db.DiemDanh.AsNoTracking()
                    join t0 in _db.TrangThaiDiemDanh.AsNoTracking()
                        on d.MaTrangThai equals t0.MaTrangThai into gj1
                    from t in gj1.DefaultIfEmpty()
                    join b in _db.BuoiHoc.AsNoTracking() on d.MaBuoi equals b.MaBuoi
                    join s in _db.SinhVien.AsNoTracking() on d.MaSinhVien equals s.MaSinhVien
                    join lhp in _db.LopHocPhan.AsNoTracking() on b.MaLopHocPhan equals lhp.MaLopHocPhan
                    select new { d, t, b, s, lhp };

            if (maDiemDanh.HasValue) q = q.Where(x => x.d.MaDiemDanh == maDiemDanh.Value);
            if (thoiGianQuetDateOnly.HasValue)
            {
                var start = thoiGianQuetDateOnly.Value.ToDateTime(TimeOnly.MinValue);
                var end = start.AddDays(1);
                q = q.Where(x => x.d.ThoiGianQuet >= start && x.d.ThoiGianQuet < end);
            }
            if (maTrangThai.HasValue) q = q.Where(x => x.t != null && x.t.MaTrangThai == maTrangThai.Value);
            if (trangThai.HasValue) q = q.Where(x => x.d.TrangThai == trangThai.Value);
            if (maBuoi.HasValue) q = q.Where(x => x.d.MaBuoi == maBuoi.Value);
            if (!string.IsNullOrWhiteSpace(maSinhVien))
                q = q.Where(x => x.d.MaSinhVien == maSinhVien);
            if (!string.IsNullOrWhiteSpace(maLopHocPhan))
                q = q.Where(x => x.lhp.MaLopHocPhan == maLopHocPhan);

            var key = (sortBy ?? "MaDiemDanh").Trim().ToLowerInvariant();
            q = key switch
            {
                "thoigianquet" => desc ? q.OrderByDescending(x => x.d.ThoiGianQuet) : q.OrderBy(x => x.d.ThoiGianQuet),
                "matrangthai" => desc ? q.OrderByDescending(x => x.t!.MaTrangThai) : q.OrderBy(x => x.t!.MaTrangThai),
                "trangthai" => desc ? q.OrderByDescending(x => x.d.TrangThai) : q.OrderBy(x => x.d.TrangThai),
                "mabuoi" => desc ? q.OrderByDescending(x => x.d.MaBuoi) : q.OrderBy(x => x.d.MaBuoi),
                "masinhvien" => desc ? q.OrderByDescending(x => x.d.MaSinhVien) : q.OrderBy(x => x.d.MaSinhVien),
                "malophocphan" => desc ? q.OrderByDescending(x => x.lhp.MaLopHocPhan) : q.OrderBy(x => x.lhp.MaLopHocPhan),
                "madiemdanh" or _ => desc ? q.OrderByDescending(x => x.d.MaDiemDanh) : q.OrderBy(x => x.d.MaDiemDanh),
            };

            var total = await q.CountAsync();
            var items = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ValueTuple<DiemDanh, TrangThaiDiemDanh?, BuoiHoc, SinhVien, LopHocPhan>(
                    x.d, x.t, x.b, x.s, x.lhp))
                .ToListAsync();

            return (items, total);
        }
    }
}
