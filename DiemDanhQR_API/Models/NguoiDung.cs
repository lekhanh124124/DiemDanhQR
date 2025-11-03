// File: Models/NguoiDung.cs
using System;

namespace DiemDanhQR_API.Models
{
    public class NguoiDung
    {
        public int? MaNguoiDung { get; set; }

        // Hồ sơ
        public string? HoTen { get; set; }
        public byte? GioiTinh { get; set; }
        public string? AnhDaiDien { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? DanToc { get; set; }
        public string? TonGiao { get; set; }
        public string? DiaChi { get; set; }

        // Tài khoản
        public string? TenDangNhap { get; set; }
        public string? MatKhau { get; set; }
        public bool? TrangThai { get; set; }

        public string? RefreshTokenHash { get; set; }
        public DateTime? RefreshTokenIssuedAt { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }
        public Guid? RefreshTokenId { get; set; }
        public DateTime? RefreshTokenRevokedAt { get; set; }

        public int? MaQuyen { get; set; }
    }
}
