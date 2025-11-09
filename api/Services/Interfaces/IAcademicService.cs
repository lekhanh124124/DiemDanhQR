// File: Services/Interfaces/IAcademicService.cs

using api.DTOs;

namespace api.Services.Interfaces
{
    public interface IAcademicService
    {
        // Khoa
        Task<PagedResult<KhoaDetailResponse>> GetKhoaListAsync(KhoaListRequest request);
        Task<KhoaDetailResponse> CreateKhoaAsync(CreateKhoaRequest request);
        Task<KhoaDetailResponse> UpdateKhoaAsync(UpdateKhoaRequest request);
        Task<bool> DeleteKhoaAsync(int maKhoa);

        // Nganh
        Task<PagedResult<NganhDetailResponse>> GetNganhListAsync(NganhListRequest request);
        Task<NganhDetailResponse> CreateNganhAsync(CreateNganhRequest request);
        Task<NganhDetailResponse> UpdateNganhAsync(UpdateNganhRequest request);
        Task<bool> DeleteNganhAsync(int maNganh);
    }
}
