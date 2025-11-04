// File: Controllers/AttendanceController.cs
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using DiemDanhQR_API.Data;
using DiemDanhQR_API.Helpers;
using DiemDanhQR_API.DTOs.Responses;
using DiemDanhQR_API;
using QRCoder;

namespace DiemDanhQR_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _cfg;

        // Replay guard (in-memory cho 1 instance)
        private static readonly ConcurrentDictionary<string, long> _usedNonces = new();

        public AttendanceController(AppDbContext db, IConfiguration cfg)
        {
            _db = db;
            _cfg = cfg;
        }

        // POST: /api/attendance/qr?maBuoi=123&ttlSeconds=300&pixelsPerModule=5
        [HttpPost("qr")]
        [Authorize(Roles = "GV")]
        [Produces("image/png")]
        public async Task<IActionResult> CreateQr(
            [FromQuery] int maBuoi,
            [FromQuery] int ttlSeconds = 300,
            [FromQuery] int pixelsPerModule = 5)
        {
            if (ttlSeconds <= 0 || ttlSeconds > 3600) ttlSeconds = 300;
            if (pixelsPerModule < 3 || pixelsPerModule > 20) pixelsPerModule = 5;

            var buoi = await _db.BuoiHoc.AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaBuoi == maBuoi && x.TrangThai == true)
                ?? throw new ApiException(ApiErrorCode.NotFound, "Không tìm thấy buổi học hoặc bị khoá.");

            if (string.IsNullOrWhiteSpace(buoi.MaLopHocPhan))
                throw new ApiException(ApiErrorCode.BadRequest, "Buổi học chưa có mã lớp học phần.");

            var lhpActive = await _db.LopHocPhan.AsNoTracking()
                .AnyAsync(x => x.MaLopHocPhan == buoi.MaLopHocPhan && x.TrangThai == true);
            if (!lhpActive)
                throw new ApiException(ApiErrorCode.NotFound, "Lớp học phần không tồn tại/đã khoá.");

            // Token: maBuoi|maLopHocPhan|expUnix|nonce|sig
            var expUtc  = DateTime.UtcNow.AddSeconds(ttlSeconds);
            var expUnix = new DateTimeOffset(expUtc).ToUnixTimeSeconds();
            var nonce   = Guid.NewGuid().ToString("N");

            var payload = $"{maBuoi}|{buoi.MaLopHocPhan}|{expUnix}|{nonce}";
            var sig     = SignToken(payload, GetQrSecret());
            var token   = Base64UrlEncode(Encoding.UTF8.GetBytes($"{payload}|{sig}"));

            // QR chỉ chứa TOKEN 
            var pngBytes = GenerateQrPng(token, pixelsPerModule);

            // Tránh cache lâu ở proxy/browser
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
            Response.Headers["Pragma"]        = "no-cache";
            Response.Headers["Expires"]       = "0";

            // Log lịch sử (dùng MaNguoiDung)
            var gv = await GetCurrentNguoiDungAsync();
            await LogHistoryAsync(gv?.MaNguoiDung, $"CREATE_QR_TOKEN|Buoi:{maBuoi}|LHP:{buoi.MaLopHocPhan}|TTL:{ttlSeconds}s");

            return File(pngBytes, "image/png");
        }

        // POST: /api/attendance/close?maBuoi=123
        [HttpPost("close")]
        [Authorize(Roles = "GV")]
        public async Task<IActionResult> CloseSession([FromQuery] int maBuoi)
        {
            var buoi = await _db.BuoiHoc.FirstOrDefaultAsync(x => x.MaBuoi == maBuoi)
                ?? throw new ApiException(ApiErrorCode.NotFound, "Không tìm thấy buổi học.");

            buoi.TrangThai = false; // khoá buổi
            await _db.SaveChangesAsync();

            var gv = await GetCurrentNguoiDungAsync();
            await LogHistoryAsync(gv?.MaNguoiDung, $"CLOSE_SESSION|Buoi:{maBuoi}|LHP:{buoi.MaLopHocPhan}");

            return NoContent();
        }

        // POST: /api/attendance/checkin?token=...
        [HttpPost("checkin")]
        [Authorize(Roles = "SV")]
        public async Task<ActionResult<ApiResponse<object>>> CheckInByQr([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ApiException(ApiErrorCode.BadRequest, "Thiếu token.");

            var tokenRaw = Encoding.UTF8.GetString(Base64UrlDecode(token));
            var parts = tokenRaw.Split('|');
            if (parts.Length != 5)
                throw new ApiException(ApiErrorCode.BadRequest, "Token không hợp lệ.");

            var maBuoiStr    = parts[0];
            var maLopHocPhan = parts[1];
            var expUnixStr   = parts[2];
            var nonce        = parts[3];
            var sig          = parts[4];

            if (!int.TryParse(maBuoiStr, out var maBuoi))
                throw new ApiException(ApiErrorCode.BadRequest, "Mã buổi trong token không hợp lệ.");
            if (!long.TryParse(expUnixStr, out var expUnix))
                throw new ApiException(ApiErrorCode.BadRequest, "Thời hạn token không hợp lệ.");

            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expUnix)
                throw new ApiException(ApiErrorCode.BadRequest, "Mã QR đã hết hạn.");

            var payload     = $"{maBuoi}|{maLopHocPhan}|{expUnixStr}|{nonce}";
            var expectedSig = SignToken(payload, GetQrSecret());
            if (!FixedTimeEquals(sig, expectedSig))
                throw new ApiException(ApiErrorCode.BadRequest, "Chữ ký không hợp lệ.");

            // Chống replay: 1 nonce chỉ dùng 1 lần trong TTL
            if (_usedNonces.TryGetValue(nonce, out var oldExp))
            {
                if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() <= oldExp)
                    throw new ApiException(ApiErrorCode.BadRequest, "QR đã được sử dụng.");
                _usedNonces.TryRemove(nonce, out _); // dọn rác quá hạn
            }
            _usedNonces.TryAdd(nonce, expUnix);

            return await WriteAttendanceAsync(maBuoi, maLopHocPhan);
        }

        // ===== Shared: ghi điểm danh (auto ONTIME/LATE, lưu UTC, log) =====
        private async Task<ActionResult<ApiResponse<object>>> WriteAttendanceAsync(int maBuoi, string maLopHocPhan)
        {
            var buoi = await _db.BuoiHoc.AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaBuoi == maBuoi && x.MaLopHocPhan == maLopHocPhan && x.TrangThai == true)
                ?? throw new ApiException(ApiErrorCode.NotFound, "Buổi học không tồn tại hoặc đã bị khoá.");

            var lhpActive = await _db.LopHocPhan.AsNoTracking()
                .AnyAsync(x => x.MaLopHocPhan == maLopHocPhan && x.TrangThai == true);
            if (!lhpActive)
                throw new ApiException(ApiErrorCode.NotFound, "Lớp học phần không tồn tại hoặc đã bị khoá.");

            var username = HelperFunctions.GetUserIdFromClaims(User)
                ?? throw new ApiException(ApiErrorCode.Unauthorized, "Không xác định người dùng.");

            var nd = await _db.NguoiDung.AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenDangNhap == username && x.TrangThai == true)
                ?? throw new ApiException(ApiErrorCode.Unauthorized, "Tài khoản không hợp lệ hoặc đã bị khoá.");

            var sv = await _db.SinhVien.AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaNguoiDung == nd.MaNguoiDung)
                ?? throw new ApiException(ApiErrorCode.Forbidden, "Tài khoản hiện tại không phải sinh viên.");

            var maSinhVien = sv.MaSinhVien;
            var inClass = await _db.ThamGiaLop.AsNoTracking()
                .AnyAsync(x => x.MaLopHocPhan == maLopHocPhan && x.TrangThai == true && x.MaSinhVien == maSinhVien);
            if (!inClass)
                throw new ApiException(ApiErrorCode.Forbidden, "Sinh viên không thuộc lớp học phần này.");

            var existed = await _db.DiemDanh.AsNoTracking()
                .AnyAsync(x => x.MaBuoi == maBuoi && x.MaSinhVien == maSinhVien && x.TrangThai == true);
            if (existed)
                throw new ApiException(ApiErrorCode.Conflict, "Bạn đã điểm danh buổi này rồi.");

            // Auto ONTIME/LATE
            var statusId = await ResolveStatusAsync(buoi, DateTime.UtcNow);

            // Lưu UTC
            var nowUtc = DateTime.UtcNow;
            var entity = new Models.DiemDanh
            {
                MaBuoi       = maBuoi,
                MaSinhVien   = maSinhVien,
                MaTrangThai  = statusId,
                ThoiGianQuet = nowUtc, // UTC
                TrangThai    = true
            };
            _db.DiemDanh.Add(entity);
            await _db.SaveChangesAsync();

            // Log lịch sử: dùng MaNguoiDung
            await LogHistoryAsync(nd.MaNguoiDung, $"CHECKIN|Buoi:{maBuoi}|LHP:{maLopHocPhan}|SV:{maSinhVien}|StatusId:{statusId}");

            var local = HelperFunctions.UtcToVietnam(nowUtc);
            return Ok(new ApiResponse<object>
            {
                Status  = 200,
                Message = "Điểm danh thành công.",
                Data = new
                {
                    maBuoi,
                    maLopHocPhan,
                    maSinhVien,
                    thoiGianQuetUtc   = nowUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                    thoiGianQuetLocal = local.ToString("dd-MM-yyyy HH:mm:ss"),
                    maTrangThai       = entity.MaTrangThai
                }
            });
        }

        // ===== Helpers =====
        private string GetQrSecret()
        {
            var secret = _cfg["QRSettings:QrSecretKey"];
            if (string.IsNullOrWhiteSpace(secret))
                throw new Exception("Chưa cấu hình QRSettings:QrSecretKey.");
            return secret!;
        }

        private static string SignToken(string payload, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Base64UrlEncode(hash);
        }

        private static string Base64UrlEncode(byte[] input)
        {
            var s = Convert.ToBase64String(input);
            s = s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
            return s;
        }

        private static byte[] Base64UrlDecode(string input)
        {
            var s = input.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 2: s += "=="; break;
                case 3: s += "="; break;
            }
            return Convert.FromBase64String(s);
        }

        private static bool FixedTimeEquals(string a, string b)
        {
            var ba = Encoding.UTF8.GetBytes(a);
            var bb = Encoding.UTF8.GetBytes(b);
            return CryptographicOperations.FixedTimeEquals(ba, bb);
        }

        private async Task<int?> TryGetTrangThaiIdByCode(string code)
        {
            var item = await _db.TrangThaiDiemDanh.AsNoTracking()
                .FirstOrDefaultAsync(x => x.CodeTrangThai == code);
            return item?.MaTrangThai;
        }

        // Auto ONTIME/LATE theo lịch 18 tiết (45p/tiết, nghỉ 25p sau 3 tiết mỗi buổi)
        private async Task<int?> ResolveStatusAsync(Models.BuoiHoc buoi, DateTime nowUtc)
        {
            TimeSpan MapTietToStart(int tiet) => tiet switch
            {
                1  => new TimeSpan(7,  0, 0),
                2  => new TimeSpan(7, 45, 0),
                3  => new TimeSpan(8, 30, 0),
                4  => new TimeSpan(9, 40, 0),
                5  => new TimeSpan(10,25, 0),
                6  => new TimeSpan(11,10, 0),

                7  => new TimeSpan(12,30, 0),
                8  => new TimeSpan(13,15, 0),
                9  => new TimeSpan(14, 0, 0),
                10 => new TimeSpan(15,10, 0),
                11 => new TimeSpan(15,55, 0),
                12 => new TimeSpan(16,40, 0),

                13 => new TimeSpan(18, 0, 0),
                14 => new TimeSpan(18,45, 0),
                15 => new TimeSpan(19,30, 0),
                16 => new TimeSpan(20,40, 0),
                17 => new TimeSpan(21,25, 0),
                18 => new TimeSpan(22,10, 0),
                _  => new TimeSpan(7,  0, 0)
            };

            DateTime ngayHocLocal;
            if (buoi.NgayHoc is DateTime dt) ngayHocLocal = dt.Date;
            else
            {
                var prop = buoi.GetType().GetProperty("NgayHoc");
                var val = prop?.GetValue(buoi);
                if (val != null && val.GetType().Name == "DateOnly")
                {
                    var year  = (int)val.GetType().GetProperty("Year")!.GetValue(val)!;
                    var month = (int)val.GetType().GetProperty("Month")!.GetValue(val)!;
                    var day   = (int)val.GetType().GetProperty("Day")!.GetValue(val)!;
                    ngayHocLocal = new DateTime(year, month, day);
                }
                else ngayHocLocal = HelperFunctions.UtcToVietnam(nowUtc).Date;
            }

            var tietStart      = (int)(buoi.GetType().GetProperty("TietBatDau")?.GetValue(buoi) ?? 1);
            var startTimeLocal = MapTietToStart(tietStart);
            var cutoffLocal    = ngayHocLocal.Add(startTimeLocal).AddMinutes(15); // trễ sau 15'

            var nowLocal = HelperFunctions.UtcToVietnam(nowUtc);
            var code     = nowLocal <= cutoffLocal ? "ONTIME" : "LATE";
            var st       = await _db.TrangThaiDiemDanh.AsNoTracking()
                .FirstOrDefaultAsync(x => x.CodeTrangThai == code);
            return st?.MaTrangThai;
        }

        // Ghi lịch sử (dùng MaNguoiDung, không dùng TenDangNhap)
        private async Task LogHistoryAsync(int? maNguoiDung, string action)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(action)) return;

                var ls = new Models.LichSuHoatDong
                {
                    ThoiGian    = DateTime.UtcNow, // UTC
                    HanhDong    = action,
                    MaNguoiDung = maNguoiDung
                };
                _db.LichSuHoatDong.Add(ls);
                await _db.SaveChangesAsync();
            }
            catch
            {
                // tránh làm hỏng flow chính
            }
        }

        private async Task<Models.NguoiDung?> GetCurrentNguoiDungAsync()
        {
            var username = HelperFunctions.GetUserIdFromClaims(User);
            if (string.IsNullOrWhiteSpace(username)) return null;

            return await _db.NguoiDung.AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenDangNhap == username);
        }

        private static byte[] GenerateQrPng(string content, int pixelsPerModule = 5)
        {
            var generator = new QRCodeGenerator();
            var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);
            var qrCode = new PngByteQRCode(data);
            return qrCode.GetGraphic(pixelsPerModule);
        }
    }
}
