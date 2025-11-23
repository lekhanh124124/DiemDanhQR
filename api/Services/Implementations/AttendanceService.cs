// File: Services/Implementations/AttendanceService.cs
using System.Globalization;
using System.Text;
using api.DTOs;
using api.ErrorHandling;
using api.Helpers;
using api.Models;
using api.Repositories.Interfaces;
using api.Services.Interfaces;

namespace api.Services.Implementations
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _repo;
        private readonly IConfiguration _cfg;

        public AttendanceService(IAttendanceRepository repo, IConfiguration cfg)
        {
            _repo = repo;
            _cfg = cfg;
        }

        private string inputResponse(string input) => input ?? "null";

        public async Task<CreateQrResponse> CreateQrAsync(CreateQrRequest req, string? currentUsername)
        {
            var ttl = (req.TtlSeconds <= 0 || req.TtlSeconds > 3600) ? 300 : req.TtlSeconds;
            var ppm = (req.PixelsPerModule < 3 || req.PixelsPerModule > 20) ? 5 : req.PixelsPerModule;

            var buoi = await _repo.GetActiveBuoiByIdAsync(req.MaBuoi)
                ?? throw new ApiException(ApiErrorCode.NotFound, "Không tìm thấy buổi học hoặc bị khoá.");

            if (string.IsNullOrWhiteSpace(buoi.MaLopHocPhan))
                throw new ApiException(ApiErrorCode.BadRequest, "Buổi học chưa có mã lớp học phần.");

            if (!await _repo.IsLopHocPhanActiveAsync(buoi.MaLopHocPhan))
                throw new ApiException(ApiErrorCode.NotFound, "Lớp học phần không tồn tại/đã khoá.");

            var lhp = await _repo.GetLopHocPhanByIdAsync(buoi.MaLopHocPhan);

            var expUtc = DateTime.UtcNow.AddSeconds(ttl);
            var expUnix = new DateTimeOffset(expUtc).ToUnixTimeSeconds();
            var nonce = Guid.NewGuid().ToString("N");

            var payload = $"{req.MaBuoi}|{buoi.MaLopHocPhan}|{expUnix}|{nonce}";
            var secret = GetQrSecret();
            var sig = AttendanceQrHelper.SignToken(payload, secret);
            var token = AttendanceQrHelper.Base64UrlEncode(Encoding.UTF8.GetBytes($"{payload}|{sig}"));
            var png = AttendanceQrHelper.GenerateQrPng(token, ppm);

            // Log: ghi DB = giờ Việt Nam
            var nd = string.IsNullOrWhiteSpace(currentUsername)
                ? null
                : await _repo.GetNguoiDungByUsernameAsync(currentUsername);
            await _repo.LogHistoryAsync(nd?.MaNguoiDung, $"CREATE_QR_TOKEN|Buoi:{req.MaBuoi}|LHP:{buoi.MaLopHocPhan}|TTL:{ttl}s");

            return new CreateQrResponse
            {
                ExpiresAt = inputResponse(TimeHelper.FormatDateTime(TimeHelper.UtcToVietnam(expUtc))),
                Token = inputResponse(token),
                PngBase64 = inputResponse(Convert.ToBase64String(png)),
                BuoiHoc = new BuoiHocDTO
                {
                    MaBuoi = inputResponse(buoi.MaBuoi.ToString()),
                    NgayHoc = inputResponse(buoi.NgayHoc.ToString("dd-MM-yyyy")),
                    TietBatDau = inputResponse(buoi.TietBatDau.ToString()),
                    SoTiet = inputResponse(buoi.SoTiet.ToString())
                },
                LopHocPhan = lhp == null ? null : new LopHocPhanDTO
                {
                    MaLopHocPhan = inputResponse(lhp.MaLopHocPhan),
                    TenLopHocPhan = inputResponse(lhp.TenLopHocPhan),
                    TrangThai = inputResponse(lhp.TrangThai.ToString().ToLowerInvariant())
                }
            };
        }

        public async Task<CreateAttendanceResponse> CheckInByQrAsync(CheckInRequest req, string? currentUsername)
        {
            var tokenRaw = Encoding.UTF8.GetString(AttendanceQrHelper.Base64UrlDecode(req.Token));
            var parts = tokenRaw.Split('|');
            if (parts.Length != 5)
                throw new ApiException(ApiErrorCode.BadRequest, "Token không hợp lệ.");

            if (!int.TryParse(parts[0], out var maBuoi))
                throw new ApiException(ApiErrorCode.BadRequest, "Mã buổi trong token không hợp lệ.");
            var maLhp = parts[1];

            if (!long.TryParse(parts[2], out var expUnix))
                throw new ApiException(ApiErrorCode.BadRequest, "Thời hạn token không hợp lệ.");
            var nonce = parts[3];
            var sig = parts[4];

            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expUnix)
                throw new ApiException(ApiErrorCode.BadRequest, "Mã QR đã hết hạn.");

            var payload = $"{maBuoi}|{maLhp}|{parts[2]}|{nonce}";
            var expectedSig = AttendanceQrHelper.SignToken(payload, GetQrSecret());
            if (!AttendanceQrHelper.FixedTimeEquals(sig, expectedSig))
                throw new ApiException(ApiErrorCode.BadRequest, "Chữ ký không hợp lệ.");

            var buoi = await _repo.GetActiveBuoiByIdAsync(maBuoi)
                ?? throw new ApiException(ApiErrorCode.NotFound, "Buổi học không tồn tại hoặc đã bị khoá.");

            if (!await _repo.IsLopHocPhanActiveAsync(maLhp))
                throw new ApiException(ApiErrorCode.NotFound, "Lớp học phần không tồn tại hoặc đã bị khoá.");

            var username = currentUsername ?? throw new ApiException(ApiErrorCode.Unauthorized, "Không xác định người dùng.");
            var nd = await _repo.GetNguoiDungByUsernameAsync(username)
                ?? throw new ApiException(ApiErrorCode.Unauthorized, "Tài khoản không hợp lệ hoặc đã bị khoá.");

            var sv = await _repo.GetSinhVienByMaNguoiDungAsync(nd.MaNguoiDung)
                ?? throw new ApiException(ApiErrorCode.Forbidden, "Tài khoản hiện tại không phải sinh viên.");

            var maSinhVien = sv.MaSinhVien;
            var thamGia = await _repo.GetThamGiaLopAsync(maLhp, maSinhVien);
            if (thamGia == null)
                throw new ApiException(ApiErrorCode.Forbidden, "Sinh viên không thuộc lớp học phần này.");

            if (thamGia.TrangThai == false)
                throw new ApiException(ApiErrorCode.Forbidden, "Sinh viên đã ngừng tham gia lớp học phần này.");

            if (await _repo.AttendanceExistsAsync(maBuoi, maSinhVien))
                throw new ApiException(ApiErrorCode.Conflict, "Bạn đã điểm danh buổi này rồi.");

            string statusCode = AttendanceQrHelper.ResolveStatusCode(buoi, DateTime.UtcNow);
            bool isPresentFlag = true;

            const double refLat = 10.80615211994601;
            const double refLon = 106.62866552433795;

            var distance = AttendanceQrHelper.DistanceInMeters(
                req.Latitude!.Value,
                req.Longitude!.Value,
                refLat,
                refLon
            );

            // Nếu > 150m: đánh FRAUD_LOCATION và coi như vắng
            if (distance > 150.0)
            {
                statusCode = "FRAUD_LOCATION";
                isPresentFlag = false;
            }

            // Lấy MaTrangThai từ CodeTrangThai
            var statusId = await _repo.TryGetTrangThaiIdByCodeAsync(statusCode)
                ?? throw new ApiException(ApiErrorCode.InternalError, "Không tìm thấy mã trạng thái.");

            var nowLocal = TimeHelper.UtcToVietnam(DateTime.UtcNow);
            var entity = new DiemDanh
            {
                MaBuoi = maBuoi,
                MaSinhVien = maSinhVien,
                MaTrangThai = statusId,
                ThoiGianQuet = nowLocal,
                TrangThai = isPresentFlag
            };

            var saved = await _repo.CreateAttendanceAsync(entity);

            await _repo.LogHistoryAsync(
                nd.MaNguoiDung,
                $"CHECKIN|Buoi:{maBuoi}|LHP:{maLhp}|SV:{maSinhVien}|StatusCode:{statusCode}|StatusId:{statusId}|Distance:{distance:F2}m"
            );

            // Build response (format thời gian, không UTC→VN nữa vì đã lưu giờ VN)
            var t = await _repo.GetStatusByIdAsync(statusId);

            return new CreateAttendanceResponse
            {
                DiemDanh = new DiemDanhDTO
                {
                    MaDiemDanh = inputResponse(saved.MaDiemDanh.ToString()),
                    ThoiGianQuet = inputResponse(TimeHelper.FormatDateTime(saved.ThoiGianQuet)),
                    TrangThai = inputResponse(saved.TrangThai.ToString().ToLowerInvariant())
                },
                TrangThaiDiemDanh = t == null ? null : new TrangThaiDiemDanhDTO
                {
                    MaTrangThai = inputResponse(t.MaTrangThai.ToString()),
                    TenTrangThai = inputResponse(t.TenTrangThai),
                    CodeTrangThai = inputResponse(t.CodeTrangThai)
                },
                SinhVien = new SinhVienDTO
                {
                    MaSinhVien = inputResponse(sv.MaSinhVien),
                    NamNhapHoc = inputResponse(sv.NamNhapHoc.ToString())
                },
                BuoiHoc = new BuoiHocDTO
                {
                    MaBuoi = inputResponse(buoi.MaBuoi.ToString()),
                    NgayHoc = inputResponse(buoi.NgayHoc.ToString("dd-MM-yyyy")),
                    TietBatDau = inputResponse(buoi.TietBatDau.ToString()),
                    SoTiet = inputResponse(buoi.SoTiet.ToString())
                },
                LopHocPhan = new LopHocPhanDTO
                {
                    MaLopHocPhan = inputResponse(buoi.MaLopHocPhan),
                    TenLopHocPhan = inputResponse((await _repo.GetLopHocPhanByIdAsync(buoi.MaLopHocPhan))?.TenLopHocPhan ?? "null"),
                }
            };
        }

        public async Task<PagedResult<AttendanceStatusListItem>> GetStatusListAsync(AttendanceStatusListRequest req)
        {
            var page = req.Page <= 0 ? 1 : req.Page;
            var size = req.PageSize <= 0 ? 20 : req.PageSize;
            var sortBy = string.IsNullOrWhiteSpace(req.SortBy) ? "MaTrangThai" : req.SortBy!;
            var desc = string.Equals(req.SortDir, "DESC", StringComparison.OrdinalIgnoreCase);

            var (rows, total) = await _repo.SearchStatusesAsync(
                req.MaTrangThai,
                req.TenTrangThai,
                req.CodeTrangThai,
                sortBy,
                desc,
                page,
                size);

            var items = rows.Select(x => new AttendanceStatusListItem
            {
                TrangThaiDiemDanh = new TrangThaiDiemDanhDTO
                {
                    MaTrangThai = inputResponse(x.MaTrangThai.ToString()),
                    TenTrangThai = inputResponse(x.TenTrangThai),
                    CodeTrangThai = inputResponse(x.CodeTrangThai)
                }
            }).ToList();

            return new PagedResult<AttendanceStatusListItem>
            {
                Page = inputResponse(page.ToString()),
                PageSize = inputResponse(size.ToString()),
                TotalRecords = inputResponse(total.ToString()),
                TotalPages = inputResponse(((int)Math.Ceiling(total / (double)size)).ToString()),
                Items = items
            };
        }

        public async Task<PagedResult<AttendanceListItem>> GetAttendanceListAsync(AttendanceListRequest req)
        {
            var page = req.Page <= 0 ? 1 : req.Page;
            var size = req.PageSize <= 0 ? 20 : req.PageSize;
            var sortBy = string.IsNullOrWhiteSpace(req.SortBy) ? "MaDiemDanh" : req.SortBy!;
            var desc = string.Equals(req.SortDir, "DESC", StringComparison.OrdinalIgnoreCase);

            var (rows, total) = await _repo.SearchAttendancesAsync(
                req.MaDiemDanh,
                req.ThoiGianQuet,
                req.MaTrangThai,
                req.TrangThai,
                req.MaBuoi,
                req.MaSinhVien,
                req.MaLopHocPhan,
                sortBy,
                desc,
                page,
                size);

            var items = rows.Select(x => new AttendanceListItem
            {
                DiemDanh = new DiemDanhDTO
                {
                    MaDiemDanh = inputResponse(x.d.MaDiemDanh.ToString()),
                    ThoiGianQuet = inputResponse(TimeHelper.FormatDateTime(x.d.ThoiGianQuet)),
                    LyDo = x.d.LyDo == null ? "null" : inputResponse(x.d.LyDo),
                    TrangThai = inputResponse(x.d.TrangThai.ToString().ToLowerInvariant())
                },
                TrangThaiDiemDanh = x.t == null ? null : new TrangThaiDiemDanhDTO
                {
                    MaTrangThai = inputResponse(x.t.MaTrangThai.ToString()),
                    TenTrangThai = inputResponse(x.t.TenTrangThai),
                    CodeTrangThai = inputResponse(x.t.CodeTrangThai)
                },
                BuoiHoc = new BuoiHocDTO
                {
                    MaBuoi = inputResponse(x.b.MaBuoi.ToString()),
                    NgayHoc = inputResponse(x.b.NgayHoc.ToString("dd-MM-yyyy")),
                    TietBatDau = inputResponse(x.b.TietBatDau.ToString()),
                    SoTiet = inputResponse(x.b.SoTiet.ToString())
                },
                SinhVien = new SinhVienDTO
                {
                    MaSinhVien = inputResponse(x.s.MaSinhVien),
                    NamNhapHoc = inputResponse(x.s.NamNhapHoc.ToString())
                },
                LopHocPhan = new LopHocPhanDTO
                {
                    MaLopHocPhan = inputResponse(x.lhp.MaLopHocPhan),
                    TenLopHocPhan = inputResponse(x.lhp.TenLopHocPhan),
                    TrangThai = inputResponse(x.lhp.TrangThai.ToString().ToLowerInvariant())
                }
            }).ToList();

            return new PagedResult<AttendanceListItem>
            {
                Page = inputResponse(page.ToString()),
                PageSize = inputResponse(size.ToString()),
                TotalRecords = inputResponse(total.ToString()),
                TotalPages = inputResponse(((int)Math.Ceiling(total / (double)size)).ToString()),
                Items = items
            };
        }

        public async Task<AttendanceStatusListItem> CreateStatusAsync(CreateAttendanceStatusRequest req)
        {
            var code = (req.CodeTrangThai ?? "").Trim().ToUpperInvariant();
            var name = (req.TenTrangThai ?? "").Trim();
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
                throw new ApiException(ApiErrorCode.ValidationError, "CodeTrangThai và TenTrangThai là bắt buộc.");

            if (await _repo.StatusCodeExistsAsync(code))
                throw new ApiException(ApiErrorCode.Conflict, "CodeTrangThai đã tồn tại.");

            var entity = new TrangThaiDiemDanh
            {
                CodeTrangThai = code,
                TenTrangThai = name
            };
            var saved = await _repo.CreateStatusAsync(entity);

            return new AttendanceStatusListItem
            {
                TrangThaiDiemDanh = new TrangThaiDiemDanhDTO
                {
                    MaTrangThai = inputResponse(saved.MaTrangThai.ToString()),
                    CodeTrangThai = inputResponse(saved.CodeTrangThai),
                    TenTrangThai = inputResponse(saved.TenTrangThai)
                }
            };
        }

        public async Task<AttendanceStatusListItem> UpdateStatusAsync(UpdateAttendanceStatusRequest req)
        {
            if (req.MaTrangThai <= 0)
                throw new ApiException(ApiErrorCode.ValidationError, "MaTrangThai không hợp lệ.");

            var code = (req.CodeTrangThai ?? "").Trim().ToUpperInvariant();
            var name = (req.TenTrangThai ?? "").Trim();
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
                throw new ApiException(ApiErrorCode.ValidationError, "CodeTrangThai và TenTrangThai là bắt buộc.");

            var entity = await _repo.GetStatusByIdAsync(req.MaTrangThai)
                ?? throw new ApiException(ApiErrorCode.NotFound, "Không tìm thấy trạng thái.");

            if (await _repo.StatusCodeExistsAsync(code, excludeId: req.MaTrangThai))
                throw new ApiException(ApiErrorCode.Conflict, "CodeTrangThai đã tồn tại.");

            entity.CodeTrangThai = code;
            entity.TenTrangThai = name;
            await _repo.UpdateStatusAsync(entity);

            return new AttendanceStatusListItem
            {
                TrangThaiDiemDanh = new TrangThaiDiemDanhDTO
                {
                    MaTrangThai = inputResponse(entity.MaTrangThai.ToString()),
                    CodeTrangThai = inputResponse(entity.CodeTrangThai),
                    TenTrangThai = inputResponse(entity.TenTrangThai)
                }
            };
        }

        public async Task<bool> DeleteStatusAsync(int maTrangThai)
        {
            if (maTrangThai <= 0)
                throw new ApiException(ApiErrorCode.ValidationError, "MaTrangThai không hợp lệ.");

            if (await _repo.IsStatusInUseAsync(maTrangThai))
                throw new ApiException(ApiErrorCode.Conflict, "Trạng thái đang được sử dụng, không thể xoá.");

            var ok = await _repo.DeleteStatusAsync(maTrangThai);
            if (!ok) throw new ApiException(ApiErrorCode.NotFound, "Không tìm thấy trạng thái.");
            return true;
        }

        public async Task<CreateAttendanceResponse> CreateAttendanceAsync(CreateAttendanceRequest req, string? currentUsername)
        {
            if (string.IsNullOrWhiteSpace(req.MaSinhVien))
                throw new ApiException(ApiErrorCode.ValidationError, "MaSinhVien là bắt buộc.");

            var buoi = await _repo.GetBuoiByIdAsync(req.MaBuoi)
                ?? throw new ApiException(ApiErrorCode.NotFound, "Không tìm thấy buổi học.");

            if (string.IsNullOrWhiteSpace(buoi.MaLopHocPhan))
                throw new ApiException(ApiErrorCode.BadRequest, "Buổi học chưa có mã lớp học phần.");

            var thamGia = await _repo.GetThamGiaLopAsync(buoi.MaLopHocPhan, req.MaSinhVien!);
            if (thamGia == null)
                throw new ApiException(ApiErrorCode.Forbidden, "Sinh viên không thuộc lớp học phần này.");
            if (thamGia.TrangThai == false)
                throw new ApiException(ApiErrorCode.Forbidden, "Sinh viên đã ngừng tham gia lớp học phần này.");

            if (await _repo.AttendanceExistsAsync(req.MaBuoi, req.MaSinhVien))
                throw new ApiException(ApiErrorCode.Conflict, "Sinh viên đã điểm danh buổi này.");

            int statusId = await _repo.GetStatusByIdAsync(req.MaTrangThai ?? 0) is null
                ? throw new ApiException(ApiErrorCode.ValidationError, "MaTrangThai không hợp lệ.")
                : req.MaTrangThai!.Value;

            // Ghi DB = giờ Việt Nam
            var nowLocal = TimeHelper.UtcToVietnam(DateTime.UtcNow);
            var entity = new DiemDanh
            {
                MaBuoi = req.MaBuoi,
                MaSinhVien = req.MaSinhVien!,
                MaTrangThai = statusId,
                LyDo = req.LyDo,
                TrangThai = req.TrangThai ?? false,
                ThoiGianQuet = nowLocal
            };

            var saved = await _repo.CreateAttendanceAsync(entity);

            if (!string.IsNullOrWhiteSpace(currentUsername))
            {
                var nd = await _repo.GetNguoiDungByUsernameAsync(currentUsername);
                await _repo.LogHistoryAsync(nd?.MaNguoiDung,
                    $"CREATE_ATTENDANCE|MaDiemDanh:{saved.MaDiemDanh}|Buoi:{req.MaBuoi}|SV:{req.MaSinhVien}|Status:{statusId}");
            }

            var t = await _repo.GetStatusByIdAsync(saved.MaTrangThai);

            // (tuỳ chọn) lấy vài thông tin SV để trả về, nếu không có vẫn map từ req
            var sv = await _repo.GetSinhVienByMaNguoiDungAsync(
                (await _repo.GetNguoiDungByUsernameAsync(currentUsername ?? ""))?.MaNguoiDung ?? 0
            );

            return new CreateAttendanceResponse
            {
                DiemDanh = new DiemDanhDTO
                {
                    MaDiemDanh = inputResponse(saved.MaDiemDanh.ToString()),
                    ThoiGianQuet = inputResponse(TimeHelper.FormatDateTime(saved.ThoiGianQuet)),
                    TrangThai = inputResponse(saved.TrangThai.ToString().ToLowerInvariant()),
                    LyDo = saved.LyDo == null ? "null" : inputResponse(saved.LyDo)
                },
                TrangThaiDiemDanh = t == null ? null : new TrangThaiDiemDanhDTO
                {
                    MaTrangThai = inputResponse(t.MaTrangThai.ToString()),
                    TenTrangThai = inputResponse(t.TenTrangThai),
                    CodeTrangThai = inputResponse(t.CodeTrangThai)
                },
                SinhVien = new SinhVienDTO
                {
                    MaSinhVien = inputResponse(req.MaSinhVien),
                    NamNhapHoc = sv == null ? "null" : inputResponse(sv.NamNhapHoc.ToString())
                },
                BuoiHoc = new BuoiHocDTO
                {
                    MaBuoi = inputResponse(buoi.MaBuoi.ToString()),
                    NgayHoc = inputResponse(buoi.NgayHoc.ToString("dd-MM-yyyy")),
                    TietBatDau = inputResponse(buoi.TietBatDau.ToString()),
                    SoTiet = inputResponse(buoi.SoTiet.ToString())
                },
                LopHocPhan = new LopHocPhanDTO
                {
                    MaLopHocPhan = inputResponse(buoi.MaLopHocPhan),
                    TenLopHocPhan = inputResponse((await _repo.GetLopHocPhanByIdAsync(buoi.MaLopHocPhan))?.TenLopHocPhan ?? "null"),
                }
            };
        }

        public async Task<UpdateAttendanceResponse> UpdateAttendanceAsync(UpdateAttendanceRequest req, string? currentUsername)
        {
            if (req.MaDiemDanh <= 0)
                throw new ApiException(ApiErrorCode.ValidationError, "MaDiemDanh không hợp lệ.");

            var entity = await _repo.GetAttendanceByIdAsync(req.MaDiemDanh)
                ?? throw new ApiException(ApiErrorCode.NotFound, "Không tìm thấy bản ghi điểm danh.");

            if (req.MaTrangThai.HasValue)
            {
                entity.MaTrangThai = req.MaTrangThai.Value;
            }

            if (req.LyDo != null)
                entity.LyDo = req.LyDo;

            if (req.TrangThai.HasValue)
                entity.TrangThai = req.TrangThai.Value;

            await _repo.UpdateAttendanceAsync(entity);

            var status = await _repo.GetStatusByIdAsync(entity.MaTrangThai);

            if (!string.IsNullOrWhiteSpace(currentUsername))
            {
                var nd = await _repo.GetNguoiDungByUsernameAsync(currentUsername);
                await _repo.LogHistoryAsync(nd?.MaNguoiDung, $"UPDATE_ATTENDANCE|MaDiemDanh:{entity.MaDiemDanh}|Code:{req.MaTrangThai}|TrangThai:{req.TrangThai}|LyDo:{req.LyDo ?? ""}");
            }

            var buoi = await _repo.GetBuoiByIdAsync(entity.MaBuoi);
            var lhp = buoi == null ? null : await _repo.GetLopHocPhanByIdAsync(buoi.MaLopHocPhan);
            var sv = await _repo.GetSinhVienByMaNguoiDungAsync(
                (await _repo.GetNguoiDungByUsernameAsync(currentUsername ?? ""))?.MaNguoiDung ?? 0);

            return new UpdateAttendanceResponse
            {
                DiemDanh = new DiemDanhDTO
                {
                    MaDiemDanh = inputResponse(entity.MaDiemDanh.ToString()),
                    ThoiGianQuet = inputResponse(TimeHelper.FormatDateTime(entity.ThoiGianQuet)),
                    TrangThai = inputResponse(entity.TrangThai.ToString().ToLowerInvariant()),
                    LyDo = entity.LyDo == null ? "null" : inputResponse(entity.LyDo)
                },
                TrangThaiDiemDanh = status == null ? null : new TrangThaiDiemDanhDTO
                {
                    MaTrangThai = inputResponse(status.MaTrangThai.ToString()),
                    TenTrangThai = inputResponse(status.TenTrangThai),
                    CodeTrangThai = inputResponse(status.CodeTrangThai)
                },
                SinhVien = sv == null ? new SinhVienDTO { MaSinhVien = inputResponse(entity.MaSinhVien), NamNhapHoc = "null" }
                                      : new SinhVienDTO { MaSinhVien = inputResponse(entity.MaSinhVien), NamNhapHoc = inputResponse(sv.NamNhapHoc.ToString()) },
                BuoiHoc = buoi == null ? null : new BuoiHocDTO
                {
                    MaBuoi = inputResponse(buoi.MaBuoi.ToString()),
                    NgayHoc = inputResponse(buoi.NgayHoc.ToString("dd-MM-yyyy")),
                    TietBatDau = inputResponse(buoi.TietBatDau.ToString()),
                    SoTiet = inputResponse(buoi.SoTiet.ToString())
                },
                LopHocPhan = lhp == null ? null : new LopHocPhanDTO
                {
                    MaLopHocPhan = inputResponse(lhp.MaLopHocPhan),
                    TenLopHocPhan = inputResponse(lhp.TenLopHocPhan),
                    TrangThai = inputResponse(lhp.TrangThai.ToString().ToLowerInvariant())
                }
            };
        }
        public async Task<List<AttendanceFacultyRatioItem>> GetFacultyAttendanceRatioAsync(int? maHocKy)
        {
            var rows = await _repo.GetFacultyAttendanceSummaryAsync(maHocKy);

            var result = new List<AttendanceFacultyRatioItem>();
            foreach (var (khoa, total, present, absent) in rows)
            {
                var totalDouble = (double)total;
                var tyLeVang = total == 0 ? 0.0 : (absent * 100.0 / totalDouble);
                var tyLeCoMat = total == 0 ? 0.0 : (present * 100.0 / totalDouble);

                result.Add(new AttendanceFacultyRatioItem
                {
                    Khoa = new KhoaDTO
                    {
                        MaKhoa = inputResponse(khoa.MaKhoa.ToString()),
                        CodeKhoa = inputResponse(khoa.CodeKhoa),
                        TenKhoa = inputResponse(khoa.TenKhoa)
                    },
                    TongBuoi = inputResponse(total.ToString()),
                    TongVang = inputResponse(absent.ToString()),
                    TyLeVang = inputResponse(tyLeVang.ToString("0.##", CultureInfo.InvariantCulture)),
                    TongCoMat = inputResponse(present.ToString()),
                    TyLeCoMat = inputResponse(tyLeCoMat.ToString("0.##", CultureInfo.InvariantCulture))
                });
            }

            return result;
        }
        public async Task<List<AttendanceLopHocPhanRatioItem>> GetTeacherAttendanceRatioAsync(string? currentUsername, int? maHocKy)
        {
            if (string.IsNullOrWhiteSpace(currentUsername))
                throw new ApiException(ApiErrorCode.Unauthorized, "Không xác định người dùng.");

            var nd = await _repo.GetNguoiDungByUsernameAsync(currentUsername)
                     ?? throw new ApiException(ApiErrorCode.Unauthorized, "Tài khoản không hợp lệ hoặc đã bị khoá.");

            var gv = await _repo.GetGiangVienByMaNguoiDungAsync(nd.MaNguoiDung)
                     ?? throw new ApiException(ApiErrorCode.Forbidden, "Tài khoản hiện tại không phải giảng viên.");

            var rows = await _repo.GetLopAttendanceSummaryForGiangVienAsync(gv.MaGiangVien, maHocKy);

            var result = new List<AttendanceLopHocPhanRatioItem>();
            foreach (var (lhp, mh, total, present, absent) in rows)
            {
                var totalDouble = (double)total;
                var tyLeVang = total == 0 ? 0.0 : (absent * 100.0 / totalDouble);
                var tyLeCoMat = total == 0 ? 0.0 : (present * 100.0 / totalDouble);

                result.Add(new AttendanceLopHocPhanRatioItem
                {
                    LopHocPhan = new LopHocPhanDTO
                    {
                        MaLopHocPhan = inputResponse(lhp.MaLopHocPhan),
                        TenLopHocPhan = inputResponse(lhp.TenLopHocPhan),
                        TrangThai = inputResponse(lhp.TrangThai.ToString().ToLowerInvariant())
                    },
                    MonHoc = new MonHocDTO
                    {
                        MaMonHoc = inputResponse(mh.MaMonHoc),
                        TenMonHoc = inputResponse(mh.TenMonHoc),
                        SoTinChi = inputResponse(mh.SoTinChi.ToString()),
                        SoTiet = inputResponse(mh.SoTiet.ToString())
                    },
                    TongBuoi = inputResponse(total.ToString()),
                    TongVang = inputResponse(absent.ToString()),
                    TyLeVang = inputResponse(tyLeVang.ToString("0.##", CultureInfo.InvariantCulture)),
                    TongCoMat = inputResponse(present.ToString()),
                    TyLeCoMat = inputResponse(tyLeCoMat.ToString("0.##", CultureInfo.InvariantCulture))
                });
            }

            return result;
        }
        public async Task<List<AttendanceLopHocPhanRatioItem>> GetStudentAttendanceRatioAsync(string? currentUsername, int? maHocKy)
        {
            if (string.IsNullOrWhiteSpace(currentUsername))
                throw new ApiException(ApiErrorCode.Unauthorized, "Không xác định người dùng.");

            var nd = await _repo.GetNguoiDungByUsernameAsync(currentUsername)
                     ?? throw new ApiException(ApiErrorCode.Unauthorized, "Tài khoản không hợp lệ hoặc đã bị khoá.");

            var sv = await _repo.GetSinhVienByMaNguoiDungAsync(nd.MaNguoiDung)
                     ?? throw new ApiException(ApiErrorCode.Forbidden, "Tài khoản hiện tại không phải sinh viên.");

            var rows = await _repo.GetLopAttendanceSummaryForSinhVienAsync(sv.MaSinhVien, maHocKy);

            var result = new List<AttendanceLopHocPhanRatioItem>();
            foreach (var (lhp, mh, total, present, absent) in rows)
            {
                var totalDouble = (double)total;
                var tyLeVang = total == 0 ? 0.0 : (absent * 100.0 / totalDouble);
                var tyLeCoMat = total == 0 ? 0.0 : (present * 100.0 / totalDouble);

                result.Add(new AttendanceLopHocPhanRatioItem
                {
                    LopHocPhan = new LopHocPhanDTO
                    {
                        MaLopHocPhan = inputResponse(lhp.MaLopHocPhan),
                        TenLopHocPhan = inputResponse(lhp.TenLopHocPhan),
                        TrangThai = inputResponse(lhp.TrangThai.ToString().ToLowerInvariant())
                    },
                    MonHoc = new MonHocDTO
                    {
                        MaMonHoc = inputResponse(mh.MaMonHoc),
                        TenMonHoc = inputResponse(mh.TenMonHoc),
                        SoTinChi = inputResponse(mh.SoTinChi.ToString()),
                        SoTiet = inputResponse(mh.SoTiet.ToString())
                    },
                    TongBuoi = inputResponse(total.ToString()),
                    TongVang = inputResponse(absent.ToString()),
                    TyLeVang = inputResponse(tyLeVang.ToString("0.##", CultureInfo.InvariantCulture)),
                    TongCoMat = inputResponse(present.ToString()),
                    TyLeCoMat = inputResponse(tyLeCoMat.ToString("0.##", CultureInfo.InvariantCulture))
                });
            }

            return result;
        }
        private string GetQrSecret()
        {
            var secret = _cfg["QRSettings:QrSecretKey"];
            if (string.IsNullOrWhiteSpace(secret))
                throw new Exception("Chưa cấu hình QRSettings:QrSecretKey.");
            return secret!;
        }
    }
}
