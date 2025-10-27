// File: Repositories/Interfaces/ICourseRepository.cs
using DiemDanhQR_API.Models;

namespace DiemDanhQR_API.Repositories.Interfaces
{
    public interface ICourseRepository
    {
        Task<(List<(LopHocPhan Lhp, MonHoc Mh, GiangVien Gv, NguoiDung Nd)> Items, int Total)> SearchCoursesAsync(
            string? keyword,
            string? maLopHocPhan,
            string? tenLopHocPhan,
            bool? trangThai,
            string? tenMonHoc,
            byte? soTinChi,
            byte? soTiet,
            byte? hocKy,
            string? tenGiangVien,
            string? maMonHoc,          // NEW
            string? maGiangVien,       // NEW
            string? sortBy,
            bool desc,
            int page,
            int pageSize);

        Task<(List<(ThamGiaLop Tgl, LopHocPhan Lhp, MonHoc Mh, SinhVien Sv, NguoiDung NdSv, GiangVien Gv, NguoiDung NdGv)> Items, int Total)>
            SearchCourseParticipantsAsync(
                string? keyword,
                string? maLopHocPhan,
                string? tenLopHocPhan,
                string? maMonHoc,
                string? tenMonHoc,
                byte? hocKy,                 // NEW
                string? maSinhVien,
                string? tenSinhVien,
                DateTime? ngayFrom,
                DateTime? ngayTo,
                bool? trangThaiThamGia,
                string? maGiangVien,
                string? tenGiangVien,
                string? sortBy,
                bool desc,
                int page,
                int pageSize);

        Task<(List<MonHoc> Items, int Total)> SearchSubjectsAsync(
                    string? keyword,
                    string? maMonHoc,
                    string? tenMonHoc,
                    byte? soTinChi,
                    byte? soTiet,
                    byte? hocKy,
                    bool? trangThai,
                    string? sortBy,
                    bool desc,
                    int page,
                    int pageSize);
    }
}

