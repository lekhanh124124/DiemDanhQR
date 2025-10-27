// File: Services/Implementations/ScheduleService.cs
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;
using DiemDanhQR_API.Helpers;
using DiemDanhQR_API.Repositories.Interfaces;
using DiemDanhQR_API.Services.Interfaces;

namespace DiemDanhQR_API.Services.Implementations
{
    public class ScheduleService : IScheduleService
    {
        private readonly IScheduleRepository _repo;
        public ScheduleService(IScheduleRepository repo) => _repo = repo;

        public async Task<PagedResult<ScheduleListItem>> GetListAsync(ScheduleListRequest req)
        {
            var page = req.Page <= 0 ? 1 : req.Page;
            var pageSize = req.PageSize <= 0 ? 20 : Math.Min(req.PageSize, 200);
            var sortBy = req.SortBy ?? "MaBuoi";
            var sortDir = (req.SortDir ?? "ASC").Trim().ToUpperInvariant();
            var desc = sortDir == "DESC";

            var (rows, total) = await _repo.SearchSchedulesAsync(
                req.Keyword,
                req.MaBuoi,
                req.MaPhong,
                req.TenPhong,
                HelperFunctions.NormalizeCode(req.MaLopHocPhan),
                req.TenLop,
                req.TenMonHoc,
                req.NgayHoc,
                req.TietBatDau,
                req.SoTiet,
                req.GhiChu,
                HelperFunctions.NormalizeCode(req.MaSinhVien),
                HelperFunctions.NormalizeCode(req.MaGiangVien),
                sortBy,
                desc,
                page,
                pageSize
            );

            var items = rows.Select(x =>
                new ScheduleListItem(
                    x.b.MaBuoi ?? 0,
                    x.p.MaPhong ?? 0,
                    tenPhong: x.p.TenPhong ?? string.Empty,
                    x.l.MaLopHocPhan ?? string.Empty,
                    x.l.TenLopHocPhan ?? string.Empty,
                    x.m.TenMonHoc ?? string.Empty,
                    x.b.NgayHoc ?? DateTime.MinValue,
                    (byte)(x.b.TietBatDau ?? 0),
                    (byte)(x.b.SoTiet ?? 0),
                    x.ndGv.HoTen ?? string.Empty,
                    x.b.GhiChu ?? string.Empty
                )
            ).ToList();


            return new PagedResult<ScheduleListItem>
            {
                Page = page,
                PageSize = pageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            };
        }

        public async Task<PagedResult<RoomListItem>> GetRoomsAsync(RoomListRequest req)
        {
            var page = req.Page <= 0 ? 1 : req.Page;
            var pageSize = req.PageSize <= 0 ? 20 : Math.Min(req.PageSize, 200);
            var sortBy = req.SortBy ?? "MaPhong";
            var sortDir = (req.SortDir ?? "ASC").Trim().ToUpperInvariant();
            var desc = sortDir == "DESC";

            var (rows, total) = await _repo.SearchRoomsAsync(
                keyword: req.Keyword,
                maPhong: req.MaPhong,
                tenPhong: req.TenPhong,
                toaNha: req.ToaNha,
                tang: req.Tang,
                sucChua: req.SucChua,
                trangThai: req.TrangThai,
                sortBy: sortBy,
                desc: desc,
                page: page,
                pageSize: pageSize
            );

            var items = rows.Select(r => new RoomListItem(
                maPhong: r.MaPhong ?? 0,
                tenPhong: r.TenPhong ?? string.Empty,
                toaNha: r.ToaNha ?? string.Empty,
                tang: (byte)(r.Tang ?? 0),         // ← byte
                sucChua: (byte)(r.SucChua ?? 0),   // ← byte
                trangThai: r.TrangThai ?? true
            )).ToList();


            return new PagedResult<RoomListItem>
            {
                Page = page,
                PageSize = pageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            };
        }

    }
}
