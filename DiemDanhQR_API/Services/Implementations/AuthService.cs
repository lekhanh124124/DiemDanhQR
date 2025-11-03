// File: Services/Implementations/AuthService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;
using DiemDanhQR_API.Repositories.Interfaces;
using DiemDanhQR_API.Helpers;
using DiemDanhQR_API.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace DiemDanhQR_API.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _cfg;

        public AuthService(IAuthRepository repo, IConfiguration cfg)
        {
            _repo = repo;
            _cfg = cfg;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var username = HelperFunctions.NormalizeCode(request.TenDangNhap);
            if (string.IsNullOrWhiteSpace(username))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Tên đăng nhập không hợp lệ.");

            if (string.IsNullOrWhiteSpace(request.MatKhau))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mật khẩu không được trống.");

            var user = await _repo.GetByUserNameAsync(username);
            if (user == null || user.TrangThai == false)
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Sai tài khoản hoặc tài khoản bị khoá.");

            if (string.IsNullOrWhiteSpace(user!.MatKhau) ||
                !BCrypt.Net.BCrypt.Verify(request.MatKhau, user.MatKhau))
            {
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Sai mật khẩu.");
            }

            var maQuyen = user.MaQuyen ?? 0;
            var role = maQuyen > 0 ? await _repo.GetRoleAsync(maQuyen) : null;
            var roleCode = role?.CodeQuyen ?? "USER";

            var jwtSection = _cfg.GetSection("Jwt");
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];
            var key = jwtSection["Key"];
            var expireMinutes = int.TryParse(jwtSection["ExpireMinutes"], out var em) ? em : 60;
            var refreshDays = int.TryParse(jwtSection["RefreshDays"], out var rd) ? rd : 14;

            if (string.IsNullOrWhiteSpace(key))
                ApiExceptionHelper.Throw(ApiErrorCode.InternalError, "Thiếu cấu hình JWT Key.");

            var nowUtc = DateTime.UtcNow;
            var accessExpiresUtc = nowUtc.AddMinutes(expireMinutes);

            // Claims dùng TenDangNhap thay cho mã người dùng
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.TenDangNhap ?? username),
                new(ClaimTypes.NameIdentifier, user.TenDangNhap ?? username),
                new(ClaimTypes.Name, user.HoTen ?? username),
                new(ClaimTypes.Role, roleCode),
                new(JwtRegisteredClaimNames.UniqueName, user.TenDangNhap ?? username),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: string.IsNullOrWhiteSpace(issuer) ? null : issuer,
                audience: string.IsNullOrWhiteSpace(audience) ? null : audience,
                claims: claims,
                notBefore: nowUtc,
                expires: accessExpiresUtc,
                signingCredentials: creds
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

            // Refresh token
            var refreshId = Guid.NewGuid();
            var refreshPlain = HelperFunctions.GenerateSecureRefreshToken();
            var refreshHash = BCrypt.Net.BCrypt.HashPassword(refreshPlain);

            var refreshIssuedVn = HelperFunctions.UtcToVietnam(nowUtc);
            var refreshExpiresVn = HelperFunctions.UtcToVietnam(nowUtc.AddDays(refreshDays));

            await _repo.UpdateRefreshTokenAsync(user, refreshHash, refreshId, refreshIssuedVn, refreshExpiresVn);
            await _repo.LogActivityAsync(user.TenDangNhap ?? username, "Đăng nhập");
            await _repo.SaveChangesAsync();

            // MaNguoiDung trong response được thay bằng TenDangNhap theo yêu cầu
            var data = new LoginResponse(
                accessToken,
                refreshPlain,
                HelperFunctions.UtcToVietnam(accessExpiresUtc),
                user.TenDangNhap ?? username,
                user.TenDangNhap ?? username,
                user.HoTen ?? username,
                maQuyen,
                roleCode
            );

            return data;
        }

        public async Task<LogoutResponse> LogoutAsync(string tenDangNhap)
        {
            var username = HelperFunctions.NormalizeCode(tenDangNhap);
            if (string.IsNullOrWhiteSpace(username))
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Không xác định được người dùng.");

            var user = await _repo.GetByUserNameAsync(username);
            if (user == null || user.TrangThai == false)
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Tài khoản không hợp lệ hoặc đã bị khoá.");

            var revokedAt = HelperFunctions.UtcToVietnam(DateTime.UtcNow);

            await _repo.RevokeRefreshTokenAsync(user!, revokedAt, clearTokenFields: true);
            await _repo.LogActivityAsync(username, "Đăng xuất");
            await _repo.SaveChangesAsync();

            // Trả về MaNguoiDung = TenDangNhap
            var data = new LogoutResponse(username, revokedAt);
            return data;
        }

        public async Task<RefreshAccessTokenResponse> RefreshAccessTokenAsync(RefreshTokenRequest request)
        {
            var username = HelperFunctions.NormalizeCode(request?.TenDangNhap);
            if (string.IsNullOrWhiteSpace(username))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu tên đăng nhập.");

            if (string.IsNullOrWhiteSpace(request?.RefreshToken))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu refresh token.");

            var user = await _repo.GetByUserNameAsync(username);
            if (user == null || user.TrangThai == false)
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Tài khoản không hợp lệ hoặc đã bị khoá.");

            if (string.IsNullOrWhiteSpace(user!.RefreshTokenHash) ||
                !BCrypt.Net.BCrypt.Verify(request.RefreshToken, user.RefreshTokenHash))
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Refresh token sai.");

            var nowVn = HelperFunctions.UtcToVietnam(DateTime.UtcNow);
            if (user.RefreshTokenRevokedAt.HasValue)
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Refresh token đã bị thu hồi.");
            if (!user.RefreshTokenExpiresAt.HasValue || nowVn > user.RefreshTokenExpiresAt.Value)
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Refresh token đã hết hạn.");

            var maQuyen = user.MaQuyen ?? 0;
            var role = maQuyen > 0 ? await _repo.GetRoleAsync(maQuyen) : null;
            var roleCode = role?.CodeQuyen ?? "USER";

            var jwtSection = _cfg.GetSection("Jwt");
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];
            var key = jwtSection["Key"];
            var expireMinutes = int.TryParse(jwtSection["ExpireMinutes"], out var em) ? em : 60;

            if (string.IsNullOrWhiteSpace(key))
                ApiExceptionHelper.Throw(ApiErrorCode.InternalError, "Thiếu cấu hình JWT Key.");

            var nowUtc = DateTime.UtcNow;
            var accessExpiresUtc = nowUtc.AddMinutes(expireMinutes);

            // Claims dùng TenDangNhap
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.TenDangNhap ?? username),
                new(ClaimTypes.NameIdentifier, user.TenDangNhap ?? username),
                new(ClaimTypes.Name, user.HoTen ?? username),
                new(ClaimTypes.Role, roleCode),
                new(JwtRegisteredClaimNames.UniqueName, user.TenDangNhap ?? username),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: string.IsNullOrWhiteSpace(issuer) ? null : issuer,
                audience: string.IsNullOrWhiteSpace(audience) ? null : audience,
                claims: claims,
                notBefore: nowUtc,
                expires: accessExpiresUtc,
                signingCredentials: creds
            );

            var newAccessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

            // Trả về MaNguoiDung = TenDangNhap
            var data = new RefreshAccessTokenResponse(
                newAccessToken,
                HelperFunctions.UtcToVietnam(accessExpiresUtc),
                user.TenDangNhap ?? username,
                user.TenDangNhap ?? username,
                user.HoTen ?? username,
                maQuyen,
                roleCode
            );

            return data;
        }

        public async Task<ChangePasswordResponse> ChangePasswordAsync(string tenDangNhapFromClaims, ChangePasswordRequest request)
        {
            var username = HelperFunctions.NormalizeCode(tenDangNhapFromClaims);
            if (string.IsNullOrWhiteSpace(username))
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Không xác định được người dùng.");

            if (string.IsNullOrWhiteSpace(request?.MatKhauCu) || string.IsNullOrWhiteSpace(request?.MatKhauMoi))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu mật khẩu cũ hoặc mật khẩu mới.");

            if (request!.MatKhauMoi.Length < 6)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mật khẩu mới phải có ít nhất 6 ký tự.");

            var user = await _repo.GetByUserNameAsync(username);
            if (user == null || user.TrangThai == false)
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Tài khoản không hợp lệ hoặc đã bị khoá.");

            if (string.IsNullOrWhiteSpace(user!.MatKhau) || !BCrypt.Net.BCrypt.Verify(request.MatKhauCu, user.MatKhau))
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Mật khẩu cũ không đúng.");

            if (BCrypt.Net.BCrypt.Verify(request.MatKhauMoi, user.MatKhau))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mật khẩu mới không được trùng mật khẩu cũ.");

            var newHash = BCrypt.Net.BCrypt.HashPassword(request.MatKhauMoi);
            await _repo.UpdatePasswordHashAsync(user, newHash);

            var revokedAtVn = HelperFunctions.UtcToVietnam(DateTime.UtcNow);
            await _repo.RevokeRefreshTokenAsync(user, revokedAtVn, clearTokenFields: true);

            await _repo.LogActivityAsync(username, "Đổi mật khẩu");
            await _repo.SaveChangesAsync();

            // Trả về MaNguoiDung = TenDangNhap
            var data = new ChangePasswordResponse(username, revokedAtVn);
            return data;
        }

        public async Task<RefreshPasswordResponse> RefreshPasswordToUserIdAsync(RefreshPasswordRequest request)
        {
            var username = HelperFunctions.NormalizeCode(request?.TenDangNhap);
            if (string.IsNullOrWhiteSpace(username))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu tên đăng nhập.");

            var user = await _repo.GetByUserNameAsync(username);
            if (user == null || user.TrangThai == false)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy tài khoản hoặc tài khoản bị khoá.");

            // Mật khẩu mới = TenDangNhap (thay cho MaNguoiDung)
            var newPlain = user.TenDangNhap ?? username;
            var newHash = BCrypt.Net.BCrypt.HashPassword(newPlain);
            await _repo.UpdatePasswordHashAsync(user, newHash);

            var changedAtVn = HelperFunctions.UtcToVietnam(DateTime.UtcNow);
            await _repo.RevokeRefreshTokenAsync(user, changedAtVn, clearTokenFields: true);

            await _repo.LogActivityAsync(username, "Làm mới mật khẩu");
            await _repo.SaveChangesAsync();

            var data = new RefreshPasswordResponse(
                user.TenDangNhap ?? username, // MaNguoiDung -> TenDangNhap
                user.TenDangNhap ?? username,
                newPlain,
                changedAtVn
            );

            return data;
        }
    }
}
