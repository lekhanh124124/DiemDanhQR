// File: Models/NhomChucNang.cs
namespace api.Models
{
    public class NhomChucNang
    {
        public int MaQuyen { get; set; }
        public int MaChucNang { get; set; }
        public bool TrangThai { get; set; } = true;
    }
}

