namespace api.Models;

public class GiangVien
{
    public int MaNguoiDung { get; set; } // PK & FK -> NguoiDung
    public string MaGiangVien { get; set; } = null!;
    public int? MaKhoa { get; set; }
    public string? HocHam { get; set; }
    public string? HocVi { get; set; }
    public DateOnly? NgayTuyenDung { get; set; }
}