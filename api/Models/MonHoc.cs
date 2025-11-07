namespace api.Models;

public class MonHoc
{
    public string MaMonHoc { get; set; } = null!;
    public string TenMonHoc { get; set; } = null!;
    public byte SoTinChi { get; set; }
    public byte SoTiet { get; set; }
    public string? MoTa { get; set; }
    public bool TrangThai { get; set; } = true;
    public byte LoaiMon { get; set; } = 1;
}