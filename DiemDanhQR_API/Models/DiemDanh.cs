// File: Models/DiemDanh.cs
namespace DiemDanhQR_API.Models
{
    public class DiemDanh
    {
        public int? MaDiemDanh { get; set; }
        public DateTime? ThoiGianQuet { get; set; }
        public int? MaTrangThai { get; set; }
        public string? LyDo { get; set; }
        public bool? TrangThai { get; set; }

        public int? MaBuoi { get; set; }
        public string? MaSinhVien { get; set; }
    }
}