// File: Services/Implementations/AuthService.cs
using System.Security.Claims;
using api.DTOs;
using api.ErrorHandling;
using api.Helpers;
using api.Repositories.Interfaces;
using api.Services.Interfaces;

namespace api.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _repo;
        private readonly IPermissionRepository _permRepo; // để lấy danh sách chức năng theo role
        private readonly IConfiguration _cfg;

        public AuthService(IAuthRepository repo, IPermissionRepository permRepo, IConfiguration cfg)
        {
            _repo = repo;
            _permRepo = permRepo;
            _cfg = cfg;
        }

        private string inputResponse(string input) => input ?? "null";

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var username = request.TenDangNhap?.Trim();
            if (string.IsNullOrWhiteSpace(username))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Tên đăng nhập không hợp lệ.");

            if (string.IsNullOrWhiteSpace(request.MatKhau))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mật khẩu không được trống.");

            var user = await _repo.GetByUserNameAsync(username!);
            if (user == null || user.TrangThai == false)
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Sai tài khoản hoặc tài khoản bị khoá.");

            if (string.IsNullOrWhiteSpace(user!.MatKhau) ||
                !BCrypt.Net.BCrypt.Verify(request.MatKhau, user.MatKhau))
            {
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Sai mật khẩu.");
            }

            var maQuyen = user.MaQuyen;
            var role = maQuyen > 0 ? await _repo.GetRoleAsync((int)maQuyen!) : null;
            var roleCode = role?.CodeQuyen ?? "USER";

            // JWT
            var jwtOpts = JwtHelper.Read(_cfg);
            var claims = new List<Claim>
            {
                new("TenDangNhap", user.TenDangNhap ?? username!),
                new("CodeQuyen", roleCode),
                new(ClaimTypes.Role, roleCode),
                new(ClaimTypes.Name, user.HoTen ?? username!),
                new(ClaimTypes.NameIdentifier, user.TenDangNhap ?? username!)
            };
            var accessToken = JwtHelper.GenerateAccessToken(claims, jwtOpts);

            // Refresh token (plain + hash)
            var refreshPlain = JwtHelper.GenerateSecureRefreshToken();
            var refreshHash = BCrypt.Net.BCrypt.HashPassword(refreshPlain);

            // Ghi DB: dùng giờ VN, không format
            var issuedLocal = TimeHelper.UtcToVietnam(DateTime.UtcNow);
            var expiresLocal = TimeHelper.UtcToVietnam(DateTime.UtcNow.AddDays(jwtOpts.RefreshDays));
            await _repo.UpdateRefreshTokenAsync(user, refreshHash, Guid.NewGuid(), issuedLocal, expiresLocal);
            await _repo.LogActivityAsync(user.TenDangNhap ?? username!, "Đăng nhập");
            await _repo.SaveChangesAsync();

            // Build NguoiDung / PhanQuyen DTO
            var nguoiDungDto = new NguoiDungDTO
            {
                MaNguoiDung = inputResponse(user.MaNguoiDung.ToString()),
                HoTen = inputResponse(user.HoTen),
                TenDangNhap = inputResponse(user.TenDangNhap),
                TrangThai = inputResponse(user.TrangThai.ToString().ToLowerInvariant())
            };

            var phanQuyenDto = new PhanQuyenDTO
            {
                MaQuyen = inputResponse(role?.MaQuyen.ToString()),
                CodeQuyen = inputResponse(role?.CodeQuyen),
                TenQuyen = inputResponse(role?.TenQuyen),
            };

            // Login giờ CHỈ trả token + user + quyền
            return new LoginResponse
            {
                AccessToken = inputResponse(accessToken),
                RefreshToken = inputResponse(refreshPlain),
                NguoiDung = nguoiDungDto,
                PhanQuyen = phanQuyenDto
            };
        }

        public async Task<LogoutResponse> LogoutAsync(string tenDangNhap)
        {
            var username = tenDangNhap?.Trim();
            if (string.IsNullOrWhiteSpace(username))
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Không xác định được người dùng.");

            var user = await _repo.GetByUserNameAsync(username!);
            if (user == null || user.TrangThai == false)
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Tài khoản không hợp lệ hoặc đã bị khoá.");

            var revokedAtLocal = TimeHelper.UtcToVietnam(DateTime.UtcNow);
            await _repo.RevokeRefreshTokenAsync(user!, revokedAtLocal, clearTokenFields: true);
            await _repo.LogActivityAsync(username!, "Đăng xuất");
            await _repo.SaveChangesAsync();

            return new LogoutResponse
            {
                NguoiDung = new NguoiDungDTO
                {
                    MaNguoiDung = inputResponse(user!.MaNguoiDung.ToString()),
                    HoTen = inputResponse(user.HoTen),
                    TenDangNhap = inputResponse(user.TenDangNhap)
                }
            };
        }

        public async Task<RefreshAccessTokenResponse> RefreshAccessTokenAsync(RefreshTokenRequest request)
        {
            var username = request?.TenDangNhap?.Trim();
            if (string.IsNullOrWhiteSpace(username))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu tên đăng nhập.");

            if (string.IsNullOrWhiteSpace(request?.RefreshToken))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu refresh token.");

            var user = await _repo.GetByUserNameAsync(username!);
            if (user == null || user.TrangThai == false)
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Tài khoản không hợp lệ hoặc đã bị khoá.");

            if (string.IsNullOrWhiteSpace(user!.RefreshTokenHash) ||
                !BCrypt.Net.BCrypt.Verify(request!.RefreshToken, user.RefreshTokenHash))
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Refresh token sai.");

            var nowLocal = TimeHelper.UtcToVietnam(DateTime.UtcNow);
            if (user.RefreshTokenRevokedAt.HasValue)
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Refresh token đã bị thu hồi.");
            if (!user.RefreshTokenExpiresAt.HasValue || nowLocal > user.RefreshTokenExpiresAt.Value)
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Refresh token đã hết hạn.");

            var maQuyen = user.MaQuyen;
            var role = maQuyen > 0 ? await _repo.GetRoleAsync((int)maQuyen!) : null;
            var roleCode = role?.CodeQuyen ?? "USER";

            var jwtOpts = JwtHelper.Read(_cfg);
            var claims = new List<Claim>
            {
                new("TenDangNhap", user.TenDangNhap ?? username!),
                new("CodeQuyen", roleCode),
                new(ClaimTypes.Role, roleCode),
                new(ClaimTypes.Name, user.HoTen ?? username!),
                new(ClaimTypes.NameIdentifier, user.TenDangNhap ?? username!)
            };
            var newAccessToken = JwtHelper.GenerateAccessToken(claims, jwtOpts);

            return new RefreshAccessTokenResponse
            {
                AccessToken = newAccessToken,
                NguoiDung = new NguoiDungDTO
                {
                    MaNguoiDung = inputResponse(user.MaNguoiDung.ToString()),
                    HoTen = inputResponse(user.HoTen),
                    TenDangNhap = inputResponse(user.TenDangNhap),
                    TrangThai = inputResponse(user.TrangThai.ToString().ToLowerInvariant())
                }
            };
        }

        public async Task<ChangePasswordResponse> ChangePasswordAsync(string tenDangNhapFromClaims, ChangePasswordRequest request)
        {
            var username = tenDangNhapFromClaims?.Trim();
            if (string.IsNullOrWhiteSpace(username))
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Không xác định được người dùng.");

            if (string.IsNullOrWhiteSpace(request?.MatKhauCu) || string.IsNullOrWhiteSpace(request?.MatKhauMoi))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu mật khẩu cũ hoặc mật khẩu mới.");

            if (request!.MatKhauMoi!.Length < 6)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mật khẩu mới phải có ít nhất 6 ký tự.");

            var user = await _repo.GetByUserNameAsync(username!);
            if (user == null || user.TrangThai == false)
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Tài khoản không hợp lệ hoặc đã bị khoá.");

            if (string.IsNullOrWhiteSpace(user!.MatKhau) ||
                !BCrypt.Net.BCrypt.Verify(request.MatKhauCu, user.MatKhau))
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Mật khẩu cũ không đúng.");

            if (BCrypt.Net.BCrypt.Verify(request.MatKhauMoi, user.MatKhau))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mật khẩu mới không được trùng mật khẩu cũ.");

            var newHash = BCrypt.Net.BCrypt.HashPassword(request.MatKhauMoi);
            await _repo.UpdatePasswordHashAsync(user, newHash);

            var changedAtLocal = TimeHelper.UtcToVietnam(DateTime.UtcNow);
            await _repo.RevokeRefreshTokenAsync(user, changedAtLocal, clearTokenFields: true);

            await _repo.LogActivityAsync(username!, "Đổi mật khẩu");
            await _repo.SaveChangesAsync();

            return new ChangePasswordResponse
            {
                NguoiDung = new NguoiDungDTO
                {
                    MaNguoiDung = inputResponse(user.MaNguoiDung.ToString()),
                    HoTen = inputResponse(user.HoTen),
                    TenDangNhap = inputResponse(user.TenDangNhap)
                },
                ChangedAt = inputResponse(changedAtLocal.ToString("dd-MM-yyyy HH:mm:ss"))
            };
        }

        public async Task<RefreshPasswordResponse> RefreshPasswordToUserIdAsync(RefreshPasswordRequest request)
        {
            var username = request?.TenDangNhap?.Trim();
            if (string.IsNullOrWhiteSpace(username))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Thiếu tên đăng nhập.");

            var user = await _repo.GetByUserNameAsync(username!);
            if (user == null || user.TrangThai == false)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy tài khoản hoặc tài khoản bị khoá.");

            var newPlain = user!.TenDangNhap ?? username;
            var newHash = BCrypt.Net.BCrypt.HashPassword(newPlain);
            await _repo.UpdatePasswordHashAsync(user, newHash);

            var refreshAtLocal = TimeHelper.UtcToVietnam(DateTime.UtcNow);
            await _repo.RevokeRefreshTokenAsync(user, refreshAtLocal, clearTokenFields: true);

            await _repo.LogActivityAsync(username!, "Làm mới mật khẩu");
            await _repo.SaveChangesAsync();

            return new RefreshPasswordResponse
            {
                NguoiDung = new NguoiDungDTO
                {
                    MaNguoiDung = inputResponse(user.MaNguoiDung.ToString()),
                    HoTen = inputResponse(user.HoTen),
                    TenDangNhap = inputResponse(user.TenDangNhap)
                },
                RefreshAt = inputResponse(refreshAtLocal.ToString("dd-MM-yyyy HH:mm:ss"))
            };
        }

        // NEW: lấy nhóm chức năng theo user hiện tại
        public async Task<UserRoleFunctionsResponse> GetCurrentUserRoleFunctionsAsync(string tenDangNhapFromClaims)
        {
            var username = tenDangNhapFromClaims?.Trim();
            if (string.IsNullOrWhiteSpace(username))
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Không xác định được người dùng.");

            var user = await _repo.GetByUserNameAsync(username!);
            if (user == null || user.TrangThai == false)
                ApiExceptionHelper.Throw(ApiErrorCode.Unauthorized, "Tài khoản không hợp lệ hoặc đã bị khoá.");

            var maQuyen = user.MaQuyen;
            var role = maQuyen > 0 ? await _repo.GetRoleAsync((int)maQuyen!) : null;
            if (role == null)
                ApiExceptionHelper.Throw(ApiErrorCode.Forbidden, "Tài khoản chưa được gán phân quyền.");

            var phanQuyenDto = new PhanQuyenDTO
            {
                MaQuyen = inputResponse(role.MaQuyen.ToString()),
                CodeQuyen = inputResponse(role.CodeQuyen),
                TenQuyen = inputResponse(role.TenQuyen),
                MoTa = inputResponse(role.MoTa)
            };

            var roleFunctions = new List<RoleFunctionDetailResponse>();

            var (funcItems, _) = await _permRepo.SearchFunctionsAsync(
                maChucNang: null,
                codeChucNang: null,
                tenChucNang: null,
                moTa: null,
                maQuyen: maQuyen,
                parentChucNangId: null,
                sortBy: "MaChucNang",
                desc: false,
                page: 1,
                pageSize: 10_000
            );

            foreach (var fn in funcItems)
            {
                var map = await _permRepo.GetRoleFunctionAsync((int)maQuyen!, fn.MaChucNang);

                roleFunctions.Add(new RoleFunctionDetailResponse
                {
                    ChucNang = new ChucNangDTO
                    {
                        MaChucNang = inputResponse(fn.MaChucNang.ToString()),
                        CodeChucNang = inputResponse(fn.CodeChucNang),
                        TenChucNang = inputResponse(fn.TenChucNang),
                        MoTa = inputResponse(fn.MoTa),
                        ParentChucNangId = inputResponse(fn.ParentChucNangId?.ToString() ?? "null"),
                        Url = inputResponse(fn.Url),
                        Stt = inputResponse(fn.Stt?.ToString() ?? "null"),
                    },
                    NhomChucNang = map == null ? null : new NhomChucNangDTO
                    {
                        TrangThai = inputResponse(map.TrangThai.ToString().ToLowerInvariant())
                    }
                });
            }

            return new UserRoleFunctionsResponse
            {
                PhanQuyen = phanQuyenDto,
                NhomChucNang = roleFunctions
            };
        }
    }
}
