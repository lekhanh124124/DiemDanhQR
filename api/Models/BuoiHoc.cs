// File: Models/BuoiHoc.cs
namespace api.Models
{
    public class BuoiHoc
    {

        public int MaBuoi { get; set; }
        public DateOnly NgayHoc { get; set; }
        public byte TietBatDau { get; set; }
        public byte SoTiet { get; set; }
        public string? GhiChu { get; set; }
        public string MaLopHocPhan { get; set; } = null!;
        public int? MaPhong { get; set; }
        public bool TrangThai { get; set; } = true;
    }
}

