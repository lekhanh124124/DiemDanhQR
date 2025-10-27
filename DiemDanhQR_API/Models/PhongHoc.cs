// File: Models/PhongHoc.cs
using System.ComponentModel.DataAnnotations;
namespace DiemDanhQR_API.Models
{
    public class PhongHoc
    {
        public int? MaPhong { get; set; }
        public string? TenPhong { get; set; }
        public string? ToaNha { get; set; }
        public byte? Tang { get; set; }
        public byte? SucChua { get; set; }
        public bool? TrangThai { get; set; }
    }
}
