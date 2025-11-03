using System;

// File: Models/LichSuHoatDong.cs
namespace DiemDanhQR_API.Models
{
    public class LichSuHoatDong
    {
        public int? MaLichSu { get; set; }
        public DateTime? ThoiGian { get; set; }
        public string? HanhDong { get; set; }
        public int? MaNguoiDung { get; set; }
    }
}
