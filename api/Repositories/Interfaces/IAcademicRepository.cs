// File: Repositories/Interfaces/IAcademicRepository.cs
using api.Models;

namespace api.Repositories.Interfaces
{
    public interface IAcademicRepository
    {
        // Khoa
        Task<(List<Khoa> Items, int Total)> SearchKhoaAsync(
            int? maKhoa, string? codeKhoa, string? tenKhoa,
            string? sortBy, bool desc, int page, int pageSize);

        Task<bool> KhoaCodeExistsAsync(string codeKhoa, int? excludeId = null);
        Task<Khoa?> GetKhoaByIdAsync(int maKhoa);
        Task AddKhoaAsync(Khoa entity);
        Task UpdateKhoaAsync(Khoa entity);
        Task DeleteKhoaAsync(Khoa entity);
        Task<bool> AnyNganhInKhoaAsync(int maKhoa);

        // Nganh
        Task<(List<Nganh> Items, int Total)> SearchNganhAsync(
            int? maNganh, string? codeNganh, string? tenNganh, int? maKhoa,
            string? sortBy, bool desc, int page, int pageSize);

        Task<bool> NganhCodeExistsAsync(string codeNganh, int? excludeId = null);
        Task<Nganh?> GetNganhByIdAsync(int maNganh);
        Task AddNganhAsync(Nganh entity);
        Task UpdateNganhAsync(Nganh entity);
        Task DeleteNganhAsync(Nganh entity);

        Task<bool> KhoaExistsAsync(int maKhoa);
        Task<Nganh?> GetNganhByCodeAsync(string codeNganh);
    }
}
