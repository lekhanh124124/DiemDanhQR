// File: Repositories/Implementations/AuthRepository.cs
using api.Data;
using api.Helpers;
using api.Models;
using api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories.Implementations
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _db;
        public AuthRepository(AppDbContext db) => _db = db;

        public Task<NguoiDung?> GetByUserNameAsync(string tenDangNhap)
            => _db.NguoiDung.FirstOrDefaultAsync(x => x.TenDangNhap == tenDangNhap);

        public Task<PhanQuyen?> GetRoleAsync(int maQuyen)
            => _db.PhanQuyen.FirstOrDefaultAsync(r => r.MaQuyen == maQuyen);

        public Task UpdateRefreshTokenAsync(
            NguoiDung user,
            string refreshTokenHash,
            Guid refreshTokenId,
            DateTime issuedAtLocal,
            DateTime expiresAtLocal)
        {
            // issuedAtLocal/expiresAtLocal đã là giờ VN (UtcToVietnam ở Service)
            user.RefreshTokenHash = refreshTokenHash;
            user.RefreshTokenId = refreshTokenId;
            user.RefreshTokenIssuedAt = issuedAtLocal;
            user.RefreshTokenExpiresAt = expiresAtLocal;
            user.RefreshTokenRevokedAt = null;
            return Task.CompletedTask;
        }

        public Task RevokeRefreshTokenAsync(NguoiDung user, DateTime revokedAtLocal, bool clearTokenFields = true)
        {
            user.RefreshTokenRevokedAt = revokedAtLocal;

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

        public async Task LogActivityAsync(string tenDangNhap, string hanhDong)
        {
            var user = await _db.NguoiDung.AsNoTracking()
                            .FirstOrDefaultAsync(x => x.TenDangNhap == tenDangNhap);

            if (user == null) return;

            _db.LichSuHoatDong.Add(new LichSuHoatDong
            {
                MaNguoiDung = user.MaNguoiDung,
                HanhDong = hanhDong,
                // Ghi DB: dùng UtcToVietnam, không format
                ThoiGian = TimeHelper.UtcToVietnam(DateTime.UtcNow)
            });
        }

        public Task SaveChangesAsync() => _db.SaveChangesAsync();
    }
}
