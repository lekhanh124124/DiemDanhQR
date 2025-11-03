// File: Repositories/Interfaces/IAuthRepository.cs
using DiemDanhQR_API.Models;

namespace DiemDanhQR_API.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<NguoiDung?> GetByUserNameAsync(string tenDangNhap);
        Task<PhanQuyen?> GetRoleAsync(int maQuyen);

        Task UpdateRefreshTokenAsync(
            NguoiDung user,
            string refreshTokenHash,
            Guid refreshTokenId,
            DateTime issuedAtUtc,
            DateTime expiresAtUtc);

        Task RevokeRefreshTokenAsync(NguoiDung user, DateTime revokedAtUtc, bool clearTokenFields = true);
        Task UpdatePasswordHashAsync(NguoiDung user, string newPasswordHash);

        // Nhận TenDangNhap, repo tự ánh xạ sang MaNguoiDung (int) để ghi lịch sử
        Task LogActivityAsync(string tenDangNhap, string hanhDong);

        Task SaveChangesAsync();
    }
}
