using System;

namespace DiemDanhQR_API.Models
{
    public class GiangVien
    {
        public int? MaNguoiDung { get; set; }
        public string? MaGiangVien { get; set; }
        public string? Khoa { get; set; }
        public string? HocHam { get; set; }
        public string? HocVi { get; set; }
        public DateTime? NgayTuyenDung { get; set; }
    }
}
