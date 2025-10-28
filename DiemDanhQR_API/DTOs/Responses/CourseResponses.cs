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
        public byte? HocKy { get; set; }

        public string? MaGiangVien { get; set; }
        public string? TenGiangVien { get; set; }

        // NEW: thông tin tham gia (ẩn nếu null)
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
        public byte? HocKy { get; set; }
        public string? MoTa { get; set; }
        public bool? TrangThai { get; set; }
    }

    public class CreateSubjectResponse
    {
        public string MaMonHoc { get; set; } = default!;
        public string TenMonHoc { get; set; } = default!;
    }

    public class CreateCourseResponse
    {
        public string MaLopHocPhan { get; set; } = default!;
        public string TenLopHocPhan { get; set; } = default!;
        public string MaMonHoc { get; set; } = default!;
        public string MaGiangVien { get; set; } = default!;
    }

    public class AddStudentToCourseResponse
    {
        public string MaLopHocPhan { get; set; } = default!;
        public string MaSinhVien { get; set; } = default!;
        public string? NgayThamGia { get; set; }   // trả về dd-MM-yyyy
        public bool? TrangThai { get; set; }
    }
}
