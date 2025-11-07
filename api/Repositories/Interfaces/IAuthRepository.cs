using api.Models;

namespace api.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<NguoiDung?> GetByUserNameAsync(string tenDangNhap);
        Task<PhanQuyen?> GetRoleAsync(int maQuyen);

        Task UpdateRefreshTokenAsync(
            NguoiDung user,
            string refreshTokenHash,
            Guid refreshTokenId,
            DateTime issuedAtLocal,
            DateTime expiresAtLocal);

        Task RevokeRefreshTokenAsync(NguoiDung user, DateTime revokedAtLocal, bool clearTokenFields = true);
        Task UpdatePasswordHashAsync(NguoiDung user, string newPasswordHash);

        // Nhận TenDangNhap, repo tự ánh xạ sang MaNguoiDung để ghi lịch sử
        Task LogActivityAsync(string tenDangNhap, string hanhDong);

        Task SaveChangesAsync();
    }
}
