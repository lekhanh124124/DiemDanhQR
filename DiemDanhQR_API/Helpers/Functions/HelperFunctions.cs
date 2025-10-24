// File: Helpers/Functions/HelperFunctions.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace DiemDanhQR_API.Helpers
{
    public static class HelperFunctions
    {
        //Chuẩn hoá mã: trim, bỏ khoảng giữa
        public static string NormalizeCode(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var s = input.Trim();
            s = Regex.Replace(s, @"\s+", ""); // bỏ whitespace
            return s;
        }

        //Tạo refresh token ngẫu nhiên 64 bytes (Base64Url, không ký tự đặc biệt).
        public static string GenerateSecureRefreshToken(int byteLength = 64)
        {
            var bytes = RandomNumberGenerator.GetBytes(byteLength);
            // Base64Url: thay '+' -> '-', '/' -> '_', bỏ '='
            var b64 = Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
            return b64;
        }
        
        //Lấy MaNguoiDung từ Claims nếu có. Ưu tiên: NameIdentifier -> Name -> Identity.Name
        public static string? GetUserIdFromClaims(ClaimsPrincipal? user)
            => user?.FindFirstValue(ClaimTypes.NameIdentifier);

        // Giờ Việt Nam (Unspecified) -> UTC (DateTime UTC). Dùng khi đọc từ DB (đã lưu theo giờ VN) rồi cần chuẩn hoá về UTC.
        public static DateTime VietnamToUtc(DateTime vietnamTime)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var offset = tz.GetUtcOffset(vietnamTime);
            var dto = new DateTimeOffset(vietnamTime, offset);
            return dto.UtcDateTime;
        }

        // UTC -> giờ Việt Nam (DateTime). Trả về Kind=Unspecified để EF không tự đổi múi giờ khi lưu DB (datetime/datetime2).
        public static DateTime UtcToVietnam(DateTime utc)
        {
            if (utc.Kind == DateTimeKind.Local) utc = utc.ToUniversalTime();
            else if (utc.Kind == DateTimeKind.Unspecified) utc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
            var vnLocal = TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            return DateTime.SpecifyKind(vnLocal, DateTimeKind.Unspecified);
        }
    }
}
