// File: Helpers/AttendanceQrHelper.cs
using System.Security.Cryptography;
using System.Text;
using api.Models;
using QRCoder;
using GeoCoordinatePortable;

namespace api.Helpers
{
    public static class AttendanceQrHelper
    {
        public static string SignToken(string payload, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Base64UrlEncode(hash);
        }

        public static string Base64UrlEncode(byte[] input)
        {
            var s = Convert.ToBase64String(input);
            s = s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
            return s;
        }

        public static byte[] Base64UrlDecode(string input)
        {
            var s = input.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 2: s += "=="; break;
                case 3: s += "="; break;
            }
            return Convert.FromBase64String(s);
        }

        public static bool FixedTimeEquals(string a, string b)
        {
            var ba = Encoding.UTF8.GetBytes(a);
            var bb = Encoding.UTF8.GetBytes(b);
            return CryptographicOperations.FixedTimeEquals(ba, bb);
        }

        public static byte[] GenerateQrPng(string content, int pixelsPerModule = 5)
        {
            var generator = new QRCodeGenerator();
            var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);
            var qrCode = new PngByteQRCode(data);
            return qrCode.GetGraphic(pixelsPerModule);
        }

        public static string ResolveStatusCode(BuoiHoc buoi, DateTime nowUtc)
        {
            TimeSpan MapTietToStart(int tiet) => tiet switch
            {
                1 => new TimeSpan(7, 0, 0),
                2 => new TimeSpan(7, 45, 0),
                3 => new TimeSpan(8, 30, 0),
                4 => new TimeSpan(9, 40, 0),
                5 => new TimeSpan(10, 25, 0),
                6 => new TimeSpan(11, 10, 0),
                7 => new TimeSpan(12, 30, 0),
                8 => new TimeSpan(13, 15, 0),
                9 => new TimeSpan(14, 0, 0),
                10 => new TimeSpan(15, 10, 0),
                11 => new TimeSpan(15, 55, 0),
                12 => new TimeSpan(16, 40, 0),
                13 => new TimeSpan(18, 0, 0),
                14 => new TimeSpan(18, 45, 0),
                15 => new TimeSpan(19, 30, 0),
                16 => new TimeSpan(20, 15, 0),
                17 => new TimeSpan(21, 0, 0),
                _ => new TimeSpan(7, 0, 0)
            };

            var ngayHocLocal = buoi.NgayHoc != default
                ? buoi.NgayHoc.ToDateTime(TimeOnly.MinValue)
                : TimeHelper.UtcToVietnam(nowUtc).Date;
            var tietStart = buoi.TietBatDau;
            var cutoffLocal = ngayHocLocal.Add(MapTietToStart(tietStart)).AddMinutes(15);
            var nowLocal = TimeHelper.UtcToVietnam(nowUtc);
            return nowLocal <= cutoffLocal ? "PRESENT" : "LATE";
        }

        public static double DistanceInMeters(double lat1, double lon1, double lat2, double lon2)
        {
            var p1 = new GeoCoordinate(lat1, lon1);
            var p2 = new GeoCoordinate(lat2, lon2);
            return p1.GetDistanceTo(p2);
        }

    }
}