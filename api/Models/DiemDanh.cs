// File: Models/DiemDanh.cs
namespace api.Models
{
    public class DiemDanh
    {
        public int MaDiemDanh { get; set; }
        public DateTime ThoiGianQuet { get; set; }
        public int MaTrangThai { get; set; }
        public string? LyDo { get; set; }
        public bool TrangThai { get; set; } = true;
        public int MaBuoi { get; set; }
        public string MaSinhVien { get; set; } = null!;
    }
}

