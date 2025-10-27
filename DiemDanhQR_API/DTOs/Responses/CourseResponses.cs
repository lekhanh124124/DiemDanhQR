// File: DTOs/Responses/CourseResponses.cs
using System.Text.Json.Serialization;

namespace DiemDanhQR_API.DTOs.Responses
{
    public class CourseListItem
    {
        public string MaLopHocPhan { get; set; }
        public string TenLopHocPhan { get; set; }
        public bool TrangThai { get; set; }

        public string MaMonHoc { get; set; }
        public string TenMonHoc { get; set; }
        public byte SoTinChi { get; set; }
        public byte SoTiet { get; set; }
        public byte? HocKy { get; set; }

        public string MaGiangVien { get; set; }
        public string TenGiangVien { get; set; }

        // NEW: thông tin tham gia (ẩn nếu null)
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? NgayThamGia { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? TrangThaiThamGia { get; set; }

        public CourseListItem(
            string maLopHocPhan,
            string tenLopHocPhan,
            bool trangThai,
            string maMonHoc,
            string tenMonHoc,
            byte soTinChi,
            byte soTiet,
            byte? hocKy,
            string maGiangVien,
            string tenGiangVien,
            DateTime? ngayThamGia = null,     
            bool? trangThaiThamGia = null     
        )
        {
            MaLopHocPhan = maLopHocPhan;
            TenLopHocPhan = tenLopHocPhan;
            TrangThai = trangThai;

            MaMonHoc = maMonHoc;
            TenMonHoc = tenMonHoc;
            SoTinChi = soTinChi;
            SoTiet = soTiet;
            HocKy = hocKy;

            MaGiangVien = maGiangVien;
            TenGiangVien = tenGiangVien;

            NgayThamGia = ngayThamGia;
            TrangThaiThamGia = trangThaiThamGia;
        }
    }

    public class SubjectListItem
    {
        public string MaMonHoc { get; }
        public string TenMonHoc { get; }
        public byte SoTinChi { get; }
        public byte SoTiet { get; }
        public byte? HocKy { get; }
        public string MoTa { get; }
        public bool TrangThai { get; }

        public SubjectListItem(
            string maMonHoc,
            string tenMonHoc,
            byte soTinChi,
            byte soTiet,
            byte? hocKy,
            string moTa,
            bool trangThai)
        {
            MaMonHoc = maMonHoc;
            TenMonHoc = tenMonHoc;
            SoTinChi = soTinChi;
            SoTiet = soTiet;
            HocKy = hocKy;
            MoTa = moTa;
            TrangThai = trangThai;
        }
    }
}
