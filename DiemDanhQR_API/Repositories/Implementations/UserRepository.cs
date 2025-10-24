// File: Repositories/Implementations/UserRepository.cs
using DiemDanhQR_API.Data;
using DiemDanhQR_API.Models;
using DiemDanhQR_API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DiemDanhQR_API.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db) => _db = db;

        public Task<bool> ExistsByMaNguoiDungAsync(string maNguoiDung)
            => _db.NguoiDung.AnyAsync(x => x.MaNguoiDung == maNguoiDung);

        public Task<bool> ExistsByTenDangNhapAsync(string tenDangNhap)
            => _db.NguoiDung.AnyAsync(x => x.TenDangNhap == tenDangNhap);

        public Task<PhanQuyen?> GetRoleAsync(int maQuyen)
            => _db.PhanQuyen.FirstOrDefaultAsync(r => r.MaQuyen == maQuyen);

        public async Task AddAsync(NguoiDung entity)
        {
            await _db.NguoiDung.AddAsync(entity);
        }

        public Task SaveChangesAsync() => _db.SaveChangesAsync();

        public Task<NguoiDung?> GetByMaNguoiDungAsync(string maNguoiDung)
            => _db.NguoiDung.FirstOrDefaultAsync(u => u.MaNguoiDung == maNguoiDung);

        public Task<NguoiDung?> GetByTenDangNhapAsync(string tenDangNhap)
            => _db.NguoiDung.FirstOrDefaultAsync(u => u.TenDangNhap == tenDangNhap);

        public Task<SinhVien?> GetStudentByMaNguoiDungAsync(string maNguoiDung)
            => _db.SinhVien.FirstOrDefaultAsync(s => s.MaNguoiDung == maNguoiDung);

        public Task<GiangVien?> GetLecturerByMaNguoiDungAsync(string maNguoiDung)
            => _db.GiangVien.FirstOrDefaultAsync(g => g.MaNguoiDung == maNguoiDung);
    }
}
