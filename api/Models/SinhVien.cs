// File: Models/SinhVien.cs
namespace api.Models
{
    public class SinhVien
    {
        public int MaNguoiDung { get; set; }
        public string MaSinhVien { get; set; } = null!;
        public int NamNhapHoc { get; set; }
        public int? MaNganh { get; set; }
    }
}

