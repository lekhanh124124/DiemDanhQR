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

    public class UserActivityItem
    {
        public int MaLichSu { get; }
        public DateTime ThoiGian { get; }
        public string HanhDong { get; }
        public string MaNguoiDung { get; }
        public string TenDangNhap { get; }

        public UserActivityItem(int maLichSu, DateTime thoiGian, string hanhDong, string maNguoiDung, string tenDangNhap)
        {
            MaLichSu = maLichSu;
            ThoiGian = thoiGian;
            HanhDong = hanhDong;
            MaNguoiDung = maNguoiDung;
            TenDangNhap = tenDangNhap;
        }
    }
}
