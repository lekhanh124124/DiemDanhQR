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
        public SinhVienDTO? SinhVien { get; set; } // MaSinhVien
        public LopHocPhanDTO? LopHocPhan { get; set; } // MaLopHocPhan, TenLopHocPhan
    }
    public class CreateAttendanceResponse // dùng cho cả CheckIn
    {
        public DiemDanhDTO? DiemDanh { get; set; } // MaDiemDanh, ThoiGianQuet, TrangThai
        public TrangThaiDiemDanhDTO? TrangThaiDiemDanh { get; set; } // MaTrangThai, TenTrangThai, CodeTrangThai
        public SinhVienDTO? SinhVien { get; set; } // MaSinhVien
        public BuoiHocDTO? BuoiHoc { get; set; } // MaBuoi, NgayHoc, TietBatDau, SoTiet
        public LopHocPhanDTO? LopHocPhan { get; set; } // MaLopHocPhan, TenLopHocPhan
    }

    public class UpdateAttendanceResponse
    {
        public DiemDanhDTO? DiemDanh { get; set; } // MaDiemDanh, ThoiGianQuet, TrangThai
        public TrangThaiDiemDanhDTO? TrangThaiDiemDanh { get; set; } // MaTrangThai, TenTrangThai, CodeTrangThai
        public SinhVienDTO? SinhVien { get; set; } // MaSinhVien
        public BuoiHocDTO? BuoiHoc { get; set; } // MaBuoi, NgayHoc, TietBatDau, SoTiet
        public LopHocPhanDTO? LopHocPhan { get; set; } // MaLopHocPhan, TenLopHocPhan
    }

    public class AttendanceFacultyRatioItem
    {
        public KhoaDTO? Khoa { get; set; }

        // Tổng số bản ghi điểm danh (cả vắng + có mặt)
        public string? TongBuoi { get; set; }

        // Số buổi vắng (TrangThai == false)
        public string? TongVang { get; set; }

        // Tỉ lệ vắng (%)
        public string? TyLeVang { get; set; }

        // Số buổi có mặt (TrangThai == true)
        public string? TongCoMat { get; set; }

        // Tỉ lệ có mặt (%)
        public string? TyLeCoMat { get; set; }
    }

    public class AttendanceLopHocPhanRatioItem
    {
        public LopHocPhanDTO? LopHocPhan { get; set; }
        public MonHocDTO? MonHoc { get; set; }

        public string? TongBuoi { get; set; }
        public string? TongVang { get; set; }
        public string? TyLeVang { get; set; }
        public string? TongCoMat { get; set; }
        public string? TyLeCoMat { get; set; }
    }
}