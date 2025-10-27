// File: DTOs/Requests/ScheduleRequests.cs
namespace DiemDanhQR_API.DTOs.Requests
{
    public class ScheduleListRequest
    {
        // Tìm kiếm
        public string? Keyword { get; set; }
        public int? MaBuoi { get; set; }
        public int? MaPhong { get; set; }
        public string? TenPhong { get; set; }
        public string? MaLopHocPhan { get; set; }
        public string? TenLop { get; set; }
        public string? TenMonHoc { get; set; }
        public DateTime? NgayHoc { get; set; }
        public byte? TietBatDau { get; set; }
        public byte? SoTiet { get; set; }
        public string? GhiChu { get; set; }

        public string? MaSinhVien { get; set; }    // để xem lịch học của SV
        public string? MaGiangVien { get; set; }   // để xem lịch dạy của GV

        // Sắp xếp & phân trang
        public string? SortBy { get; set; }     // MaBuoi (default) | MaPhong | TenPhong | MaLop | TenLop | TenMonHoc | NgayHoc | TietBatDau | SoTiet | GhiChu | SoTinChi | HocKy | TenGiangVien | TrangThai
        public string? SortDir { get; set; }    // ASC | DESC (default: ASC)
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class RoomListRequest
    {
        // Paging
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Sorting: MaPhong (default), TenPhong, ToaNha, Tang, SucChua, TrangThai
        public string? SortBy { get; set; } = "MaPhong";
        public string? SortDir { get; set; } = "ASC"; // ASC | DESC

        // Filters
        public string? Keyword { get; set; }   // OR theo nhiều trường
        public int? MaPhong { get; set; }
        public string? TenPhong { get; set; }
        public string? ToaNha { get; set; }
        public byte? Tang { get; set; }        // ← đổi sang byte
        public byte? SucChua { get; set; }     // ← đổi sang byte
        public bool? TrangThai { get; set; }
    }

}
