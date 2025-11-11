// File: Helpers/JwtHelper.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace api.Helpers
{
    public static class JwtHelper
    {
        // Đọc cấu hình từ IConfiguration, trả về tuple (không cần class Options)
        public static (string? Issuer, string? Audience, string Key, int ExpireMinutes, int RefreshDays) Read(IConfiguration cfg)
        {
            var s = cfg.GetSection("Jwt");
            var key = s["Key"];
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("Missing Jwt:Key configuration.");

            return (
                Issuer: s["Issuer"],
                Audience: s["Audience"],
                Key: key!,
                ExpireMinutes: int.TryParse(s["ExpireMinutes"], out var em) ? em : 60,
                RefreshDays: int.TryParse(s["RefreshDays"], out var rd) ? rd : 14
            );
        }

        // Tạo AccessToken từ claims + tuple cấu hình ở trên
        public static string GenerateAccessToken(IEnumerable<Claim> claims,
            (string? Issuer, string? Audience, string Key, int ExpireMinutes, int RefreshDays) opts)
        {
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opts.Key));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var nowUtc = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                issuer: string.IsNullOrWhiteSpace(opts.Issuer) ? null : opts.Issuer,
                audience: string.IsNullOrWhiteSpace(opts.Audience) ? null : opts.Audience,
                claims: claims,
                notBefore: nowUtc,
                expires: nowUtc.AddMinutes(opts.ExpireMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        // Tạo refresh token ngẫu nhiên (base64url)
        public static string GenerateSecureRefreshToken(int byteLength = 32)
        {
            var bytes = RandomNumberGenerator.GetBytes(byteLength);
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        /// <summary>
        /// Lấy username ưu tiên theo thứ tự: TenDangNhap -> NameIdentifier -> Name -> Identity.Name.
        /// </summary>
        public static string? GetUsername(ClaimsPrincipal user)
        {
            if (user == null) return null;
            return user.FindFirst("TenDangNhap")?.Value
                ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst(ClaimTypes.Name)?.Value
                ?? user.Identity?.Name;
        }

        /// <summary>
        /// Kiểm tra 1 chuỗi role có được coi là ADMIN hay không.
        /// Tiêu chí:
        /// - Chứa "ADMIN" (không phân biệt hoa/thường), ví dụ: "ADMIN_QTND"
        /// - Hoặc bắt đầu bằng "AD" (phổ biến "AD_" rút gọn của ADMIN), ví dụ: "AD_QTGV"
        /// </summary>
        public static bool IsAdminRoleString(string? role)
        {
            if (string.IsNullOrWhiteSpace(role)) return false;
            var r = role.Trim();
            if (r.IndexOf("ADMIN", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (r.StartsWith("AD", StringComparison.OrdinalIgnoreCase)) return true; // chấp nhận AD, AD_, AD-...
            return false;
        }

        /// <summary>
        /// Lấy tất cả role từ ClaimsPrincipal (Role & "role") và xác định có ADMIN hay không theo IsAdminRoleString.
        /// </summary>
        public static bool IsAdmin(ClaimsPrincipal user)
        {
            if (user == null) return false;

            // Gom cả ClaimTypes.Role và claim "role" tuỳ backend phát hành token
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value)
                            .Concat(user.FindAll("role").Select(c => c.Value));

            foreach (var r in roles)
            {
                if (IsAdminRoleString(r)) return true;
            }

            // Một số hệ thống nhét role vào "roles" dạng chuỗi CSV
            var rolesCsv = user.FindFirst("roles")?.Value;
            if (!string.IsNullOrWhiteSpace(rolesCsv))
            {
                foreach (var r in rolesCsv.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                {
                    if (IsAdminRoleString(r)) return true;
                }
            }

            return false;
        }
    }
}
