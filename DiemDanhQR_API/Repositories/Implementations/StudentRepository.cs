// File: Repositories/Implementations/StudentRepository.cs
using DiemDanhQR_API.Data;
using DiemDanhQR_API.Models;
using DiemDanhQR_API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DiemDanhQR_API.Repositories.Implementations
{
    public class StudentRepository : IStudentRepository
    {
        private readonly AppDbContext _db;
        public StudentRepository(AppDbContext db) => _db = db;

        public Task<bool> ExistsStudentAsync(string maSinhVien)
            => _db.SinhVien.AnyAsync(s => s.MaSinhVien == maSinhVien);

        public async Task AddStudentAsync(SinhVien entity)
            => await _db.SinhVien.AddAsync(entity);

        public Task<NguoiDung?> GetUserByMaAsync(string maNguoiDung)
            => _db.NguoiDung.FirstOrDefaultAsync(u => u.MaNguoiDung == maNguoiDung);

        public Task<NguoiDung?> GetUserByUsernameAsync(string tenDangNhap)
            => _db.NguoiDung.FirstOrDefaultAsync(u => u.TenDangNhap == tenDangNhap);

        public async Task AddUserAsync(NguoiDung user)
            => await _db.NguoiDung.AddAsync(user);

        public Task<PhanQuyen?> GetRoleAsync(int maQuyen)
            => _db.PhanQuyen.FirstOrDefaultAsync(r => r.MaQuyen == maQuyen);

        public Task SaveChangesAsync() => _db.SaveChangesAsync();
        public async Task<(List<(SinhVien Sv, NguoiDung Nd)> Items, int Total)> SearchStudentsAsync(
            string? keyword,
            string? khoa,
            string? nganh,
            int? namNhapHoc,
            bool? trangThaiUser,
            string? sortBy,
            bool desc,
            int page,
            int pageSize
        )
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

            // Join hoàn toàn bằng Models
            var q = from sv in _db.SinhVien.AsNoTracking()
                    join nd in _db.NguoiDung.AsNoTracking()
                        on sv.MaNguoiDung equals nd.MaNguoiDung
                    select new { sv, nd };

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                q = q.Where(x =>
                    (x.sv.MaSinhVien ?? "").Contains(kw) ||
                    (x.nd.HoTen ?? "").Contains(kw) ||
                    (x.nd.Email ?? "").Contains(kw) ||
                    (x.nd.SoDienThoai ?? "").Contains(kw)
                );
            }
            if (!string.IsNullOrWhiteSpace(khoa))
                q = q.Where(x => x.sv.Khoa == khoa);

            if (!string.IsNullOrWhiteSpace(nganh))
                q = q.Where(x => x.sv.Nganh == nganh);

            if (namNhapHoc.HasValue)
                q = q.Where(x => x.sv.NamNhapHoc == namNhapHoc.Value);

            if (trangThaiUser.HasValue)
                q = q.Where(x => (x.nd.TrangThai ?? true) == trangThaiUser.Value);

            var key = (sortBy ?? "HoTen").ToLowerInvariant();
            q = key switch
            {
                "masinhvien" => (desc ? q.OrderByDescending(x => x.sv.MaSinhVien) : q.OrderBy(x => x.sv.MaSinhVien)),
                "namnhaphoc" => (desc ? q.OrderByDescending(x => x.sv.NamNhapHoc) : q.OrderBy(x => x.sv.NamNhapHoc)),
                "hoten" or _ => (desc ? q.OrderByDescending(x => x.nd.HoTen) : q.OrderBy(x => x.nd.HoTen)),
            };

            var total = await q.CountAsync();

            var list = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new { x.sv, x.nd })
                .ToListAsync();

            // Trả về tuple của Models
            var items = list.Select(x => (x.sv, x.nd)).ToList();

            return (items, total);
        }
    }
}
