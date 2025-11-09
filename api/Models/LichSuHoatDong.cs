// File: Models/LichSuHoatDong.cs
namespace api.Models
{
    public class LichSuHoatDong
    {
        public int MaLichSu { get; set; }
        public DateTime ThoiGian { get; set; }
        public string HanhDong { get; set; } = null!;
        public int? MaNguoiDung { get; set; }
    }
}

