// File: Repositories/Implementations/LecturerRepository.cs
using DiemDanhQR_API.Data;
using DiemDanhQR_API.Models;
using DiemDanhQR_API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DiemDanhQR_API.Repositories.Implementations
{
    public class LecturerRepository : ILecturerRepository
    {
        private readonly AppDbContext _db;
        public LecturerRepository(AppDbContext db) => _db = db;

        public Task<bool> ExistsLecturerAsync(string maGiangVien)
            => _db.GiangVien.AnyAsync(g => g.MaGiangVien == maGiangVien);

        public async Task AddLecturerAsync(GiangVien entity)
            => await _db.GiangVien.AddAsync(entity);

        public Task<NguoiDung?> GetUserByMaAsync(string maNguoiDung)
            => _db.NguoiDung.FirstOrDefaultAsync(u => u.MaNguoiDung == maNguoiDung);

        public Task<NguoiDung?> GetUserByUsernameAsync(string tenDangNhap)
            => _db.NguoiDung.FirstOrDefaultAsync(u => u.TenDangNhap == tenDangNhap);

        public async Task AddUserAsync(NguoiDung user)
            => await _db.NguoiDung.AddAsync(user);

        public Task<PhanQuyen?> GetRoleAsync(int maQuyen)
            => _db.PhanQuyen.FirstOrDefaultAsync(r => r.MaQuyen == maQuyen);

        public Task SaveChangesAsync() => _db.SaveChangesAsync();
        public async Task<(List<(GiangVien Gv, NguoiDung Nd)> Items, int Total)> SearchLecturersAsync(
            string? keyword,
            string? khoa,
            string? hocHam,
            string? hocVi,
            DateTime? ngayTuyenDungFrom,
            DateTime? ngayTuyenDungTo,
            bool? trangThaiUser,
            string? sortBy,
            bool desc,
            int page,
            int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

            var q = from gv in _db.GiangVien.AsNoTracking()
                    join nd in _db.NguoiDung.AsNoTracking()
                        on gv.MaNguoiDung equals nd.MaNguoiDung
                    select new { gv, nd };

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                q = q.Where(x =>
                    (x.gv.MaGiangVien ?? "").Contains(kw) ||
                    (x.nd.HoTen ?? "").Contains(kw) ||
                    (x.nd.Email ?? "").Contains(kw) ||
                    (x.nd.SoDienThoai ?? "").Contains(kw)
                );
            }

            if (!string.IsNullOrWhiteSpace(khoa))
                q = q.Where(x => x.gv.Khoa == khoa);

            if (!string.IsNullOrWhiteSpace(hocHam))
                q = q.Where(x => x.gv.HocHam == hocHam);

            if (!string.IsNullOrWhiteSpace(hocVi))
                q = q.Where(x => x.gv.HocVi == hocVi);

            if (ngayTuyenDungFrom.HasValue)
                q = q.Where(x => x.gv.NgayTuyenDung >= ngayTuyenDungFrom.Value);

            if (ngayTuyenDungTo.HasValue)
                q = q.Where(x => x.gv.NgayTuyenDung <= ngayTuyenDungTo.Value);

            if (trangThaiUser.HasValue)
                q = q.Where(x => (x.nd.TrangThai ?? true) == trangThaiUser.Value);

            var key = (sortBy ?? "HoTen").ToLowerInvariant();
            q = key switch
            {
                "magiangvien" => (desc ? q.OrderByDescending(x => x.gv.MaGiangVien) : q.OrderBy(x => x.gv.MaGiangVien)),
                "khoa" => (desc ? q.OrderByDescending(x => x.gv.Khoa) : q.OrderBy(x => x.gv.Khoa)),
                "hocham" => (desc ? q.OrderByDescending(x => x.gv.HocHam) : q.OrderBy(x => x.gv.HocHam)),
                "hocvi" => (desc ? q.OrderByDescending(x => x.gv.HocVi) : q.OrderBy(x => x.gv.HocVi)),
                "ngaytuyendung" => (desc ? q.OrderByDescending(x => x.gv.NgayTuyenDung) : q.OrderBy(x => x.gv.NgayTuyenDung)),
                "hoten" or _ => (desc ? q.OrderByDescending(x => x.nd.HoTen) : q.OrderBy(x => x.nd.HoTen)),
            };


            var total = await q.CountAsync();

            var list = await q.Skip((page - 1) * pageSize)
                              .Take(pageSize)
                              .Select(x => new { x.gv, x.nd })
                              .ToListAsync();

            var items = list.Select(x => (x.gv, x.nd)).ToList();
            return (items, total);
        }

        public Task<GiangVien?> GetLecturerByMaNguoiDungAsync(string maNguoiDung)
            => _db.GiangVien.FirstOrDefaultAsync(g => g.MaNguoiDung == maNguoiDung);

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

        public async Task<bool> ExistsUsernameForAnotherAsync(string tenDangNhap)
            => await _db.NguoiDung.AnyAsync(u => u.TenDangNhap == tenDangNhap);

        public async Task AddActivityAsync(LichSuHoatDong log)
        {
            await _db.LichSuHoatDong.AddAsync(log);
        }
    }
}
