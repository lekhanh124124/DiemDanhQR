// File: DTOs/Responses/AcademicResponses.cs
namespace api.DTOs
{
    public class KhoaDetailResponse
    {
        public required KhoaDTO Khoa { get; set; } // MaKhoa, TenKhoa, CodeKhoa
    }

    public class NganhDetailResponse
    {
        public required NganhDTO Nganh { get; set; } // MaNganh, TenNganh, CodeNganh
        public KhoaDTO? Khoa { get; set; } // MaKhoa, TenKhoa, CodeKhoa
    }
}
