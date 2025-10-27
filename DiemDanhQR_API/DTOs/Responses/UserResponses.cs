// File: DTOs/Responses/UserResponses.cs
namespace DiemDanhQR_API.DTOs.Responses
{
    public class CreateUserResponse
    {
        public required string MaNguoiDung { get; set; }
        public required string TenDangNhap { get; set; }
        public required string HoTen { get; set; }
        public required int MaQuyen { get; set; }
        public bool TrangThai { get; set; }
    }

    public class UserActivityItem
    {
        public int? MaLichSu { get; set; }
        public string? ThoiGian  { get; set; }
        public string? HanhDong  { get; set; }
        public string? MaNguoiDung  { get; set; }
        public string? TenDangNhap { get; set; }
    }

    public class UpdateUserProfileResponse
    {
        public required string MaNguoiDung { get; set; }
        public string? HoTen { get; set; }
        public byte? GioiTinh { get; set; }
        public string? AnhDaiDien { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public string? NgaySinh { get; set; }
        public string? DanToc { get; set; }
        public string? TonGiao { get; set; }
        public string? DiaChi { get; set; }
    }
}
