// File: DTOs/Responses/ScheduleResponses.cs
namespace api.DTOs
{
    public class ScheduleListItem
    {
        public BuoiHocDTO? BuoiHoc { get; set; } // MaBuoi, NgayHoc, TietBatDau, SoTiet, GhiChu, TrangThai
        public PhongHocDTO? PhongHoc { get; set; } // MaPhong, TenPhong, TrangThai
        public LopHocPhanDTO? LopHocPhan { get; set; } // MaLopHocPhan, TenLop, TrangThai
        public MonHocDTO? MonHoc { get; set; } // MaMonHoc, TenMonHoc, TrangThai
        public GiangVienDTO? GiangVien { get; set; } // MaGiangVien
        public NguoiDungDTO? GiangVienInfo { get; set; } // HoTen
    }
    public class CreateScheduleResponse
    {
        public BuoiHocDTO? BuoiHoc { get; set; } // MaBuoi, NgayHoc, TietBatDau, SoTiet, GhiChu, TrangThai
        public PhongHocDTO? PhongHoc { get; set; } // MaPhong, TenPhong
        public LopHocPhanDTO? LopHocPhan { get; set; } // MaLopHocPhan, TenLop
    }
    public class UpdateScheduleResponse
    {
        public BuoiHocDTO? BuoiHoc { get; set; } // MaBuoi, NgayHoc, TietBatDau, SoTiet, GhiChu, TrangThai
        public PhongHocDTO? PhongHoc { get; set; } // MaPhong, TenPhong
        public LopHocPhanDTO? LopHocPhan { get; set; } // MaLopHocPhan, TenLop
    }
    public class RoomListItem
    {
        public PhongHocDTO? PhongHoc { get; set; } // MaPhong, TenPhong, ToaNha, Tang, SucChua, TrangThai
    }
    public class CreateRoomResponse
    {
        public PhongHocDTO? PhongHoc { get; set; } // MaPhong, TenPhong, ToaNha, Tang, SucChua, TrangThai
    }

    public class UpdateRoomResponse
    {
        public PhongHocDTO? PhongHoc { get; set; } // MaPhong, TenPhong, ToaNha, Tang, SucChua, TrangThai
    }


}
