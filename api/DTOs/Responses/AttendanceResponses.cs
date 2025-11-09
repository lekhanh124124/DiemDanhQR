// File: DTOs/Responses/AttendanceResponses.cs

namespace api.DTOs
{
    public class CreateQrResponse
    {
        public string ExpiresAt { get; set; } = string.Empty; // thời gian local (UTC->Vietnam)
        public string Token { get; set; } = string.Empty;
        public string PngBase64 { get; set; } = string.Empty; // image/png (base64)
        public BuoiHocDTO? BuoiHoc { get; set; } // MaBuoi, NgayHoc, TietBatDau, SoTiet
        public LopHocPhanDTO? LopHocPhan { get; set; } // MaLopHocPhan, TenLopHocPhan
    }

    // List item: TrangThaiDiemDanh
    public class AttendanceStatusListItem
    {
        public TrangThaiDiemDanhDTO? TrangThaiDiemDanh { get; set; } // MaTrangThai, TenTrangThai, CodeTrangThai
    }

    public class CreateAttendanceStatus
    {
        public TrangThaiDiemDanhDTO? TrangThaiDiemDanh { get; set; } // MaTrangThai, TenTrangThai, CodeTrangThai
    }

    public class UpdateAttendanceStatus
    {
        public TrangThaiDiemDanhDTO? TrangThaiDiemDanh { get; set; } // MaTrangThai, TenTrangThai, CodeTrangThai
    }

    // List item: DiemDanh
    public class AttendanceListItem
    {
        public DiemDanhDTO? DiemDanh { get; set; } // MaDiemDanh, ThoiGianQuet, LyDo, TrangThai
        public TrangThaiDiemDanhDTO? TrangThaiDiemDanh { get; set; } // MaTrangThai, TenTrangThai, CodeTrangThai
        public BuoiHocDTO? BuoiHoc { get; set; } // MaBuoi, NgayHoc, TietBatDau, SoTiet
        public SinhVienDTO? SinhVien { get; set; } // MaSinhVien, HoTen
        public LopHocPhanDTO? LopHocPhan { get; set; } // MaLopHocPhan, TenLopHocPhan
    }
    public class CreateAttendanceResponse // dùng cho cả CheckIn
    {
        public DiemDanhDTO? DiemDanh { get; set; } // MaDiemDanh, ThoiGianQuet, TrangThai
        public TrangThaiDiemDanhDTO? TrangThaiDiemDanh { get; set; } // MaTrangThai, TenTrangThai, CodeTrangThai
        public SinhVienDTO? SinhVien { get; set; } // MaSinhVien, HoTen
        public BuoiHocDTO? BuoiHoc { get; set; } // MaBuoi, NgayHoc, TietBatDau, SoTiet
        public LopHocPhanDTO? LopHocPhan { get; set; } // MaLopHocPhan, TenLopHocPhan
    }

    public class UpdateAttendanceResponse
    {
        public DiemDanhDTO? DiemDanh { get; set; } // MaDiemDanh, ThoiGianQuet, TrangThai
        public TrangThaiDiemDanhDTO? TrangThaiDiemDanh { get; set; } // MaTrangThai, TenTrangThai, CodeTrangThai
        public SinhVienDTO? SinhVien { get; set; } // MaSinhVien, HoTen
        public BuoiHocDTO? BuoiHoc { get; set; } // MaBuoi, NgayHoc, TietBatDau, SoTiet
        public LopHocPhanDTO? LopHocPhan { get; set; } // MaLopHocPhan, TenLopHocPhan
    }

}