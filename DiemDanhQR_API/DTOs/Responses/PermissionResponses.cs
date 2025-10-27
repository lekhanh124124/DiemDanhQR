// File: DTOs/Responses/PermissionResponses.cs
namespace DiemDanhQR_API.DTOs.Responses
{
    public class PermissionListItem
    {
        public int MaQuyen { get; }
        public string? CodeQuyen { get; }
        public string? TenQuyen { get; }
        public string? MoTa { get; }

        public PermissionListItem(int maQuyen, string? codeQuyen, string? tenQuyen, string? moTa)
        {
            MaQuyen = maQuyen;
            CodeQuyen = codeQuyen;
            TenQuyen = tenQuyen;
            MoTa = moTa;
        }
    }
    public class FunctionListItem
    {
        public int MaChucNang { get; }
        public string? CodeChucNang { get; }
        public string? TenChucNang { get; }
        public string? MoTa { get; }
        public bool TrangThai { get; }

        public FunctionListItem(int maChucNang, string? codeChucNang, string? tenChucNang, string? moTa, bool trangThai)
        {
            MaChucNang = maChucNang;
            CodeChucNang = codeChucNang;
            TenChucNang = tenChucNang;
            MoTa = moTa;
            TrangThai = trangThai;
        }
    }
}
