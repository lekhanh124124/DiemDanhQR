// File: Models/GiangVien.cs
namespace api.Models
{
    public class GiangVien
    {
        public int MaNguoiDung { get; set; }
        public string MaGiangVien { get; set; } = null!;
        public int? MaKhoa { get; set; }
        public string? HocHam { get; set; }
        public string? HocVi { get; set; }
        public DateOnly? NgayTuyenDung { get; set; }
    }
}

