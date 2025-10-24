// File: Models/MonHoc.cs
namespace DiemDanhQR_API.Models
{
    public class MonHoc
    {
        public string? MaMonHoc { get; set; }
        public string? TenMonHoc { get; set; }
        public byte? SoTinChi { get; set; }
        public byte? SoTiet { get; set; }
        public byte? HocKy { get; set; }
        public string? MoTa { get; set; }
        public bool? TrangThai { get; set; }
    }
}