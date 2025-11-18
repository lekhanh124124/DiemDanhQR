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
                req.TenLopHocPhan,
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
                    TrangThai = inputResponse(x.b.TrangThai.ToString())
                };

                // Phòng học (list yêu cầu: MaPhong, TenPhong, TrangThai)
                PhongHocDTO? phong = null;
                if (x.p != null)
                {
                    phong = new PhongHocDTO
                    {
                        MaPhong = inputResponse(x.p.MaPhong.ToString()),
                        TenPhong = inputResponse(x.p.TenPhong ?? "null"),
                        TrangThai = inputResponse(x.p.TrangThai.ToString())
                    };
                }

                // Lớp học phần (MaLopHocPhan, TenLopHocPhan, TrangThai)
                var lhp = new LopHocPhanDTO
                {
                    MaLopHocPhan = inputResponse(x.l.MaLopHocPhan ?? "null"),
                    TenLopHocPhan = inputResponse(x.l.TenLopHocPhan ?? "null"),
                    TrangThai = inputResponse(x.l.TrangThai.ToString())
                };

                // Môn học (MaMonHoc, TenMonHoc, TrangThai) — chỉ map các trường hiện có trong DTO
                var mon = new MonHocDTO
                {
                    MaMonHoc = inputResponse(x.m.MaMonHoc ?? "null"),
                    TenMonHoc = inputResponse(x.m.TenMonHoc ?? "null"),
                    TrangThai = inputResponse(x.m.TrangThai.ToString())
                };

                GiangVienDTO? gv = null;
                NguoiDungDTO? gvInfo = null;
                if (x.gv != null)
                {
                    gv = new GiangVienDTO
                    {
                        MaGiangVien = inputResponse(x.gv.MaGiangVien ?? "null")
                    };
                    var nd = _repo.GetUserByIdAsync(x.gv.MaNguoiDung).Result;
                    gvInfo = new NguoiDungDTO
                    {
                        HoTen = inputResponse(nd?.HoTen ?? "null")
                    };
                }

                return new ScheduleListItem
                {
                    BuoiHoc = buoi,
                    PhongHoc = phong,
                    LopHocPhan = lhp,
                    MonHoc = mon,
                    GiangVien = gv,
                    GiangVienInfo = gvInfo
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
                    TrangThai = inputResponse(r.TrangThai.ToString())
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
                    TrangThai = inputResponse(entity.TrangThai.ToString())
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
                    TrangThai = inputResponse(entity.TrangThai.ToString())
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
                    TrangThai = inputResponse(room.TrangThai.ToString())
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
                    TrangThai = inputResponse(buoi.TrangThai.ToString())
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
        public async Task<List<ScheduleListItem>> AutoGenerateAsync(string maLopHocPhan, string? tenDangNhap)
        {
            // 1) Lấy bundle thông tin lớp, môn, học kỳ, giảng viên
            var bundle = await _repo.GetCourseBundleAsync(maLopHocPhan);
            if (bundle == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy lớp học phần.");

            var (lhp, mon, hk, gv) = bundle.Value;

            // 2) Xác thực số tiết & thiết lập tham số theo LoaiMon
            if (mon.SoTiet <= 0)
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Môn học chưa thiết lập số tiết hợp lệ.");

            var soTietMon = (int)mon.SoTiet;
            byte[] allowedStarts;
            byte soTietMoiBuoi;

            if (mon.LoaiMon is 2 or 3)
            {
                // Môn thực hành
                allowedStarts = new byte[] { 1, 7, 13 };  // Ca sáng/chiều/tối
                soTietMoiBuoi = 5;                        // 5 tiết mỗi buổi
            }
            else
            {
                // Môn lý thuyết
                allowedStarts = new byte[] { 1, 4, 7, 10, 13, 16 };
                soTietMoiBuoi = 3;                        // 3 tiết mỗi buổi
            }

            // Số buổi = ceil(SoTietMon / soTietMoiBuoi)
            var soBuoi = (int)Math.Ceiling(soTietMon / (double)soTietMoiBuoi);

            // 3) Xác định ngày bắt đầu kỳ học (Kỳ 1: 01/08, Kỳ 2: 01/01, Kỳ 3: 01/06)
            var start = hk.Ky switch
            {
                1 => new DateOnly(hk.NamHoc, 8, 1),
                2 => new DateOnly(hk.NamHoc, 1, 1),
                3 => new DateOnly(hk.NamHoc, 6, 1),
                _ => new DateOnly(hk.NamHoc, 8, 1)
            };

            // 4) Dải thứ ưu tiên (Thứ 2..Thứ 7)
            var weekdays = new DayOfWeek[] {
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
        DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday
    };

            // 5) Chọn (phòng, thứ, tiết bắt đầu) phù hợp cho toàn chuỗi buổi
            var rooms = await _repo.GetActiveRoomsAsync();
            (int MaPhong, DayOfWeek Weekday, byte TietBatDau)? chosen = null;

            foreach (var r in rooms)
            {
                foreach (var w in weekdays)
                {
                    var firstDate = ToNextOrSame(start, w);

                    foreach (var tbd in allowedStarts)
                    {
                        var ok = true;
                        for (var i = 0; i < soBuoi; i++)
                        {
                            var d = firstDate.AddDays(i * 7);

                            // Tiết của buổi i (buổi cuối có thể ngắn hơn nếu còn dư)
                            var du = soTietMon % soTietMoiBuoi;
                            byte tiet = (i == soBuoi - 1 && du != 0) ? (byte)du : soTietMoiBuoi;

                            // Kiểm tra trùng lịch phòng và trùng lịch lớp
                            if (await _repo.AnyRoomConflictAsync(r.MaPhong, d, tbd, tiet)
                                || await _repo.AnyCourseConflictAsync(lhp.MaLopHocPhan!, d, tbd, tiet))
                            {
                                ok = false; break;
                            }
                        }

                        if (ok) { chosen = (r.MaPhong, w, tbd); break; }
                    }
                    if (chosen != null) break;
                }
                if (chosen != null) break;
            }

            if (chosen is null)
                ApiExceptionHelper.Throw(ApiErrorCode.Conflict, "Không tìm được lịch/phòng phù hợp để xếp tự động.");

            var (chosenRoom, chosenWeekday, chosenStartTiet) = chosen.Value;
            var firstDay = ToNextOrSame(start, chosenWeekday);

            // 6) Tạo danh sách buổi học
            var buoiList = new List<BuoiHoc>();
            for (var i = 0; i < soBuoi; i++)
            {
                var du = soTietMon % soTietMoiBuoi;
                byte tiet = (i == soBuoi - 1 && du != 0) ? (byte)du : soTietMoiBuoi;

                buoiList.Add(new BuoiHoc
                {
                    MaLopHocPhan = lhp.MaLopHocPhan!,
                    MaPhong = chosenRoom,
                    NgayHoc = firstDay.AddDays(i * 7),
                    TietBatDau = chosenStartTiet,
                    SoTiet = tiet,
                    GhiChu = null,
                    TrangThai = true
                });
            }

            // 7) Ghi DB
            await _repo.AddSchedulesAsync(buoiList);

            // 8) Log
            await _repo.LogActivityAsync(tenDangNhap,
                $"Tự động sinh {buoiList.Count} buổi cho {lhp.MaLopHocPhan} (phòng {chosenRoom}, {chosenWeekday}, tiết {chosenStartTiet}, LoaiMon={mon.LoaiMon}).");

            // 9) Map response: ScheduleListItem (có cả GiangVienInfo nếu có)
            var phong = await _repo.GetRoomByIdAsync(chosenRoom);
            NguoiDungDTO? gvInfo = null;
            if (gv != null)
            {
                var nd = await _repo.GetUserByIdAsync(gv.MaNguoiDung);
                gvInfo = new NguoiDungDTO { HoTen = inputResponse(nd?.HoTen ?? "null") };
            }

            var items = buoiList.Select(b =>
            {
                var buoi = new BuoiHocDTO
                {
                    MaBuoi = inputResponse(b.MaBuoi.ToString()),
                    NgayHoc = inputResponse(b.NgayHoc.ToString("dd-MM-yyyy")),
                    TietBatDau = inputResponse(b.TietBatDau.ToString()),
                    SoTiet = inputResponse(b.SoTiet.ToString()),
                    GhiChu = inputResponse(b.GhiChu ?? "null"),
                    TrangThai = inputResponse(b.TrangThai.ToString())
                };

                var phongDto = phong == null ? null : new PhongHocDTO
                {
                    MaPhong = inputResponse(phong.MaPhong.ToString()),
                    TenPhong = inputResponse(phong.TenPhong ?? "null"),
                    TrangThai = inputResponse(phong.TrangThai.ToString())
                };

                var lhpDto = new LopHocPhanDTO
                {
                    MaLopHocPhan = inputResponse(lhp.MaLopHocPhan ?? "null"),
                    TenLopHocPhan = inputResponse(lhp.TenLopHocPhan ?? "null"),
                    TrangThai = inputResponse(lhp.TrangThai.ToString())
                };

                var monDto = new MonHocDTO
                {
                    MaMonHoc = inputResponse(mon.MaMonHoc ?? "null"),
                    TenMonHoc = inputResponse(mon.TenMonHoc ?? "null"),
                    TrangThai = inputResponse(mon.TrangThai.ToString())
                };

                GiangVienDTO? gvDto = null;
                if (gv != null)
                {
                    gvDto = new GiangVienDTO { MaGiangVien = inputResponse(gv.MaGiangVien ?? "null") };
                }

                return new ScheduleListItem
                {
                    BuoiHoc = buoi,
                    PhongHoc = phongDto,
                    LopHocPhan = lhpDto,
                    MonHoc = monDto,
                    GiangVien = gvDto,
                    GiangVienInfo = gvInfo
                };
            }).ToList();

            return items;

            // local helper
            static DateOnly ToNextOrSame(DateOnly d, DayOfWeek wd)
            {
                var cur = (int)d.DayOfWeek;
                var target = (int)wd;
                var add = (target - cur + 7) % 7;
                return d.AddDays(add);
            }
        }
    }
}
