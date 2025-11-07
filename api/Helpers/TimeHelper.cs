// File: Helpers/TimeHelper.cs

namespace api.Helpers
{
    public static class TimeHelper
    {

        public static string FormatDateTime(DateTime dt) => dt.ToString("dd-MM-yyyy HH:mm:ss");

        // UTC -> giờ Việt Nam (DateTime, Kind=Unspecified)
        public static DateTime UtcToVietnam(DateTime utc)
        {
            if (utc.Kind == DateTimeKind.Local) utc = utc.ToUniversalTime();
            else if (utc.Kind == DateTimeKind.Unspecified) utc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
            var vnLocal = TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            return DateTime.SpecifyKind(vnLocal, DateTimeKind.Unspecified);
        }
    }
}