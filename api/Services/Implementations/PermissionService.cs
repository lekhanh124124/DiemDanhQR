using api.DTOs;
using api.ErrorHandling;
using api.Models;
using api.Repositories.Interfaces;
using api.Services.Interfaces;

namespace api.Services.Implementations
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _repo;
        public PermissionService(IPermissionRepository repo) => _repo = repo;
        private string inputResponse(string input) => input ?? "null";
        public async Task<PagedResult<PermissionListItem>> GetListAsync(PermissionListRequest request)
        {
            var page = request.Page <= 0 ? 1 : request.Page!.Value;
            var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize!.Value, 200);

            var sortBy = request.SortBy ?? "MaQuyen";
            var sortDir = (request.SortDir ?? "ASC").Trim().ToUpperInvariant();
            var desc = sortDir == "DESC";

            var (items, total) = await _repo.SearchAsync(
                request.MaQuyen,
                request.CodeQuyen,
                request.TenQuyen,
                request.MoTa,
                request.MaChucNang,
                sortBy,
                desc,
                page,
                pageSize
            );

            var list = new List<PermissionListItem>();
            foreach (var x in items)
            {
                NhomChucNangDTO? nhom = null;
                if (request.MaChucNang.HasValue)
                {
                    var map = await _repo.GetRoleFunctionAsync(x.MaQuyen, request.MaChucNang.Value);
                    if (map != null)
                    {
                        nhom = new NhomChucNangDTO
                        {
                            TrangThai = inputResponse(map.TrangThai.ToString().ToLowerInvariant())
                        };
                    }
                }

                list.Add(new PermissionListItem
                {
                    PhanQuyen = new PhanQuyenDTO
                    {
                        MaQuyen = inputResponse(x.MaQuyen.ToString()),
                        CodeQuyen = inputResponse(x.CodeQuyen),
                        TenQuyen = inputResponse(x.TenQuyen),
                        MoTa = inputResponse(x.MoTa)
                    },
                    NhomChucNang = nhom
                });
            }

            return new PagedResult<PermissionListItem>
            {
                Page = inputResponse(page.ToString()),
                PageSize = inputResponse(pageSize.ToString()),
                TotalRecords = inputResponse(total.ToString()),
                TotalPages = inputResponse(((int)Math.Ceiling(total / (double)pageSize)).ToString()),
                Items = list
            };
        }

        public async Task<PagedResult<FunctionListItem>> GetFunctionListAsync(FunctionListRequest request)
        {
            var page = request.Page <= 0 ? 1 : request.Page!.Value;
            var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize!.Value, 200);
            var sortBy = request.SortBy ?? "MaChucNang";
            var desc = string.Equals((request.SortDir ?? "ASC").Trim(), "DESC", StringComparison.OrdinalIgnoreCase);

            var (items, total) = await _repo.SearchFunctionsAsync(
                request.MaChucNang,
                request.CodeChucNang,
                request.TenChucNang,
                request.MoTa,
                request.MaQuyen,
                request.ParentChucNangId,
                sortBy,
                desc,
                page,
                pageSize
            );

            var list = new List<FunctionListItem>();
            foreach (var x in items)
            {
                NhomChucNangDTO? nhom = null;
                if (request.MaQuyen.HasValue)
                {
                    var map = await _repo.GetRoleFunctionAsync(request.MaQuyen.Value, x.MaChucNang);
                    if (map != null)
                    {
                        nhom = new NhomChucNangDTO
                        {
                            TrangThai = inputResponse(map.TrangThai.ToString().ToLowerInvariant())
                        };
                    }
                }

                list.Add(new FunctionListItem
                {
                    ChucNang = new ChucNangDTO
                    {
                        MaChucNang = inputResponse(x.MaChucNang.ToString()),
                        CodeChucNang = inputResponse(x.CodeChucNang),
                        TenChucNang = inputResponse(x.TenChucNang),
                        MoTa = inputResponse(x.MoTa),
                        ParentChucNangId = inputResponse(x.ParentChucNangId?.ToString() ?? "null"),
                        Url = inputResponse(x.Url),
                        Stt = inputResponse(x.Stt?.ToString() ?? "null"),
                    },
                    NhomChucNang = nhom
                });
            }

            return new PagedResult<FunctionListItem>
            {
                Page = inputResponse(page.ToString()),
                PageSize = inputResponse(pageSize.ToString()),
                TotalRecords = inputResponse(total.ToString()),
                TotalPages = inputResponse(((int)Math.Ceiling(total / (double)pageSize)).ToString()),
                Items = list
            };
        }

        public async Task<PagedResult<RoleFunctionListItem>> GetRoleFunctionListAsync(RoleFunctionListRequest request)
        {
            var page = request.Page <= 0 ? 1 : request.Page!.Value;
            var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize!.Value, 200);
            var sortBy = request.SortBy ?? "MaQuyen";
            var desc = string.Equals((request.SortDir ?? "ASC").Trim(), "DESC", StringComparison.OrdinalIgnoreCase);

            var (items, total) = await _repo.SearchRoleFunctionsAsync(
                request.MaQuyen, request.MaChucNang, sortBy, desc, page, pageSize);

            var list = items.Select(x => new RoleFunctionListItem
            {
                PhanQuyen = new PhanQuyenDTO
                {
                    MaQuyen = inputResponse(x.Role.MaQuyen.ToString()),
                    CodeQuyen = inputResponse(x.Role.CodeQuyen),
                    TenQuyen = inputResponse(x.Role.TenQuyen),
                    MoTa = inputResponse(x.Role.MoTa)
                },
                ChucNang = new ChucNangDTO
                {
                    MaChucNang = inputResponse(x.Func.MaChucNang.ToString()),
                    CodeChucNang = inputResponse(x.Func.CodeChucNang),
                    TenChucNang = inputResponse(x.Func.TenChucNang),
                    MoTa = inputResponse(x.Func.MoTa),
                    ParentChucNangId = inputResponse(x.Func.ParentChucNangId?.ToString() ?? "null"),
                    Url = inputResponse(x.Func.Url),
                    Stt = inputResponse(x.Func.Stt?.ToString() ?? "null"),
                },
                NhomChucNang = new NhomChucNangDTO
                {
                    TrangThai = inputResponse(x.Map.TrangThai.ToString().ToLowerInvariant())
                }
            }).ToList();

            return new PagedResult<RoleFunctionListItem>
            {
                Page = inputResponse(page.ToString()),
                PageSize = inputResponse(pageSize.ToString()),
                TotalRecords = inputResponse(total.ToString()),
                TotalPages = inputResponse(((int)Math.Ceiling(total / (double)pageSize)).ToString()),
                Items = list
            };
        }
        public async Task<RoleDetailResponse> CreateRoleAsync(CreateRoleRequest req, string? currentUsername)
        {
            var code = (req.CodeQuyen ?? "").Trim();
            var name = (req.TenQuyen ?? "").Trim();

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "CodeQuyen và TenQuyen là bắt buộc.");

            if (await _repo.RoleCodeExistsAsync(code))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "CodeQuyen đã tồn tại.");

            var entity = new PhanQuyen
            {
                CodeQuyen = code,
                TenQuyen = name,
                MoTa = string.IsNullOrWhiteSpace(req.MoTa) ? null : req.MoTa!.Trim()
            };

            await _repo.AddRoleAsync(entity);

            // Lấy tất cả chức năng và tạo mapping TrangThai=false
            var allFuncs = await _repo.GetAllFunctionsAsync();
            if (allFuncs.Count > 0)
            {
                var mappings = allFuncs.Select(f => new NhomChucNang
                {
                    MaQuyen = entity.MaQuyen,
                    MaChucNang = f.MaChucNang,
                    TrangThai = false
                });
                await _repo.AddRoleFunctionsBulkAsync(mappings);
            }

            await _repo.LogActivityAsync(currentUsername, $"Tạo quyền: {entity.CodeQuyen} - {entity.TenQuyen}");

            return new RoleDetailResponse
            {
                PhanQuyen = new PhanQuyenDTO
                {
                    MaQuyen = inputResponse(entity.MaQuyen.ToString()),
                    CodeQuyen = inputResponse(entity.CodeQuyen),
                    TenQuyen = inputResponse(entity.TenQuyen),
                    MoTa = inputResponse(entity.MoTa)
                }
            };
        }

        public async Task<RoleDetailResponse> UpdateRoleAsync(UpdateRoleRequest req, string? currentUsername)
        {
            var id = req.MaQuyen ?? 0;
            if (id <= 0) ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "MaQuyen không hợp lệ.");

            var role = await _repo.GetRoleByIdAsync(id);
            if (role == null) ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy quyền.");

            if (!string.IsNullOrWhiteSpace(req.CodeQuyen))
            {
                var code = req.CodeQuyen!.Trim();
                if (await _repo.RoleCodeExistsAsync(code, excludeId: id))
                    ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "CodeQuyen đã tồn tại.");
                role!.CodeQuyen = code;
            }
            if (!string.IsNullOrWhiteSpace(req.TenQuyen)) role!.TenQuyen = req.TenQuyen!.Trim();
            if (req.MoTa != null) role!.MoTa = string.IsNullOrWhiteSpace(req.MoTa) ? null : req.MoTa!.Trim();

            await _repo.UpdateRoleAsync(role!);
            await _repo.LogActivityAsync(currentUsername, $"Cập nhật quyền: {role!.MaQuyen} - {role.CodeQuyen}");

            return new RoleDetailResponse
            {
                PhanQuyen = new PhanQuyenDTO
                {
                    MaQuyen = inputResponse(role.MaQuyen.ToString()),
                    CodeQuyen = inputResponse(role.CodeQuyen),
                    TenQuyen = inputResponse(role.TenQuyen),
                    MoTa = inputResponse(role.MoTa)
                }
            };
        }

        public async Task<bool> DeleteRoleAsync(int maQuyen, string? currentUsername)
        {
            if (maQuyen <= 0) ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "MaQuyen không hợp lệ.");

            var role = await _repo.GetRoleByIdAsync(maQuyen);
            if (role == null) ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy quyền.");

            // var users = await _repo.CountUsersByRoleAsync(maQuyen);
            // if (users > 0)
            //     ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Quyền đang được gán cho người dùng, không thể xóa.");

            // var hasMappings = await _repo.AnyRoleFunctionMappingsAsync(maQuyen);
            // if (hasMappings)
            //     ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Quyền đang được gán chức năng, không thể xóa.");

            await _repo.DeleteRoleAsync(role!);
            await _repo.LogActivityAsync(currentUsername, $"Xóa quyền: {role!.CodeQuyen}");

            return true;
        }

        public async Task<FunctionDetailResponse> CreateFunctionAsync(CreateFunctionRequest req, string? currentUsername)
        {
            var code = (req.CodeChucNang ?? "").Trim();
            var name = (req.TenChucNang ?? "").Trim();
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "CodeChucNang và TenChucNang là bắt buộc.");

            if (await _repo.FunctionCodeExistsAsync(code))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "CodeChucNang đã tồn tại.");

            int? parentId = req.ParentChucNangId;
            if (parentId.HasValue)
            {
                var parent = await _repo.GetFunctionByIdAsync(parentId.Value);
                if (parent == null)
                    ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "ParentChucNangId không tồn tại.");
            }

            var entity = new ChucNang
            {
                CodeChucNang = code,
                TenChucNang = name,
                MoTa = string.IsNullOrWhiteSpace(req.MoTa) ? null : req.MoTa!.Trim(),
                ParentChucNangId = parentId,
                Url = string.IsNullOrWhiteSpace(req.Url) ? null : req.Url.Trim(),
                Stt = req.Stt
            };


            await _repo.AddFunctionAsync(entity);

            // Tự thêm mapping TrangThai=false cho tất cả quyền (giữ nguyên logic cũ)
            var allRoles = await _repo.GetAllRolesAsync();
            if (allRoles.Count > 0)
            {
                var mappings = allRoles.Select(r => new NhomChucNang
                {
                    MaQuyen = r.MaQuyen,
                    MaChucNang = entity.MaChucNang,
                    TrangThai = false
                });
                await _repo.AddRoleFunctionsBulkAsync(mappings);
            }

            await _repo.LogActivityAsync(currentUsername, $"Tạo chức năng: {entity.CodeChucNang} - {entity.TenChucNang}");

            return new FunctionDetailResponse
            {
                ChucNang = new ChucNangDTO
                {
                    MaChucNang = inputResponse(entity.MaChucNang.ToString()),
                    CodeChucNang = inputResponse(entity.CodeChucNang),
                    TenChucNang = inputResponse(entity.TenChucNang),
                    MoTa = inputResponse(entity.MoTa),
                    ParentChucNangId = inputResponse(entity.ParentChucNangId?.ToString() ?? "null"),
                    Url = inputResponse(entity.Url),
                    Stt = inputResponse(entity.Stt?.ToString() ?? "null"),
                }
            };
        }

        public async Task<FunctionDetailResponse> UpdateFunctionAsync(UpdateFunctionRequest req, string? currentUsername)
        {
            var id = req.MaChucNang ?? 0;
            if (id <= 0) ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "MaChucNang không hợp lệ.");

            var fn = await _repo.GetFunctionByIdAsync(id)
                     ?? throw ApiExceptionHelper.New(ApiErrorCode.NotFound, "Không tìm thấy chức năng.");

            if (!string.IsNullOrWhiteSpace(req.CodeChucNang))
            {
                var code = req.CodeChucNang!.Trim();
                if (await _repo.FunctionCodeExistsAsync(code, excludeId: id))
                    ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "CodeChucNang đã tồn tại.");
                fn.CodeChucNang = code;
            }
            if (req.Url != null)
                fn.Url = string.IsNullOrWhiteSpace(req.Url) ? null : req.Url.Trim();

            if (req.Stt.HasValue)
                fn.Stt = req.Stt;

            if (!string.IsNullOrWhiteSpace(req.TenChucNang)) fn.TenChucNang = req.TenChucNang!.Trim();
            if (req.MoTa != null) fn.MoTa = string.IsNullOrWhiteSpace(req.MoTa) ? null : req.MoTa!.Trim();

            // ⭐ Update parent
            if (req.ParentChucNangId.HasValue)
            {
                var newParentId = req.ParentChucNangId.Value;

                if (newParentId == 0)
                {
                    fn.ParentChucNangId = null; // đưa về root
                }
                else
                {
                    if (newParentId == id)
                        ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "ParentChucNangId không được trùng chính nó.");

                    var parent = await _repo.GetFunctionByIdAsync(newParentId);
                    if (parent == null)
                        ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "ParentChucNangId không tồn tại.");

                    // (Đơn giản) Không cho phép set parent là chính nó; để tránh vòng, có thể cấm set parent là con trực tiếp của nó
                    // Nếu muốn chặt chẽ hơn, cần hàm duyệt cây để phát hiện chu kỳ.
                    fn.ParentChucNangId = newParentId;
                }
            }
            else if (req.ParentChucNangId != null && !req.ParentChucNangId.HasValue)
            {
                // no-op
            }

            await _repo.UpdateFunctionAsync(fn);
            await _repo.LogActivityAsync(currentUsername, $"Cập nhật chức năng: {fn.MaChucNang} - {fn.CodeChucNang}");

            return new FunctionDetailResponse
            {
                ChucNang = new ChucNangDTO
                {
                    MaChucNang = inputResponse(fn.MaChucNang.ToString()),
                    CodeChucNang = inputResponse(fn.CodeChucNang),
                    TenChucNang = inputResponse(fn.TenChucNang),
                    MoTa = inputResponse(fn.MoTa),
                    ParentChucNangId = inputResponse(fn.ParentChucNangId?.ToString() ?? "null"),
                    Url = inputResponse(fn.Url),
                    Stt = inputResponse(fn.Stt?.ToString() ?? "null"),
                }
            };
        }

        public async Task<bool> DeleteFunctionAsync(int maChucNang, string? currentUsername)
        {
            if (maChucNang <= 0) ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "MaChucNang không hợp lệ.");

            var fn = await _repo.GetFunctionByIdAsync(maChucNang)
                     ?? throw ApiExceptionHelper.New(ApiErrorCode.NotFound, "Không tìm thấy chức năng.");

            // ⭐ NEW: chặn nếu có con
            if (await _repo.AnyFunctionChildrenAsync(maChucNang))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Chức năng đang có chức năng con, không thể xóa.");

            // // Giữ nguyên: chặn nếu đang map với quyền
            // if (await _repo.AnyFunctionRoleMappingsAsync(maChucNang))
            //     ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Chức năng đang được gán cho quyền, không thể xóa.");

            await _repo.DeleteFunctionAsync(fn);
            await _repo.LogActivityAsync(currentUsername, $"Xóa chức năng: {fn.CodeChucNang}");

            return true;
        }

        public async Task<RoleFunctionDetailResponse> CreateRoleFunctionByCodeAsync(CreateRoleFunctionByCodeRequest req, string? currentUsername)
        {
            var maQuyen = req.MaQuyen;
            var maChucNang = req.MaChucNang;
            if (maQuyen == null || maChucNang == null)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "MaQuyen/MaChucNang là bắt buộc.");

            var role = await _repo.GetRoleByIdAsync(maQuyen!.Value);
            if (role == null) ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy mã quyền.");
            var fn = await _repo.GetFunctionByIdAsync(maChucNang!.Value);
            if (fn == null) ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy mã chức năng.");

            if (await _repo.RoleFunctionExistsAsync(role!.MaQuyen, fn!.MaChucNang))
                ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Cặp Quyền–Chức năng đã tồn tại.");

            await _repo.AddRoleFunctionAsync(new NhomChucNang
            {
                MaQuyen = role.MaQuyen,
                MaChucNang = fn.MaChucNang,
                TrangThai = req.TrangThai ?? true
            });

            await _repo.LogActivityAsync(currentUsername, $"Thêm nhóm chức năng: {role.CodeQuyen} - {fn.CodeChucNang}");

            return new RoleFunctionDetailResponse
            {
                PhanQuyen = new PhanQuyenDTO
                {
                    MaQuyen = inputResponse(role.MaQuyen.ToString()),
                    CodeQuyen = inputResponse(role.CodeQuyen),
                    TenQuyen = inputResponse(role.TenQuyen),
                },
                ChucNang = new ChucNangDTO
                {
                    MaChucNang = inputResponse(fn.MaChucNang.ToString()),
                    CodeChucNang = inputResponse(fn.CodeChucNang),
                    TenChucNang = inputResponse(fn.TenChucNang),
                    Url = inputResponse(fn.Url),
                    Stt = inputResponse(fn.Stt?.ToString() ?? "null"),
                },
                NhomChucNang = new NhomChucNangDTO
                {
                    TrangThai = inputResponse(req.TrangThai.ToString().ToLowerInvariant())
                }
            };
        }

        public async Task<RoleFunctionDetailResponse> UpdateRoleFunctionByCodeAsync(UpdateRoleFunctionByCodeRequest req, string? currentUsername)
        {
            var fromRoleId = req.FromMaQuyen ?? 0;
            var fromFnId = req.FromMaChucNang ?? 0;
            if (fromRoleId <= 0 || fromFnId <= 0)
                ApiExceptionHelper.Throw(ApiErrorCode.ValidationError, "FromMaQuyen và FromMaChucNang là bắt buộc.");

            var srcRole = await _repo.GetRoleByIdAsync(fromRoleId)
                ?? throw ApiExceptionHelper.New(ApiErrorCode.NotFound, "Không tìm thấy quyền nguồn.");
            var srcFn = await _repo.GetFunctionByIdAsync(fromFnId)
                ?? throw ApiExceptionHelper.New(ApiErrorCode.NotFound, "Không tìm thấy chức năng nguồn.");
            var srcMap = await _repo.GetRoleFunctionAsync(srcRole.MaQuyen, srcFn.MaChucNang)
                ?? throw ApiExceptionHelper.New(ApiErrorCode.NotFound, "Mapping nguồn không tồn tại.");

            var newRoleId = req.ToMaQuyen ?? fromRoleId;
            var newFnId = req.ToMaChucNang ?? fromFnId;
            var newStatus = req.TrangThai ?? srcMap.TrangThai;

            PhanQuyen finalRole = srcRole;
            ChucNang finalFn = srcFn;
            bool finalStatus = srcMap.TrangThai;

            var pairChanged = newRoleId != fromRoleId || newFnId != fromFnId;
            var statusChanged = newStatus != srcMap.TrangThai;

            if (pairChanged)
            {
                finalRole = newRoleId == fromRoleId ? srcRole :
                    await _repo.GetRoleByIdAsync(newRoleId)
                        ?? throw ApiExceptionHelper.New(ApiErrorCode.NotFound, "Không tìm thấy quyền đích.");
                finalFn = newFnId == fromFnId ? srcFn :
                    await _repo.GetFunctionByIdAsync(newFnId)
                        ?? throw ApiExceptionHelper.New(ApiErrorCode.NotFound, "Không tìm thấy chức năng đích.");

                if (await _repo.RoleFunctionExistsAsync(finalRole.MaQuyen, finalFn.MaChucNang))
                    ApiExceptionHelper.Throw(ApiErrorCode.BadRequest, "Cặp quyền–chức năng đích đã tồn tại.");

                await _repo.DeleteRoleFunctionAsync(srcRole.MaQuyen, srcFn.MaChucNang);
                await _repo.AddRoleFunctionAsync(new NhomChucNang
                {
                    MaQuyen = finalRole.MaQuyen,
                    MaChucNang = finalFn.MaChucNang,
                    TrangThai = newStatus
                });
                finalStatus = newStatus;
                await _repo.LogActivityAsync(currentUsername, $"Cập nhật mapping: ({fromRoleId},{fromFnId}) -> ({newRoleId},{newFnId}), TrangThai={newStatus}");
            }
            else if (statusChanged)
            {
                await _repo.UpdateRoleFunctionStatusAsync(srcRole.MaQuyen, srcFn.MaChucNang, newStatus);
                finalStatus = newStatus;
                await _repo.LogActivityAsync(currentUsername, $"Đổi trạng thái mapping: ({fromRoleId},{fromFnId}) -> TrangThai={newStatus}");
            }

            return new RoleFunctionDetailResponse
            {
                PhanQuyen = new PhanQuyenDTO
                {
                    MaQuyen = inputResponse(finalRole.MaQuyen.ToString()),
                    CodeQuyen = inputResponse(finalRole.CodeQuyen),
                    TenQuyen = inputResponse(finalRole.TenQuyen)
                },
                ChucNang = new ChucNangDTO
                {
                    MaChucNang = inputResponse(finalFn.MaChucNang.ToString()),
                    CodeChucNang = inputResponse(finalFn.CodeChucNang),
                    TenChucNang = inputResponse(finalFn.TenChucNang),
                    Url = inputResponse(finalFn.Url),
                    Stt = inputResponse(finalFn.Stt?.ToString() ?? "null"),
                },
                NhomChucNang = new NhomChucNangDTO
                {
                    TrangThai = inputResponse(finalStatus.ToString().ToLowerInvariant())
                }
            };
        }

        public async Task<bool> DeleteRoleFunctionByCodeAsync(int maQuyen, int maChucNang, string? currentUsername)
        {
            var role = await _repo.GetRoleByIdAsync(maQuyen);
            if (role == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy quyền.");
            var fn = await _repo.GetFunctionByIdAsync(maChucNang);
            if (fn == null)
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Không tìm thấy chức năng.");

            if (!await _repo.RoleFunctionExistsAsync(role!.MaQuyen, fn!.MaChucNang))
                ApiExceptionHelper.Throw(ApiErrorCode.NotFound, "Nhóm chức năng không tồn tại.");

            await _repo.DeleteRoleFunctionAsync(role.MaQuyen, fn.MaChucNang);
            await _repo.LogActivityAsync(currentUsername, $"Xóa nhóm chức năng: {role.CodeQuyen} - {fn.CodeChucNang}");
            return true;
        }
    }
}
