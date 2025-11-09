// File: Models/ChucNang.cs
namespace api.Models
{
    public class ChucNang
    {
        public int MaChucNang { get; set; }
        public string CodeChucNang { get; set; } = null!;
        public string TenChucNang { get; set; } = null!;
        public string? MoTa { get; set; }
    }
}

