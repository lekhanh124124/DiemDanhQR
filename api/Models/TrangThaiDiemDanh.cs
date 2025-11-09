// File: Models/TrangThaiDiemDanh.cs
namespace api.Models
{
    public class TrangThaiDiemDanh
    {
        public int MaTrangThai { get; set; }
        public string TenTrangThai { get; set; } = null!;
        public string CodeTrangThai { get; set; } = null!;
    }
}

