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
    }
}
