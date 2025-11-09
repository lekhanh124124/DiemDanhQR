// File: Models/PhongHoc.cs
namespace api.Models
{
    public class PhongHoc
    {
        public int MaPhong { get; set; }
        public string TenPhong { get; set; } = null!;
        public string? ToaNha { get; set; }
        public byte? Tang { get; set; }
        public byte SucChua { get; set; }
        public bool TrangThai { get; set; } = true;
    }
}

