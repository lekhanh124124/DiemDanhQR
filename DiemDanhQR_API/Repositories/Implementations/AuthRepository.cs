// File: Repositories/Implementations/AuthRepository.cs
using DiemDanhQR_API.Data;
using DiemDanhQR_API.Models;
using DiemDanhQR_API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DiemDanhQR_API.Repositories.Implementations
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _db;
        public AuthRepository(AppDbContext db) => _db = db;

        public Task<NguoiDung?> GetByUserNameAsync(string tenDangNhap)
            => _db.NguoiDung.FirstOrDefaultAsync(x => x.TenDangNhap == tenDangNhap);

        public Task<NguoiDung?> GetByIdAsync(string maNguoiDung)
            => _db.NguoiDung.FirstOrDefaultAsync(x => x.MaNguoiDung == maNguoiDung);

        public Task<PhanQuyen?> GetRoleAsync(int maQuyen)
            => _db.PhanQuyen.FirstOrDefaultAsync(r => r.MaQuyen == maQuyen);

        public Task UpdateRefreshTokenAsync(
            NguoiDung user,
            string refreshTokenHash,
            Guid refreshTokenId,
            DateTime issuedAtUtc,
            DateTime expiresAtUtc)
        {
            user.RefreshTokenHash = refreshTokenHash;
            user.RefreshTokenId = refreshTokenId;
            user.RefreshTokenIssuedAt = issuedAtUtc;
            user.RefreshTokenExpiresAt = expiresAtUtc;
            user.RefreshTokenRevokedAt = null;
            return Task.CompletedTask;
        }

        public Task RevokeRefreshTokenAsync(NguoiDung user, DateTime revokedAtUtc, bool clearTokenFields = true)
        {
            user.RefreshTokenRevokedAt = revokedAtUtc;

            if (clearTokenFields)
            {
                user.RefreshTokenHash = null;
                user.RefreshTokenId = null;
                user.RefreshTokenIssuedAt = null;
                user.RefreshTokenExpiresAt = null;
            }
            return Task.CompletedTask;
        }

        public Task UpdatePasswordHashAsync(NguoiDung user, string newPasswordHash)
        {
            user.MatKhau = newPasswordHash;
            return Task.CompletedTask;
        }
        
        public async Task LogActivityAsync(string maNguoiDung, string hanhDong)
        {
            _db.LichSuHoatDong.Add(new LichSuHoatDong
            {
                MaNguoiDung = maNguoiDung,
                HanhDong = hanhDong,
                ThoiGian = DateTime.Now
            });
            await Task.CompletedTask;
        }

        public Task SaveChangesAsync() => _db.SaveChangesAsync();
    }
}
