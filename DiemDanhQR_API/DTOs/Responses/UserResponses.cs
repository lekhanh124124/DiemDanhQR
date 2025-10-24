// File: DTOs/Responses/UserResponses.cs
namespace DiemDanhQR_API.DTOs.Responses
{
    public class CreateUsertResponse
    {
        public required string MaNguoiDung { get; set; }
        public required string TenDangNhap { get; set; }
        public required string HoTen { get; set; }
        public required int MaQuyen { get; set; }
        public bool TrangThai { get; set; }
    }
}
