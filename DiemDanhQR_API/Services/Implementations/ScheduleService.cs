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
                req.MaBuoi,
                req.MaPhong,
                req.TenPhong,
                HelperFunctions.NormalizeCode(req.MaLopHocPhan),
                req.TenLop,
                req.TenMonHoc,
                req.NgayHoc,
                req.Nam,       // NEW
                req.Tuan,      // keep
                req.Thang,     // keep
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

            // Ghi log theo TenDangNhap -> map sang MaNguoiDung trong repo
            await _repo.LogActivityAsync(currentUserId, $"Tạo phòng học: {entity.TenPhong}"
                + (string.IsNullOrWhiteSpace(entity.ToaNha) ? "" : $" - {entity.ToaNha}")
                + (entity.Tang.HasValue ? $" (Tầng {entity.Tang})" : ""));

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

            if (!await _repo.CourseExistsByCodeAsync(maLhp))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Lớp học phần không tồn tại.");
            if (!await _repo.RoomExistsByIdAsync(maPhong))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Phòng học không tồn tại.");

            if (await _repo.ScheduleExistsAsync(maLhp, ngay, tietBd))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Buổi học đã tồn tại (trùng lớp, ngày, tiết bắt đầu).");

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

            await _repo.LogActivityAsync(currentUserId,
                $"Tạo buổi học: {entity.MaLopHocPhan} - {entity.NgayHoc:dd-MM-yyyy} (Tiết {entity.TietBatDau}, {entity.SoTiet} tiết) - Phòng {entity.MaPhong}");

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

        public async Task<UpdateRoomResponse> UpdateRoomAsync(UpdateRoomRequest req, string? currentUserId)
        {
            if (!req.MaPhong.HasValue || req.MaPhong.Value <= 0)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã phòng không hợp lệ.");

            var room = await _repo.GetRoomForUpdateAsync(req.MaPhong.Value);
            if (room == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy phòng học.");

            // Trùng tên (ngoại trừ chính nó)
            if (!string.IsNullOrWhiteSpace(req.TenPhong))
            {
                var ten = req.TenPhong!.Trim();
                if (await _repo.RoomNameExistsExceptIdAsync(ten, room.MaPhong ?? 0))
                    ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Tên phòng đã tồn tại.");
                room.TenPhong = ten;
            }

            if (req.ToaNha != null) room.ToaNha = string.IsNullOrWhiteSpace(req.ToaNha) ? null : req.ToaNha!.Trim();
            if (req.Tang.HasValue) room.Tang = req.Tang;
            if (req.SucChua.HasValue) room.SucChua = req.SucChua;
            if (req.TrangThai.HasValue) room.TrangThai = req.TrangThai;

            await _repo.UpdateRoomAsync(room);

            await _repo.LogActivityAsync(currentUserId,
                $"Cập nhật phòng học: {room.TenPhong}"
                + (string.IsNullOrWhiteSpace(room.ToaNha) ? "" : $" - {room.ToaNha}")
                + (room.Tang.HasValue ? $" (Tầng {room.Tang})" : ""));

            return new UpdateRoomResponse
            {
                MaPhong = room.MaPhong ?? 0,
                TenPhong = room.TenPhong ?? "",
                ToaNha = room.ToaNha,
                Tang = room.Tang,
                SucChua = room.SucChua,
                TrangThai = room.TrangThai
            };
        }

        public async Task<UpdateScheduleResponse> UpdateScheduleAsync(UpdateScheduleRequest req, string? currentUserId)
        {
            if (!req.MaBuoi.HasValue || req.MaBuoi.Value <= 0)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã buổi học không hợp lệ.");

            var buoi = await _repo.GetScheduleByIdAsync(req.MaBuoi.Value);
            if (buoi == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy buổi học.");

            // Chuẩn bị giá trị mới để check trùng trước khi cập nhật
            var newMaPhong = req.MaPhong ?? (buoi.MaPhong ?? 0);
            var newNgay = (req.NgayHoc ?? buoi.NgayHoc ?? DateTime.Now).Date;
            var newTietBd = req.TietBatDau ?? (byte)(buoi.TietBatDau ?? 0);
            var newSoTiet = req.SoTiet ?? (byte)(buoi.SoTiet ?? 0);

            // Kiểm tra phòng tồn tại nếu có cập nhật
            if (req.MaPhong.HasValue && !await _repo.RoomExistsByIdAsync(req.MaPhong.Value))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Phòng học không tồn tại.");

            // Kiểm tra trùng (cùng lớp, cùng ngày, cùng tiết bắt đầu) nhưng loại trừ chính bản ghi hiện tại
            var maLhp = buoi.MaLopHocPhan ?? "";
            if (await _repo.ScheduleExistsAsync(maLhp, newNgay, newTietBd, excludeMaBuoi: buoi.MaBuoi ?? 0))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Buổi học đã tồn tại (trùng lớp, ngày, tiết bắt đầu).");

            // Cập nhật
            if (req.MaPhong.HasValue) buoi.MaPhong = newMaPhong;
            if (req.NgayHoc.HasValue) buoi.NgayHoc = newNgay;
            if (req.TietBatDau.HasValue) buoi.TietBatDau = newTietBd;
            if (req.SoTiet.HasValue) buoi.SoTiet = newSoTiet;
            if (req.GhiChu != null) buoi.GhiChu = string.IsNullOrWhiteSpace(req.GhiChu) ? null : req.GhiChu!.Trim();
            if (req.TrangThai.HasValue) buoi.TrangThai = req.TrangThai;

            await _repo.UpdateScheduleAsync(buoi);

            var phong = await _repo.GetRoomByIdAsync(buoi.MaPhong ?? 0);

            await _repo.LogActivityAsync(currentUserId,
                $"Cập nhật buổi học: {buoi.MaLopHocPhan} - {buoi.NgayHoc:dd-MM-yyyy} (Tiết {buoi.TietBatDau}, {buoi.SoTiet} tiết) - Phòng {buoi.MaPhong}");

            return new UpdateScheduleResponse
            {
                MaBuoi = buoi.MaBuoi ?? 0,
                MaLopHocPhan = buoi.MaLopHocPhan ?? "",
                MaPhong = buoi.MaPhong ?? 0,
                TenPhong = phong?.TenPhong ?? "",
                NgayHoc = buoi.NgayHoc?.ToString("dd-MM-yyyy") ?? "",
                TietBatDau = (byte)(buoi.TietBatDau ?? 0),
                SoTiet = (byte)(buoi.SoTiet ?? 0),
                GhiChu = buoi.GhiChu,
                TrangThai = buoi.TrangThai
            };
        }
    }
}
