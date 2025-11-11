// File: Repositories/Interfaces/ICourseRepository.cs
using api.Models;

namespace api.Repositories.Interfaces
{
    public interface ICourseRepository
    {
        Task<(List<(LopHocPhan Lhp, MonHoc Mh, GiangVien Gv, HocKy Hk, DateOnly? NgayThamGia, bool? TrangThaiThamGia)> Items, int Total)>
            SearchCoursesAsync(
                string? maLopHocPhan,
                string? tenLopHocPhan,
                bool? trangThai,
                string? maMonHoc,
                byte? soTinChi,
                string? maGiangVien,
                int? maHocKy,
                short? namHoc,
                byte? ky,
                string? maSinhVien,
                string? sortBy,
                bool desc,
                int page,
                int pageSize);

        Task<(List<MonHoc> Items, int Total)> SearchSubjectsAsync(
            string? maMonHoc,
            string? tenMonHoc,
            byte? soTinChi,
            byte? soTiet,
            bool? trangThai,
            string? sortBy,
            bool desc,
            int page,
            int pageSize);

        Task<bool> SubjectExistsAsync(string maMonHoc);
        Task AddSubjectAsync(MonHoc subject);

        Task<bool> CourseExistsAsync(string maLopHocPhan);
        Task AddCourseAsync(LopHocPhan course);

        Task<bool> LecturerExistsByCodeAsync(string maGiangVien);
        Task<bool> SemesterExistsByIdAsync(int maHocKy);

        Task LogActivityAsync(string? tenDangNhap, string hanhDong);

        Task<(List<HocKy> Items, int Total)> SearchSemestersAsync(short? namHoc, byte? ky, string? sortBy, bool desc, int page, int pageSize);
        Task<bool> ExistsSemesterAsync(short? namHoc, byte? ky, int? excludeId = null);
        Task AddSemesterAsync(HocKy hk);
        Task<HocKy?> GetSemesterByIdAsync(int maHocKy);
        Task UpdateSemesterAsync(HocKy hk);

        Task<MonHoc?> GetSubjectByCodeAsync(string maMonHoc);
        Task UpdateSubjectAsync(MonHoc subject);
        Task<LopHocPhan?> GetCourseByCodeAsync(string maLopHocPhan);
        Task UpdateCourseAsync(LopHocPhan course);

        Task<string> GenerateNextSubjectCodeAsync(string tenMonHoc);
        Task<string> GenerateNextCourseCodeAsync(string tenMonHoc, short namHoc, byte ky);
    }
}
