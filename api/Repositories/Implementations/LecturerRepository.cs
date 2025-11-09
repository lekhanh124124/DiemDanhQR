// File: Repositories/Implementations/LecturerRepository.cs
using api.Data;
using api.Models;
using api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories.Implementations
{
    public class LecturerRepository : ILecturerRepository
    {
        private readonly AppDbContext _db;
        public LecturerRepository(AppDbContext db) => _db = db;

        public Task<bool> ExistsLecturerAsync(string maGiangVien)
            => _db.GiangVien.AnyAsync(g => g.MaGiangVien == maGiangVien);

        public async Task AddLecturerAsync(GiangVien entity)
            => await _db.GiangVien.AddAsync(entity);

        public Task<GiangVien?> GetLecturerByMaNguoiDungAsync(int maNguoiDung)
            => _db.GiangVien.FirstOrDefaultAsync(g => g.MaNguoiDung == maNguoiDung);

        public Task<GiangVien?> GetLecturerByMaGiangVienAsync(string maGiangVien)
            => _db.GiangVien.FirstOrDefaultAsync(g => g.MaGiangVien == maGiangVien);

        public Task<NguoiDung?> GetUserByIdAsync(int maNguoiDung)
            => _db.NguoiDung.FirstOrDefaultAsync(u => u.MaNguoiDung == maNguoiDung);

        public Task<NguoiDung?> GetUserByUsernameAsync(string tenDangNhap)
            => _db.NguoiDung.FirstOrDefaultAsync(u => u.TenDangNhap == tenDangNhap);

        public async Task AddUserAsync(NguoiDung user)
            => await _db.NguoiDung.AddAsync(user);

        public Task UpdateLecturerAsync(GiangVien entity)
        {
            _db.GiangVien.Update(entity);
            return Task.CompletedTask;
        }

        public Task UpdateUserAsync(NguoiDung user)
        {
            _db.NguoiDung.Update(user);
            return Task.CompletedTask;
        }

        public async Task AddActivityAsync(LichSuHoatDong log)
        {
            await _db.LichSuHoatDong.AddAsync(log);
        }

        public Task SaveChangesAsync() => _db.SaveChangesAsync();

        public async Task<(List<(GiangVien Gv, NguoiDung Nd)> Items, int Total)> SearchLecturersAsync(
            string? maGiangVien,
            string? hoTen,
            int? maKhoa,
            string? hocHam,
            string? hocVi,
            DateOnly? ngayTuyenDungFrom,
            DateOnly? ngayTuyenDungTo,
            bool? trangThaiUser,
            string? sortBy,
            bool desc,
            int page,
            int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

            var q = from gv in _db.GiangVien.AsNoTracking()
                    join nd in _db.NguoiDung.AsNoTracking() on gv.MaNguoiDung equals nd.MaNguoiDung
                    select new { gv, nd };

            if (!string.IsNullOrWhiteSpace(maGiangVien))
            {
                var code = maGiangVien.Trim();
                q = q.Where(x => EF.Functions.Like(x.gv.MaGiangVien, $"%{code}%"));
            }

            if (!string.IsNullOrWhiteSpace(hoTen))
            {
                var name = hoTen.Trim();
                q = q.Where(x => x.nd.HoTen != null && EF.Functions.Like(x.nd.HoTen, $"%{name}%"));
            }

            if (maKhoa.HasValue)
                q = q.Where(x => x.gv.MaKhoa == maKhoa.Value);

            if (!string.IsNullOrWhiteSpace(hocHam))
                q = q.Where(x => x.gv.HocHam == hocHam);

            if (!string.IsNullOrWhiteSpace(hocVi))
                q = q.Where(x => x.gv.HocVi == hocVi);

            if (ngayTuyenDungFrom.HasValue)
                q = q.Where(x => x.gv.NgayTuyenDung >= ngayTuyenDungFrom.Value);

            if (ngayTuyenDungTo.HasValue)
                q = q.Where(x => x.gv.NgayTuyenDung <= ngayTuyenDungTo.Value);

            if (trangThaiUser.HasValue)
                q = q.Where(x => x.nd.TrangThai == trangThaiUser.Value);

            var key = (sortBy ?? "HoTen").Trim().ToLowerInvariant();
            q = key switch
            {
                "magiangvien" => (desc ? q.OrderByDescending(x => x.gv.MaGiangVien) : q.OrderBy(x => x.gv.MaGiangVien)),
                "makhoa"      => (desc ? q.OrderByDescending(x => x.gv.MaKhoa)      : q.OrderBy(x => x.gv.MaKhoa)),
                "hocham"      => (desc ? q.OrderByDescending(x => x.gv.HocHam)      : q.OrderBy(x => x.gv.HocHam)),
                "hocvi"       => (desc ? q.OrderByDescending(x => x.gv.HocVi)       : q.OrderBy(x => x.gv.HocVi)),
                "ngaytuyendung" => (desc ? q.OrderByDescending(x => x.gv.NgayTuyenDung) : q.OrderBy(x => x.gv.NgayTuyenDung)),
                "hoten" or _  => (desc ? q.OrderByDescending(x => x.nd.HoTen)       : q.OrderBy(x => x.nd.HoTen)),
            };

            var total = await q.CountAsync();
            var list = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var items = list.Select(x => (x.gv, x.nd)).ToList();
            return (items, total);
        }
    }
}
