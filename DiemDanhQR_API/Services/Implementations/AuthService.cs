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

        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            var username = HelperFunctions.NormalizeCode(request.TenDangNhap);
            if (string.IsNullOrWhiteSpace(username))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Tên đăng nhập không hợp lệ.");

            if (string.IsNullOrWhiteSpace(request.MatKhau))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mật khẩu không được trống.");

            var user = await _repo.GetByUserNameAsync(username);
            if (user == null || user.TrangThai == false)
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Sai tài khoản hoặc tài khoản bị khoá.");

            // Verify password (BCrypt)
            if (string.IsNullOrWhiteSpace(user!.MatKhau) ||
                !BCrypt.Net.BCrypt.Verify(request.MatKhau, user.MatKhau))
            {
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Sai mật khẩu.");
            }

            // Load role
            var maQuyen = user.MaQuyen ?? 0;
            var role = maQuyen > 0 ? await _repo.GetRoleAsync(maQuyen) : null;
            var roleCode = role?.CodeQuyen ?? "USER";

            // JWT config
            var jwtSection = _cfg.GetSection("Jwt");
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];
            var key = jwtSection["Key"];
            var expireMinutes = int.TryParse(jwtSection["ExpireMinutes"], out var em) ? em : 60;
            var refreshDays = int.TryParse(jwtSection["RefreshDays"], out var rd) ? rd : 14;

            if (string.IsNullOrWhiteSpace(key))
                ApiExceptionHelper.Throw(ApiErrorCode.InternalError, "Thiếu cấu hình JWT Key.");

            // ===== Thời gian =====
            var nowUtc = DateTime.UtcNow;                       // cho JWT, response
            var accessExpiresUtc = nowUtc.AddMinutes(expireMinutes);

            // Claims
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.MaNguoiDung ?? username),
                new(ClaimTypes.NameIdentifier, user.MaNguoiDung ?? username),
                new(ClaimTypes.Name, user.HoTen ?? username),
                new(ClaimTypes.Role, roleCode),
                new(JwtRegisteredClaimNames.UniqueName, user.TenDangNhap ?? username),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Create Access Token
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

            // ===== Refresh Token =====
            var refreshId = Guid.NewGuid();
            var refreshPlain = HelperFunctions.GenerateSecureRefreshToken(); // random string
            var refreshHash = BCrypt.Net.BCrypt.HashPassword(refreshPlain);

            // Lưu DB theo GIỜ VIỆT NAM (DateTime – Windows)
            var refreshIssuedVn = HelperFunctions.UtcToVietnam(nowUtc);
            var refreshExpiresVn = HelperFunctions.UtcToVietnam(nowUtc.AddDays(refreshDays));

            await _repo.UpdateRefreshTokenAsync(user, refreshHash, refreshId, refreshIssuedVn, refreshExpiresVn);
            await _repo.LogActivityAsync(user.MaNguoiDung!, "Đăng nhập");
            await _repo.SaveChangesAsync();

            var data = new LoginResponse(
                accessToken,
                refreshPlain,
                HelperFunctions.UtcToVietnam(accessExpiresUtc),
                user.MaNguoiDung ?? username,
                user.TenDangNhap ?? username,
                user.HoTen ?? username,
                maQuyen,
                roleCode
            );

            return new ApiResponse<LoginResponse>
            {
                Status = 200,
                Message = "Đăng nhập thành công.",
                Data = data
            };
        }

        public async Task<ApiResponse<LogoutResponse>> LogoutAsync(string maNguoiDung)
        {
            var userId = HelperFunctions.NormalizeCode(maNguoiDung);
            if (string.IsNullOrWhiteSpace(userId))
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Không xác định được người dùng.");

            var user = await _repo.GetByIdAsync(userId);
            if (user == null || user.TrangThai == false)
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Tài khoản không hợp lệ hoặc đã bị khoá.");

            var revokedAt = HelperFunctions.UtcToVietnam(DateTime.UtcNow);

            await _repo.RevokeRefreshTokenAsync(user!, revokedAt, clearTokenFields: true);
            await _repo.LogActivityAsync(userId, "Đăng xuất");
            await _repo.SaveChangesAsync();

            var data = new LogoutResponse(userId, revokedAt);
            return new ApiResponse<LogoutResponse>
            {
                Status = 200,
                Message = "Đăng xuất thành công.",
                Data = data
            };
        }
        public async Task<ApiResponse<RefreshAccessTokenResponse>> RefreshAccessTokenAsync(RefreshTokenRequest request)
        {
            // 1) Validate input
            var username = HelperFunctions.NormalizeCode(request?.TenDangNhap);
            if (string.IsNullOrWhiteSpace(username))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu tên đăng nhập.");

            if (string.IsNullOrWhiteSpace(request?.RefreshToken))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu refresh token.");

            // 2) Tìm user
            var user = await _repo.GetByUserNameAsync(username);
            if (user == null || user.TrangThai == false)
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Tài khoản không hợp lệ hoặc đã bị khoá.");

            // 3) Kiểm tra refresh token
            if (string.IsNullOrWhiteSpace(user!.RefreshTokenHash))
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Không có refresh token hợp lệ.");

            if (!BCrypt.Net.BCrypt.Verify(request!.RefreshToken, user.RefreshTokenHash))
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Refresh token sai.");

            // Hạn dùng/lần thu hồi — DB đang lưu GIỜ VIỆT NAM (DateTime)
            var nowVn = HelperFunctions.UtcToVietnam(DateTime.UtcNow);

            if (user.RefreshTokenRevokedAt.HasValue)
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Refresh token đã bị thu hồi.");

            if (!user.RefreshTokenExpiresAt.HasValue || nowVn > user.RefreshTokenExpiresAt.Value)
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Refresh token đã hết hạn.");

            // 4) Load role
            var maQuyen = user.MaQuyen ?? 0;
            var role = maQuyen > 0 ? await _repo.GetRoleAsync(maQuyen) : null;
            var roleCode = role?.CodeQuyen ?? "USER";

            // 5) Cấp AccessToken mới (KHÔNG làm mới RefreshToken)
            var jwtSection = _cfg.GetSection("Jwt");
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];
            var key = jwtSection["Key"];
            var expireMinutes = int.TryParse(jwtSection["ExpireMinutes"], out var em) ? em : 60;

            if (string.IsNullOrWhiteSpace(key))
                ApiExceptionHelper.Throw(ApiErrorCode.InternalError, "Thiếu cấu hình JWT Key.");

            var nowUtc = DateTime.UtcNow;
            var accessExpiresUtc = nowUtc.AddMinutes(expireMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.MaNguoiDung ?? username),
                new(ClaimTypes.NameIdentifier, user.MaNguoiDung ?? username),
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

            var data = new RefreshAccessTokenResponse(
                newAccessToken,
                HelperFunctions.UtcToVietnam(accessExpiresUtc),
                user.MaNguoiDung ?? username,
                user.TenDangNhap ?? username,
                user.HoTen ?? username,
                maQuyen,
                roleCode
            );

            return new ApiResponse<RefreshAccessTokenResponse>
            {
                Status = 200,
                Message = "Làm mới access token thành công.",
                Data = data
            };
        }

        public async Task<ApiResponse<ChangePasswordResponse>> ChangePasswordAsync(string maNguoiDungFromClaims, ChangePasswordRequest request)
        {
            // Validate input
            var userKey = HelperFunctions.NormalizeCode(maNguoiDungFromClaims);
            if (string.IsNullOrWhiteSpace(userKey))
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Không xác định được người dùng.");

            if (string.IsNullOrWhiteSpace(request?.MatKhauCu) || string.IsNullOrWhiteSpace(request?.MatKhauMoi))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu mật khẩu cũ hoặc mật khẩu mới.");

            // Một vài rule đơn giản
            if (request!.MatKhauMoi.Length < 6)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mật khẩu mới phải có ít nhất 6 ký tự.");

            // Tải user
            var user = await _repo.GetByIdAsync(userKey);
            if (user == null || user.TrangThai == false)
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Tài khoản không hợp lệ hoặc đã bị khoá.");

            // Xác thực mật khẩu cũ
            if (string.IsNullOrWhiteSpace(user!.MatKhau) || !BCrypt.Net.BCrypt.Verify(request.MatKhauCu, user.MatKhau))
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Mật khẩu cũ không đúng.");

            // Không cho phép đặt trùng mật khẩu cũ
            if (BCrypt.Net.BCrypt.Verify(request.MatKhauMoi, user.MatKhau))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mật khẩu mới không được trùng mật khẩu cũ.");

            // Cập nhật mật khẩu mới (hash)
            var newHash = BCrypt.Net.BCrypt.HashPassword(request.MatKhauMoi);
            await _repo.UpdatePasswordHashAsync(user, newHash);

            // Thu hồi refresh token hiện có (để buộc đăng nhập lại trên các thiết bị khác)
            var revokedAtVn = HelperFunctions.UtcToVietnam(DateTime.UtcNow);
            await _repo.RevokeRefreshTokenAsync(user, revokedAtVn, clearTokenFields: true);

            await _repo.LogActivityAsync(user.MaNguoiDung ?? userKey, "Đổi mật khẩu");
            await _repo.SaveChangesAsync();

            var data = new ChangePasswordResponse(user.MaNguoiDung ?? userKey, revokedAtVn);
            return new ApiResponse<ChangePasswordResponse>
            {
                Status = 200,
                Message = "Đổi mật khẩu thành công.",
                Data = data
            };
        }
        public async Task<ApiResponse<RefreshPasswordResponse>> RefreshPasswordToUserIdAsync(RefreshPasswordRequest request)
        {
            var username = HelperFunctions.NormalizeCode(request?.TenDangNhap);
            if (string.IsNullOrWhiteSpace(username))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu tên đăng nhập.");

            // Tìm user theo TenDangNhap
            var user = await _repo.GetByUserNameAsync(username);
            if (user == null || user.TrangThai == false)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy tài khoản hoặc tài khoản bị khoá.");

            // Mật khẩu mới = MaNguoiDung (fallback TenDangNhap nếu MaNguoiDung null)
            var newPlain = user.MaNguoiDung ?? user.TenDangNhap ?? username;
            if (string.IsNullOrWhiteSpace(newPlain))
                ApiExceptionHelper.Throw(ApiErrorCode.InternalError, "Không xác định được mật khẩu mặc định.");

            // Băm và cập nhật
            var newHash = BCrypt.Net.BCrypt.HashPassword(newPlain);
            await _repo.UpdatePasswordHashAsync(user, newHash);

            // Thu hồi refresh token (bắt đăng nhập lại)
            var changedAtVn = HelperFunctions.UtcToVietnam(DateTime.UtcNow);
            await _repo.RevokeRefreshTokenAsync(user, changedAtVn, clearTokenFields: true);

            await _repo.LogActivityAsync(user.MaNguoiDung ?? username, "Làm mới mật khẩu");
            await _repo.SaveChangesAsync();

            var data = new RefreshPasswordResponse(
                user.MaNguoiDung ?? username,
                user.TenDangNhap ?? username,
                newPlain,                 // trả plaintext để hiển thị cho user
                changedAtVn
            );

            return new ApiResponse<RefreshPasswordResponse>
            {
                Status = 200,
                Message = "Đã làm mới mật khẩu về MaNguoiDung.",
                Data = data
            };
        }
    }
}
