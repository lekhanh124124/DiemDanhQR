namespace api.Models;

public class LopHocPhan
{
    public string MaLopHocPhan { get; set; } = null!;
    public string TenLopHocPhan { get; set; } = null!;
    public bool TrangThai { get; set; } = true;

    public string MaMonHoc { get; set; } = null!;
    public string? MaGiangVien { get; set; }
    public int MaHocKy { get; set; }
}