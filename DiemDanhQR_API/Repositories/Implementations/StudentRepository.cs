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

        public Task<NguoiDung?> GetUserByIdAsync(int maNguoiDung)
            => _db.NguoiDung.FirstOrDefaultAsync(u => u.MaNguoiDung == maNguoiDung);

        public Task<NguoiDung?> GetUserByUsernameAsync(string tenDangNhap)
            => _db.NguoiDung.FirstOrDefaultAsync(u => u.TenDangNhap == tenDangNhap);

        public async Task AddUserAsync(NguoiDung user)
            => await _db.NguoiDung.AddAsync(user);

        public Task UpdateUserAsync(NguoiDung user)
        {
            _db.NguoiDung.Update(user);
            return Task.CompletedTask;
        }

        public Task<PhanQuyen?> GetRoleAsync(int maQuyen)
            => _db.PhanQuyen.FirstOrDefaultAsync(r => r.MaQuyen == maQuyen);

        public Task SaveChangesAsync() => _db.SaveChangesAsync();

        public async Task<(List<(SinhVien Sv, NguoiDung Nd)> Items, int Total)> SearchStudentsAsync(
            // string? keyword, // removed
            string? khoa,
            string? nganh,
            int? namNhapHoc,
            bool? trangThaiUser,
            string? maLopHocPhan,
            string? sortBy,
            bool desc,
            int page,
            int pageSize
        )
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

            var q = from sv in _db.SinhVien.AsNoTracking()
                    join nd in _db.NguoiDung.AsNoTracking()
                        on sv.MaNguoiDung equals nd.MaNguoiDung
                    select new { sv, nd };

            // removed keyword OR filter
            // if (!string.IsNullOrWhiteSpace(keyword)) { ... }

            if (!string.IsNullOrWhiteSpace(khoa))
                q = q.Where(x => x.sv.Khoa == khoa);

            if (!string.IsNullOrWhiteSpace(nganh))
                q = q.Where(x => x.sv.Nganh == nganh);

            if (namNhapHoc.HasValue)
                q = q.Where(x => x.sv.NamNhapHoc == namNhapHoc.Value);

            if (trangThaiUser.HasValue)
                q = q.Where(x => (x.nd.TrangThai ?? true) == trangThaiUser.Value);

            if (!string.IsNullOrWhiteSpace(maLopHocPhan))
            {
                var lhp = maLopHocPhan.Trim();
                q = q.Where(x => _db.ThamGiaLop.AsNoTracking()
                    .Any(t => t.MaSinhVien == x.sv.MaSinhVien && t.MaLopHocPhan == lhp));
            }

            var key = (sortBy ?? "HoTen").ToLowerInvariant();
            q = key switch
            {
                "masinhvien" => (desc ? q.OrderByDescending(x => x.sv.MaSinhVien) : q.OrderBy(x => x.sv.MaSinhVien)),
                "lophanhchinh" => (desc ? q.OrderByDescending(x => x.sv.LopHanhChinh) : q.OrderBy(x => x.sv.LopHanhChinh)),
                "namnhaphoc" => (desc ? q.OrderByDescending(x => x.sv.NamNhapHoc) : q.OrderBy(x => x.sv.NamNhapHoc)),
                "khoa" => (desc ? q.OrderByDescending(x => x.sv.Khoa) : q.OrderBy(x => x.sv.Khoa)),
                "nganh" => (desc ? q.OrderByDescending(x => x.sv.Nganh) : q.OrderBy(x => x.sv.Nganh)),
                "hoten" or _ => (desc ? q.OrderByDescending(x => x.nd.HoTen) : q.OrderBy(x => x.nd.HoTen)),
            };

            var total = await q.CountAsync();

            var list = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new { x.sv, x.nd })
                .ToListAsync();

            var items = list.Select(x => (x.sv, x.nd)).ToList();
            return (items, total);
        }

        public Task<SinhVien?> GetStudentByMaNguoiDungAsync(int maNguoiDung)
            => _db.SinhVien.FirstOrDefaultAsync(s => s.MaNguoiDung == maNguoiDung);

        public Task<SinhVien?> GetStudentByMaSinhVienAsync(string maSinhVien)
            => _db.SinhVien.FirstOrDefaultAsync(s => s.MaSinhVien == maSinhVien);

        public Task UpdateStudentAsync(SinhVien entity)
        {
            _db.SinhVien.Update(entity);
            return Task.CompletedTask;
        }

        public async Task AddActivityAsync(LichSuHoatDong log)
            => await _db.LichSuHoatDong.AddAsync(log);

        public async Task<bool> CourseExistsAsync(string maLopHocPhan)
        {
            if (string.IsNullOrWhiteSpace(maLopHocPhan)) return false;
            var code = maLopHocPhan.Trim();
            return await _db.LopHocPhan.AsNoTracking().AnyAsync(l => (l.MaLopHocPhan ?? "") == code);
        }

        public async Task<bool> ParticipationExistsAsync(string maLopHocPhan, string maSinhVien)
        {
            var lhp = maLopHocPhan?.Trim() ?? "";
            var sv = maSinhVien?.Trim() ?? "";
            return await _db.ThamGiaLop.AsNoTracking()
                .AnyAsync(t => (t.MaLopHocPhan ?? "") == lhp && (t.MaSinhVien ?? "") == sv);
        }

        public async Task AddParticipationAsync(ThamGiaLop thamGia)
        {
            _db.ThamGiaLop.Add(thamGia);
            await _db.SaveChangesAsync();
        }

        public Task<ThamGiaLop?> GetParticipationAsync(string maLopHocPhan, string maSinhVien)
        {
            var lhp = (maLopHocPhan ?? "").Trim();
            var sv = (maSinhVien ?? "").Trim();
            return _db.ThamGiaLop.FirstOrDefaultAsync(t => (t.MaLopHocPhan ?? "") == lhp && (t.MaSinhVien ?? "") == sv);
        }

        public async Task UpdateParticipationAsync(ThamGiaLop thamGia)
        {
            _db.ThamGiaLop.Update(thamGia);
            await _db.SaveChangesAsync();
        }
    }
}
