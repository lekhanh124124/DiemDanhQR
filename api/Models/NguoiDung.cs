// File: Models/NguoiDung.cs
namespace api.Models
{
    public class NguoiDung
    {
        public int MaNguoiDung { get; set; }

        public string HoTen { get; set; } = null!;
        public byte? GioiTinh { get; set; }
        public string? AnhDaiDien { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public DateOnly? NgaySinh { get; set; }
        public string? DiaChi { get; set; }

        public string TenDangNhap { get; set; } = null!;
        public string MatKhau { get; set; } = null!;
        public bool TrangThai { get; set; } = true;

        public string? RefreshTokenHash { get; set; }
        public DateTime? RefreshTokenIssuedAt { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }
        public Guid? RefreshTokenId { get; set; }
        public DateTime? RefreshTokenRevokedAt { get; set; }

        public int? MaQuyen { get; set; }
    }
}

