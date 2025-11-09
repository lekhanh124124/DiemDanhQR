// File: DTOs/Responses/CourseResponses.cs
using System.Text.Json.Serialization;

namespace api.DTOs
{
    public class CourseListItem
    {
        public LopHocPhanDTO? LopHocPhan { get; set; } // MaLopHocPhan, TenLopHocPhan, TrangThai
        public MonHocDTO? MonHoc { get; set; } // MaMonHoc, TenMonHoc, SoTinChi, SoTiet
        public GiangVienDTO? GiangVien { get; set; } // MaGiangVien, TenGiangVien
        public HocKyDTO? HocKy { get; set; } // MaHocKy, NamHoc, Ky
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ThamGiaLopDTO? ThamGiaLop { get; set; } // NgayThamGia, TrangThai
    }

    public class SubjectListItem
    {
        public MonHocDTO? MonHoc { get; set; } // MaMonHoc, TenMonHoc, SoTinChi, SoTiet, MoTa, TrangThai
    }

    public class CreateSubjectResponse
    {
        public MonHocDTO? MonHoc { get; set; } // MaMonHoc, TenMonHoc, SoTinChi, SoTiet, MoTa, TrangThai
    }

    public class CreateCourseResponse
    {
        public LopHocPhanDTO? LopHocPhan { get; set; } // MaLopHocPhan, TenLopHocPhan, TrangThai
        public MonHocDTO? MonHoc { get; set; } // MaMonHoc, TenMonHoc, SoTinChi, SoTiet, LoaiMon
        public GiangVienDTO? GiangVien { get; set; } // MaGiangVien, TenGiangVien
        public HocKyDTO? HocKy { get; set; } // MaHocKy, NamHoc, Ky
    }



    public class SemesterListItem
    {
        public HocKyDTO? HocKy { get; set; } // MaHocKy, NamHoc, Ky
    }

    public class CreateSemesterResponse
    {
        public HocKyDTO? HocKy { get; set; } // MaHocKy, NamHoc, Ky
    }

    public class UpdateSemesterResponse
    {
        public HocKyDTO? HocKy { get; set; } // MaHocKy, NamHoc, Ky
    }

    public class UpdateSubjectResponse
    {
        public MonHocDTO? MonHoc { get; set; } // MaMonHoc, TenMonHoc, SoTinChi, SoTiet, MoTa, TrangThai, LoaiMon
    }

    public class UpdateCourseResponse
    {
        
        public LopHocPhanDTO? LopHocPhan { get; set; } // MaLopHocPhan, TenLopHocPhan, TrangThai
        public MonHocDTO? MonHoc { get; set; } // MaMonHoc, TenMonHoc
        public GiangVienDTO? GiangVien { get; set; } // MaGiangVien, TenGiangVien
        public HocKyDTO? HocKy { get; set; } // MaHocKy, NamHoc, Ky
    }
}
