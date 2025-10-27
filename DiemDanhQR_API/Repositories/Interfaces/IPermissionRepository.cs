// File: Repositories/Interfaces/IPermissionRepository.cs
using DiemDanhQR_API.Models;

namespace DiemDanhQR_API.Repositories.Interfaces
{
    public interface IPermissionRepository
    {
        Task<(List<PhanQuyen> Items, int Total)> SearchAsync(
            string? keyword,
            int? maQuyen,
            string? codeQuyen,
            string? tenQuyen,
            string? moTa,
            string? sortBy,
            bool desc,
            int page,
            int pageSize
        );

        Task<(List<ChucNang> Items, int Total)> SearchFunctionsAsync(
            string? keyword,
            int? maChucNang,
            string? codeChucNang,
            string? tenChucNang,
            string? moTa,
            bool? trangThai,
            int? maQuyen,
            string? sortBy,
            bool desc,
            int page,
            int pageSize
        );
    }
}
