// File: Models/Nganh.cs
namespace api.Models
{
    public class Nganh
    {
        public int MaNganh { get; set; }
        public string CodeNganh { get; set; } = null!;
        public string TenNganh { get; set; } = null!;
        public int MaKhoa { get; set; }
    }
}

