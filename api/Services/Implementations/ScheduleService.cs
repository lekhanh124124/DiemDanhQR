// File: Services/Implementations/ScheduleService.cs
using api.DTOs;
using api.ErrorHandling;
using api.Helpers;
using api.Models;
using api.Repositories.Interfaces;
using api.Services.Interfaces;

namespace api.Services.Implementations
{
    public class ScheduleService : IScheduleService
    {
        private readonly IScheduleRepository _repo;
        public ScheduleService(IScheduleRepository repo) => _repo = repo;

        private string inputResponse(string input) => input ?? "null";

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
                req.MaLopHocPhan,
                req.TenLop,
                req.TenMonHoc,
                req.NgayHoc,
                req.Nam,
                req.Tuan,
                req.Thang,
                req.TietBatDau,
                req.SoTiet,
                req.TrangThai,
                req.MaSinhVien,
                req.MaGiangVien,
                sortBy,
                desc,
                page,
                pageSize
            );

            var items = rows.Select(x =>
            {
                // Buổi học
                var buoi = new BuoiHocDTO
                {
                    MaBuoi = inputResponse(x.b.MaBuoi.ToString()),
                    NgayHoc = inputResponse(x.b.NgayHoc.ToString("dd-MM-yyyy")), // format khi trả ra
                    TietBatDau = inputResponse(x.b.TietBatDau.ToString()),
                    SoTiet = inputResponse(x.b.SoTiet.ToString()),
                    GhiChu = inputResponse(x.b.GhiChu ?? "null"),
                    TrangThai = inputResponse(x.b.TrangThai ? "1" : "0")
                };

                // Phòng học (list yêu cầu: MaPhong, TenPhong, TrangThai)
                PhongHocDTO? phong = null;
                if (x.p != null)
                {
                    phong = new PhongHocDTO
                    {
                        MaPhong = inputResponse(x.p.MaPhong.ToString()),
                        TenPhong = inputResponse(x.p.TenPhong ?? "null"),
                        TrangThai = inputResponse(x.p.TrangThai ? "1" : "0")
                    };
                }

                // Lớp học phần (MaLopHocPhan, TenLopHocPhan, TrangThai)
                var lhp = new LopHocPhanDTO
                {
                    MaLopHocPhan = inputResponse(x.l.MaLopHocPhan ?? "null"),
                    TenLopHocPhan = inputResponse(x.l.TenLopHocPhan ?? "null"),
                    TrangThai = inputResponse(x.l.TrangThai ? "1" : "0")
                };

                // Môn học (MaMonHoc, TenMonHoc, TrangThai) — chỉ map các trường hiện có trong DTO
                var mon = new MonHocDTO
                {
                    MaMonHoc = inputResponse(x.m.MaMonHoc ?? "null"),
                    TenMonHoc = inputResponse(x.m.TenMonHoc ?? "null"),
                    TrangThai = inputResponse(x.m.TrangThai ? "1" : "0")
                };

                // Giảng viên (DTO hiện KHÔNG có HoTen — map MaGiangVien nếu có)
                GiangVienDTO? gv = null;
                if (x.gv != null)
                {
                    gv = new GiangVienDTO
                    {
                        MaGiangVien = inputResponse(x.gv.MaGiangVien ?? "null")
                    };
                }

                return new ScheduleListItem
                {
                    BuoiHoc = buoi,
                    PhongHoc = phong,
                    LopHocPhan = lhp,
                    MonHoc = mon,
                    GiangVien = gv
                };
            }).ToList();

            return new PagedResult<ScheduleListItem>
            {
                Page = inputResponse(page.ToString()),
                PageSize = inputResponse(pageSize.ToString()),
                TotalRecords = inputResponse(total.ToString()),
                TotalPages = inputResponse(((int)Math.Ceiling(total / (double)pageSize)).ToString()),
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

            var items = rows.Select(r =>
            {
                var phong = new PhongHocDTO
                {
                    MaPhong = inputResponse(r.MaPhong.ToString()),
                    TenPhong = inputResponse(r.TenPhong ?? "null"),
                    ToaNha = inputResponse(r.ToaNha ?? "null"),
                    Tang = inputResponse((r.Tang?.ToString() ?? "null")),
                    SucChua = inputResponse(r.SucChua.ToString()),
                    TrangThai = inputResponse(r.TrangThai ? "1" : "0")
                };
                return new RoomListItem { PhongHoc = phong };
            }).ToList();

            return new PagedResult<RoomListItem>
            {
                Page = inputResponse(page.ToString()),
                PageSize = inputResponse(pageSize.ToString()),
                TotalRecords = inputResponse(total.ToString()),
                TotalPages = inputResponse(((int)Math.Ceiling(total / (double)pageSize)).ToString()),
                Items = items
            };
        }

        public async Task<CreateRoomResponse> CreateRoomAsync(CreateRoomRequest req, string? tenDangNhap)
        {
            var tenPhong = (req.TenPhong ?? "").Trim();
            if (string.IsNullOrWhiteSpace(tenPhong))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Tên phòng là bắt buộc.");

            if (!req.SucChua.HasValue)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Sức chứa là bắt buộc.");

            if (await _repo.RoomNameExistsAsync(tenPhong))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Tên phòng đã tồn tại.");

            var entity = new PhongHoc
            {
                TenPhong = tenPhong,
                ToaNha = string.IsNullOrWhiteSpace(req.ToaNha) ? null : req.ToaNha!.Trim(),
                Tang = req.Tang,
                SucChua = req.SucChua!.Value,
                TrangThai = req.TrangThai ?? true
            };

            await _repo.AddRoomAsync(entity);

            await _repo.LogActivityAsync(tenDangNhap,
                $"Tạo phòng học: {entity.TenPhong}"
                + (string.IsNullOrWhiteSpace(entity.ToaNha) ? "" : $" - {entity.ToaNha}")
                + (entity.Tang.HasValue ? $" (Tầng {entity.Tang})" : ""));

            return new CreateRoomResponse
            {
                PhongHoc = new PhongHocDTO
                {
                    MaPhong = inputResponse(entity.MaPhong.ToString()),
                    TenPhong = inputResponse(entity.TenPhong ?? "null"),
                    ToaNha = inputResponse(entity.ToaNha ?? "null"),
                    Tang = inputResponse(entity.Tang?.ToString() ?? "null"),
                    SucChua = inputResponse(entity.SucChua.ToString()),
                    TrangThai = inputResponse(entity.TrangThai ? "1" : "0")
                }
            };
        }

        public async Task<CreateScheduleResponse> CreateScheduleAsync(CreateScheduleRequest req, string? tenDangNhap)
        {
            if (string.IsNullOrWhiteSpace(req.MaLopHocPhan))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã lớp học phần là bắt buộc.");
            if (!req.MaPhong.HasValue)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã phòng là bắt buộc.");
            if (!req.NgayHoc.HasValue)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Ngày học là bắt buộc.");
            if (!req.TietBatDau.HasValue)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Tiết bắt đầu là bắt buộc.");
            if (!req.SoTiet.HasValue)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Số tiết là bắt buộc.");

            var maLhp = req.MaLopHocPhan.Trim();
            var maPhong = req.MaPhong!.Value;
            var ngay = req.NgayHoc!.Value;
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

            await _repo.LogActivityAsync(tenDangNhap,
                $"Tạo buổi học: {entity.MaLopHocPhan} - {entity.NgayHoc:dd-MM-yyyy} (Tiết {entity.TietBatDau}, {entity.SoTiet} tiết) - Phòng {entity.MaPhong}");

            var phong = await _repo.GetRoomByIdAsync(entity.MaPhong ?? 0);

            return new CreateScheduleResponse
            {
                BuoiHoc = new BuoiHocDTO
                {
                    MaBuoi = inputResponse(entity.MaBuoi.ToString()),
                    NgayHoc = inputResponse(entity.NgayHoc.ToString("dd-MM-yyyy")),
                    TietBatDau = inputResponse(entity.TietBatDau.ToString()),
                    SoTiet = inputResponse(entity.SoTiet.ToString()),
                    GhiChu = inputResponse(entity.GhiChu ?? "null"),
                    TrangThai = inputResponse(entity.TrangThai ? "1" : "0")
                },
                PhongHoc = phong == null ? null : new PhongHocDTO
                {
                    MaPhong = inputResponse(phong.MaPhong.ToString()),
                    TenPhong = inputResponse(phong.TenPhong ?? "null")
                },
                LopHocPhan = new LopHocPhanDTO
                {
                    MaLopHocPhan = inputResponse(entity.MaLopHocPhan ?? "null"),
                    TenLopHocPhan = null // không join tên lớp tại đây
                }
            };
        }

        public async Task<UpdateRoomResponse> UpdateRoomAsync(UpdateRoomRequest req, string? tenDangNhap)
        {
            if (!req.MaPhong.HasValue || req.MaPhong.Value <= 0)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã phòng không hợp lệ.");

            var room = await _repo.GetRoomForUpdateAsync(req.MaPhong!.Value);
            if (room == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy phòng học.");

            if (!string.IsNullOrWhiteSpace(req.TenPhong))
            {
                var ten = req.TenPhong!.Trim();
                if (await _repo.RoomNameExistsExceptIdAsync(ten, room.MaPhong))
                    ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Tên phòng đã tồn tại.");
                room.TenPhong = ten;
            }

            if (req.ToaNha != null) room.ToaNha = string.IsNullOrWhiteSpace(req.ToaNha) ? null : req.ToaNha!.Trim();
            if (req.Tang.HasValue) room.Tang = req.Tang.Value;
            if (req.SucChua.HasValue) room.SucChua = req.SucChua.Value;
            if (req.TrangThai.HasValue) room.TrangThai = req.TrangThai.Value;

            await _repo.UpdateRoomAsync(room);

            await _repo.LogActivityAsync(tenDangNhap,
                $"Cập nhật phòng học: {room.TenPhong}"
                + (string.IsNullOrWhiteSpace(room.ToaNha) ? "" : $" - {room.ToaNha}")
                + (room.Tang.HasValue ? $" (Tầng {room.Tang})" : ""));

            return new UpdateRoomResponse
            {
                PhongHoc = new PhongHocDTO
                {
                    MaPhong = inputResponse(room.MaPhong.ToString()),
                    TenPhong = inputResponse(room.TenPhong ?? "null"),
                    ToaNha = inputResponse(room.ToaNha ?? "null"),
                    Tang = inputResponse(room.Tang?.ToString() ?? "null"),
                    SucChua = inputResponse(room.SucChua.ToString()),
                    TrangThai = inputResponse(room.TrangThai ? "1" : "0")
                }
            };
        }

        public async Task<UpdateScheduleResponse> UpdateScheduleAsync(UpdateScheduleRequest req, string? tenDangNhap)
        {
            if (!req.MaBuoi.HasValue || req.MaBuoi.Value <= 0)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "Mã buổi học không hợp lệ.");

            var buoi = await _repo.GetScheduleByIdAsync(req.MaBuoi!.Value);
            if (buoi == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy buổi học.");

            var newMaPhong = req.MaPhong ?? (buoi.MaPhong ?? 0);
            var newNgay = req.NgayHoc ?? buoi.NgayHoc;
            var newTietBd = req.TietBatDau ?? buoi.TietBatDau;
            var newSoTiet = req.SoTiet ?? buoi.SoTiet;

            if (req.MaPhong.HasValue && !await _repo.RoomExistsByIdAsync(req.MaPhong.Value))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Phòng học không tồn tại.");

            var maLhp = buoi.MaLopHocPhan ?? "";
            if (await _repo.ScheduleExistsExceptAsync(maLhp, newNgay, newTietBd, excludeMaBuoi: buoi.MaBuoi))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Buổi học đã tồn tại (trùng lớp, ngày, tiết bắt đầu).");

            if (req.MaPhong.HasValue) buoi.MaPhong = newMaPhong;
            if (req.NgayHoc.HasValue) buoi.NgayHoc = newNgay;
            if (req.TietBatDau.HasValue) buoi.TietBatDau = newTietBd;
            if (req.SoTiet.HasValue) buoi.SoTiet = newSoTiet;
            if (req.GhiChu != null) buoi.GhiChu = string.IsNullOrWhiteSpace(req.GhiChu) ? null : req.GhiChu!.Trim();
            if (req.TrangThai.HasValue) buoi.TrangThai = req.TrangThai.Value;

            await _repo.UpdateScheduleAsync(buoi);
            var phong = await _repo.GetRoomByIdAsync(buoi.MaPhong ?? 0);

            await _repo.LogActivityAsync(tenDangNhap,
                $"Cập nhật buổi học: {buoi.MaLopHocPhan} - {buoi.NgayHoc:dd-MM-yyyy} (Tiết {buoi.TietBatDau}, {buoi.SoTiet} tiết) - Phòng {buoi.MaPhong}");

            return new UpdateScheduleResponse
            {
                BuoiHoc = new BuoiHocDTO
                {
                    MaBuoi = inputResponse(buoi.MaBuoi.ToString()),
                    NgayHoc = inputResponse(buoi.NgayHoc.ToString("dd-MM-yyyy")),
                    TietBatDau = inputResponse(buoi.TietBatDau.ToString()),
                    SoTiet = inputResponse(buoi.SoTiet.ToString()),
                    GhiChu = inputResponse(buoi.GhiChu ?? "null"),
                    TrangThai = inputResponse(buoi.TrangThai ? "1" : "0")
                },
                PhongHoc = phong == null ? null : new PhongHocDTO
                {
                    MaPhong = inputResponse(phong.MaPhong.ToString()),
                    TenPhong = inputResponse(phong.TenPhong ?? "null")
                },
                LopHocPhan = new LopHocPhanDTO
                {
                    MaLopHocPhan = inputResponse(buoi.MaLopHocPhan ?? "null"),
                }
            };
        }
    }
}
