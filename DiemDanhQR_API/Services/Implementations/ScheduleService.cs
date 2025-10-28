// File: Services/Implementations/ScheduleService.cs
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;
using DiemDanhQR_API.Helpers;
using DiemDanhQR_API.Models;
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
                req.TrangThai,
                HelperFunctions.NormalizeCode(req.MaSinhVien),
                HelperFunctions.NormalizeCode(req.MaGiangVien),
                sortBy,
                desc,
                page,
                pageSize
            );

            var items = rows.Select(x =>
                new ScheduleListItem
                {
                    MaBuoi = x.b.MaBuoi,
                    MaPhong = x.b.MaPhong,
                    TenPhong = x.p?.TenPhong,
                    MaLopHocPhan = x.b.MaLopHocPhan,
                    TenLopHocPhan = x.l?.TenLopHocPhan,
                    TenMonHoc = x.m?.TenMonHoc,
                    NgayHoc = x.b.NgayHoc?.ToString("dd-MM-yyyy"),
                    TietBatDau = x.b.TietBatDau,
                    SoTiet = x.b.SoTiet,
                    TenGiangVien = x.ndGv?.HoTen,
                    GhiChu = x.b.GhiChu,
                    TrangThai = x.b.TrangThai
                }
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

            var items = rows.Select(r => new RoomListItem{
                MaPhong = r.MaPhong,
                TenPhong = r.TenPhong,
                ToaNha = r.ToaNha,
                Tang = r.Tang,
                SucChua = r.SucChua,
                TrangThai = r.TrangThai
            }).ToList();


            return new PagedResult<RoomListItem>
            {
                Page = page,
                PageSize = pageSize,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            };
        }
        public async Task<CreateRoomResponse> CreateRoomAsync(CreateRoomRequest req, string? currentUserId)
        {
            // kiểm tra trùng tên phòng (không phân biệt hoa thường)
            var tenPhong = (req.TenPhong ?? "").Trim();
            if (await _repo.RoomNameExistsAsync(tenPhong))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Tên phòng đã tồn tại.");

            var entity = new PhongHoc
            {
                TenPhong = tenPhong,
                ToaNha = string.IsNullOrWhiteSpace(req.ToaNha) ? null : req.ToaNha!.Trim(),
                Tang = req.Tang,
                SucChua = req.SucChua,
                TrangThai = req.TrangThai ?? true
            };

            await _repo.AddRoomAsync(entity);

            // ghi log
            var log = new LichSuHoatDong
            {
                MaNguoiDung = string.IsNullOrWhiteSpace(currentUserId) ? "system" : currentUserId,
                HanhDong = $"Tạo phòng học: {entity.TenPhong}" +
                           (string.IsNullOrWhiteSpace(entity.ToaNha) ? "" : $" - {entity.ToaNha}") +
                           (entity.Tang.HasValue ? $" (Tầng {entity.Tang})" : "")
            };
            await _repo.WriteActivityLogAsync(log);

            return new CreateRoomResponse
            {
                MaPhong = entity.MaPhong ?? 0,
                TenPhong = entity.TenPhong ?? "",
                ToaNha = entity.ToaNha,
                Tang = entity.Tang,
                SucChua = entity.SucChua,
                TrangThai = entity.TrangThai
            };
        }

        public async Task<CreateScheduleResponse> CreateScheduleAsync(CreateScheduleRequest req, string? currentUserId)
        {
            var maLhp = HelperFunctions.NormalizeCode(req.MaLopHocPhan);
            var maPhong = req.MaPhong ?? 0;
            var ngay = (req.NgayHoc ?? DateTime.Now).Date;
            var tietBd = req.TietBatDau!.Value;
            var soTiet = req.SoTiet!.Value;

            // 1) Check tồn tại lớp học phần & phòng
            if (!await _repo.CourseExistsByCodeAsync(maLhp))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Lớp học phần không tồn tại.");
            if (!await _repo.RoomExistsByIdAsync(maPhong))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Phòng học không tồn tại.");

            // 2) Chặn trùng cùng lớp + cùng ngày + cùng tiết bắt đầu
            if (await _repo.ScheduleExistsAsync(maLhp, ngay, tietBd))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Buổi học đã tồn tại (trùng lớp, ngày, tiết bắt đầu).");

            // (tuỳ chọn) có thể kiểm tra va chạm phòng theo khung tiết ở đây

            // 3) Tạo entity
            var entity = new BuoiHoc
            {
                MaLopHocPhan = maLhp,
                MaPhong = maPhong,
                NgayHoc = ngay,
                TietBatDau = tietBd,
                SoTiet = soTiet,
                GhiChu = string.IsNullOrWhiteSpace(req.GhiChu) ? null : req.GhiChu!.Trim(),
                TrangThai = req.TrangThai ?? true
            };

            await _repo.AddScheduleAsync(entity);

            // 4) Log
            await _repo.WriteActivityLogAsync(new LichSuHoatDong
            {
                MaNguoiDung = string.IsNullOrWhiteSpace(currentUserId) ? "system" : currentUserId,
                HanhDong = $"Tạo buổi học: {entity.MaLopHocPhan} - {entity.NgayHoc:dd-MM-yyyy} (Tiết {entity.TietBatDau}, {entity.SoTiet} tiết) - Phòng {entity.MaPhong}"
            });

            // 5) Lấy tên phòng (nếu muốn trả về đẹp)
            var phong = await _repo.GetRoomByIdAsync(entity.MaPhong ?? 0);

            return new CreateScheduleResponse
            {
                MaBuoi = entity.MaBuoi ?? 0,
                MaLopHocPhan = entity.MaLopHocPhan!,
                MaPhong = entity.MaPhong ?? 0,
                TenPhong = phong?.TenPhong ?? "",
                NgayHoc = entity.NgayHoc?.ToString("dd-MM-yyyy") ?? "",
                TietBatDau = (byte)(entity.TietBatDau ?? 0),
                SoTiet = (byte)(entity.SoTiet ?? 0),
                GhiChu = entity.GhiChu,
                TrangThai = entity.TrangThai ?? true
            };
        }
    }
}
