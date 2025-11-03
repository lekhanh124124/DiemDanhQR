// Models/LopHocPhan.cs
namespace DiemDanhQR_API.Models
{
    public class LopHocPhan
    {
        public string? MaLopHocPhan { get; set; }
        public string? TenLopHocPhan { get; set; }
        public bool? TrangThai { get; set; }

        public string? MaMonHoc { get; set; }
        public string? MaGiangVien { get; set; }
        public int? MaHocKy { get; set; }
    }
}
