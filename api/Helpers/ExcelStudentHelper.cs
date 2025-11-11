// File: Helpers/ExcelStudentHelper.cs
using System.Globalization;
using System.Text.RegularExpressions;

namespace api.Helpers
{
    public static class ExcelStudentHelper
    {
        // Map "1|2|3" hoặc "Nam|Nu/Nữ|Khac/Khác" -> 1/2/3
        public static byte? ParseGender(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var v = raw.Trim().ToLowerInvariant();
            if (v is "1" or "nam" or "Nam") return 1;
            if (v is "2" or "nu" or "nữ" or "Nu" or "Nữ") return 2;
            if (v is "3" or "khac" or "khác" or "Khac" or "Khác") return 3;
            return null;
        }
        
        public static bool? ParseBool(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var v = raw.Trim().ToLowerInvariant();

            // số/boolean
            if (v is "1" or "true" or "yes" or "y" or "x") return true;
            if (v is "0" or "false" or "no" or "n") return false;

            // tiếng Việt phổ biến
            if (v is "hoatdong" or "hoạt động" or "danghoc" or "đang học" or "thamgia" or "đangthamgia") return true;
            if (v is "ngunghoc" or "ngừng học" or "nghihoc" or "nghỉ học" or "khong" or "không" or "khongthamgia") return false;

            return null;
        }

        // Email regex “vừa đủ”
        private static readonly Regex EmailRx = new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static bool IsValidEmail(string? email)
            => !string.IsNullOrWhiteSpace(email) && EmailRx.IsMatch(email.Trim());

        // dd/MM/yyyy hoặc yyyy-MM-dd
        public static DateOnly? ParseDate(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            raw = raw.Trim();
            string[] fmts = { "yyyy-MM-dd", "dd/MM/yyyy" };
            if (DateTime.TryParseExact(raw, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return DateOnly.FromDateTime(dt);
            // cố gắng parse “thường”
            if (DateTime.TryParse(raw, out var dt2))
                return DateOnly.FromDateTime(dt2);
            return null;
        }

        public static string? CleanString(string? s)
            => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        // Số điện thoại: giữ nguyên dạng chuỗi, chấp nhận [0-9+() -]
        private static readonly Regex PhoneRx = new(@"^[0-9\+\-\s\(\)]{3,}$", RegexOptions.Compiled);
        public static string? NormalizePhone(string? s)
        {
            s = CleanString(s);
            if (string.IsNullOrEmpty(s)) return null;
            return PhoneRx.IsMatch(s) ? s : null;
        }
    }
}
