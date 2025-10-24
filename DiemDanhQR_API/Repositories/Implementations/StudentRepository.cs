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
    }
}
