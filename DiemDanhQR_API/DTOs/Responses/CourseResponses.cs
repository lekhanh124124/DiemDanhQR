// File: DTOs/Responses/CourseResponses.cs
namespace DiemDanhQR_API.DTOs.Responses
{
    public class CourseListItem
    {
        public string MaLopHocPhan { get; }
        public string TenLopHocPhan { get; }
        public bool TrangThai { get; }

        public string MaMonHoc { get; }
        public string TenMonHoc { get; }
        public byte SoTinChi { get; }
        public byte SoTiet { get; }
        public byte? HocKy { get; }

        public string MaGiangVien { get; }
        public string TenGiangVien { get; }

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
            string tenGiangVien)
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
        }
    }

    public class CourseParticipantItem
    {
        public string MaLopHocPhan { get; }
        public string TenLopHocPhan { get; }

        public string MaMonHoc { get; }
        public string TenMonHoc { get; }
        public byte? HocKy { get; }

        public string MaSinhVien { get; }
        public string TenSinhVien { get; }
        public DateTime NgayThamGia { get; }
        public bool TrangThaiThamGia { get; }

        public string MaGiangVien { get; }
        public string TenGiangVien { get; }

        public CourseParticipantItem(
            string maLopHocPhan,
            string tenLopHocPhan,
            string maMonHoc,
            string tenMonHoc,
            byte? hocKy,
            string maSinhVien,
            string tenSinhVien,
            DateTime ngayThamGia,
            bool trangThaiThamGia,
            string maGiangVien,
            string tenGiangVien)
        {
            MaLopHocPhan = maLopHocPhan;
            TenLopHocPhan = tenLopHocPhan;
            MaMonHoc = maMonHoc;
            TenMonHoc = tenMonHoc;
            HocKy = hocKy;
            MaSinhVien = maSinhVien;
            TenSinhVien = tenSinhVien;
            NgayThamGia = ngayThamGia;
            TrangThaiThamGia = trangThaiThamGia;
            MaGiangVien = maGiangVien;
            TenGiangVien = tenGiangVien;
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
