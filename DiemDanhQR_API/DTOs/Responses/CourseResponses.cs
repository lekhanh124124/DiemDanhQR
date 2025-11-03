// File: DTOs/Responses/CourseResponses.cs
using System.Text.Json.Serialization;

namespace DiemDanhQR_API.DTOs.Responses
{
    public class CourseListItem
    {
        public string? MaLopHocPhan { get; set; }
        public string? TenLopHocPhan { get; set; }
        public bool? TrangThai { get; set; }

        public string? MaMonHoc { get; set; }
        public string? TenMonHoc { get; set; }
        public byte? SoTinChi { get; set; }
        public byte? SoTiet { get; set; }

        public string? MaGiangVien { get; set; }
        public string? TenGiangVien { get; set; }

        // HocKy
        public int? MaHocKy { get; set; }
        public short? NamHoc { get; set; }
        public byte? Ky { get; set; }

        // Thông tin tham gia (ẩn nếu null)
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? NgayThamGia { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? TrangThaiThamGia { get; set; }
    }

    public class SubjectListItem
    {
        public string? MaMonHoc { get; set; }
        public string? TenMonHoc { get; set; }
        public byte? SoTinChi { get; set; }
        public byte? SoTiet { get; set; }
        public string? MoTa { get; set; }
        public bool? TrangThai { get; set; }
    }

    public class CreateSubjectResponse
    {
        public string? MaMonHoc { get; set; } = default!;
        public string? TenMonHoc { get; set; } = default!;
    }

    public class CreateCourseResponse
    {
        public string? MaLopHocPhan { get; set; } = default!;
        public string? TenLopHocPhan { get; set; } = default!;
        public string? MaMonHoc { get; set; } = default!;
        public string? MaGiangVien { get; set; } = default!;
        public int? MaHocKy { get; set; }
    }



    public class SemesterListItem
    {
        public int? MaHocKy { get; set; }
        public short? NamHoc { get; set; }
        public byte? Ky { get; set; }
    }

    public class CreateSemesterResponse
    {
        public int? MaHocKy { get; set; }
        public short? NamHoc { get; set; }
        public byte? Ky { get; set; }
    }

    public class UpdateSemesterResponse
    {
        public int? MaHocKy { get; set; }
        public short? NamHoc { get; set; }
        public byte? Ky { get; set; }
    }

    public class UpdateSubjectResponse
    {
        public string? MaMonHoc { get; set; }
        public string? TenMonHoc { get; set; }
        public byte? SoTinChi { get; set; }
        public byte? SoTiet { get; set; }
        public string? MoTa { get; set; }
        public bool? TrangThai { get; set; }
    }

    public class UpdateCourseResponse
    {
        public string? MaLopHocPhan { get; set; }
        public string? TenLopHocPhan { get; set; }
        public bool? TrangThai { get; set; }

        public string? MaMonHoc { get; set; }
        public string? MaGiangVien { get; set; }
        public int? MaHocKy { get; set; }
    }
}
