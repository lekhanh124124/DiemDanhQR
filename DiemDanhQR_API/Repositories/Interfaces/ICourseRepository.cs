// File: Repositories/Interfaces/ICourseRepository.cs
using DiemDanhQR_API.Models;

namespace DiemDanhQR_API.Repositories.Interfaces
{
    public interface ICourseRepository
    {
        Task<(List<(LopHocPhan Lhp, MonHoc Mh, GiangVien Gv, NguoiDung Nd, DateTime? NgayThamGia, bool? TrangThaiThamGia)> Items, int Total)>
                    SearchCoursesAsync(
                        string? keyword,
                        string? maLopHocPhan,
                        string? tenLopHocPhan,
                        bool? trangThai,
                        string? tenMonHoc,
                        byte? soTinChi,
                        byte? soTiet,
                        byte? hocKy,
                        string? tenGiangVien,
                        string? maMonHoc,
                        string? maGiangVien,
                        string? maSinhVien,   // NEW
                        string? sortBy,
                        bool desc,
                        int page,
                        int pageSize
                    );

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

