// File: DTOs/Responses/ScheduleResponses.cs
namespace DiemDanhQR_API.DTOs.Responses
{
    public class ScheduleListItem
    {
        public int? MaBuoi { get; set; }
        public int? MaPhong { get; set; }
        public string? TenPhong { get; set; }
        public string? MaLopHocPhan { get; set; }
        public string? TenLopHocPhan { get; set; }
        public string? TenMonHoc { get; set; }
        public string? NgayHoc { get; set; }
        public byte? TietBatDau { get; set; }
        public byte? SoTiet { get; set; }
        public string? TenGiangVien { get; set; }
        public string? GhiChu { get; set; }
        public bool? TrangThai { get; set; }
    }

    public class RoomListItem
    {
        public int? MaPhong { get; set; }
        public string? TenPhong { get; set; }
        public string? ToaNha { get; set; }
        public byte? Tang { get; set; }
        public byte? SucChua { get; set; }
        public bool? TrangThai { get; set; }
    }
    public class CreateRoomResponse
    {
        public int MaPhong { get; set; }
        public string TenPhong { get; set; } = default!;
        public string? ToaNha { get; set; }
        public byte? Tang { get; set; }
        public byte? SucChua { get; set; }
        public bool? TrangThai { get; set; }
    }
    public class CreateScheduleResponse
    {
        public int MaBuoi { get; set; }
        public string MaLopHocPhan { get; set; } = default!;
        public int MaPhong { get; set; }
        public string TenPhong { get; set; } = "";
        public string NgayHoc { get; set; } = "";   // dd-MM-yyyy
        public byte TietBatDau { get; set; }
        public byte SoTiet { get; set; }
        public string? GhiChu { get; set; }
        public bool? TrangThai { get; set; }
    }
    public class UpdateRoomResponse
    {
        public int MaPhong { get; set; }
        public string TenPhong { get; set; } = default!;
        public string? ToaNha { get; set; }
        public byte? Tang { get; set; }
        public byte? SucChua { get; set; }
        public bool? TrangThai { get; set; }
    }

    public class UpdateScheduleResponse
    {
        public int MaBuoi { get; set; }
        public string MaLopHocPhan { get; set; } = default!;
        public int MaPhong { get; set; }
        public string TenPhong { get; set; } = "";
        public string NgayHoc { get; set; } = "";   // dd-MM-yyyy
        public byte TietBatDau { get; set; }
        public byte SoTiet { get; set; }
        public string? GhiChu { get; set; }
        public bool? TrangThai { get; set; }
    }
}
