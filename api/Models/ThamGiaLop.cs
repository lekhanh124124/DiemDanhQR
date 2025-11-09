// File: Models/ThamGiaLop.cs
namespace api.Models
{
    public class ThamGiaLop
    {
        public DateOnly NgayThamGia { get; set; }
        public bool TrangThai { get; set; } = true;
        public string MaSinhVien { get; set; } = null!;
        public string MaLopHocPhan { get; set; } = null!;
    }
}

