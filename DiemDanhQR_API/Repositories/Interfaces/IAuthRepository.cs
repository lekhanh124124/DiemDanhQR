// File: Repositories/Interfaces/IAuthRepository.cs
using DiemDanhQR_API.Models;

namespace DiemDanhQR_API.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<NguoiDung?> GetByUserNameAsync(string tenDangNhap);
        Task<NguoiDung?> GetByIdAsync(string maNguoiDung);
        Task<PhanQuyen?> GetRoleAsync(int maQuyen);

        Task UpdateRefreshTokenAsync(
            NguoiDung user,
            string refreshTokenHash,
            Guid refreshTokenId,
            DateTime issuedAtUtc,
            DateTime expiresAtUtc);

        Task RevokeRefreshTokenAsync(NguoiDung user, DateTime revokedAtUtc, bool clearTokenFields = true);
        Task UpdatePasswordHashAsync(NguoiDung user, string newPasswordHash);

        Task LogActivityAsync(string maNguoiDung, string hanhDong);

        Task SaveChangesAsync();
    }
}
