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

        public string? MaSinhVien { get; set; }   
        public string? MaGiangVien { get; set; }   

        // Sắp xếp & phân trang
        public string? SortBy { get; set; }    
        public string? SortDir { get; set; }    
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class RoomListRequest
    {
        // Paging
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Sorting
        public string? SortBy { get; set; } = "MaPhong";
        public string? SortDir { get; set; } = "ASC"; // ASC | DESC

        // Filters
        public string? Keyword { get; set; }   // OR theo nhiều trường
        public int? MaPhong { get; set; }
        public string? TenPhong { get; set; }
        public string? ToaNha { get; set; }
        public byte? Tang { get; set; }    
        public byte? SucChua { get; set; }  
        public bool? TrangThai { get; set; }
    }

}
