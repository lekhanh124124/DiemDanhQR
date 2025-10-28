// File: Models/BuoiHoc.cs
namespace DiemDanhQR_API.Models
{
    public class BuoiHoc
    {
        public int? MaBuoi { get; set; }
        public DateTime? NgayHoc { get; set; }
        public byte? TietBatDau { get; set; }
        public byte? SoTiet { get; set; }
        public string? GhiChu { get; set; }

        public string? MaLopHocPhan { get; set; }
        public int? MaPhong { get; set; }
        public bool? TrangThai { get; set; } 
    }
}
