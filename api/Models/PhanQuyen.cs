// File: Models/PhanQuyen.cs
namespace api.Models
{
    public class PhanQuyen
    {
        public int MaQuyen { get; set; }
        public string CodeQuyen { get; set; } = null!;
        public string TenQuyen { get; set; } = null!;
        public string? MoTa { get; set; }
    }
}

