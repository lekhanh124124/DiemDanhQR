// File: Controllers/PermissionController.cs
// Bảng PhanQuyen + ChucNang + NhomChucNang
using api.DTOs;
using api.Helpers;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _svc;
        public PermissionController(IPermissionService svc) => _svc = svc;

        [HttpGet("list")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PagedResult<PermissionListItem>>>> GetList([FromQuery] PermissionListRequest req)
        {
            var data = await _svc.GetListAsync(req);
            return Ok(new ApiResponse<PagedResult<PermissionListItem>> { Status = "200", Message = "Lấy danh sách phân quyền thành công.", Data = data });
        }

        [HttpGet("functions")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PagedResult<FunctionListItem>>>> GetFunctions([FromQuery] FunctionListRequest req)
        {
            var data = await _svc.GetFunctionListAsync(req);
            return Ok(new ApiResponse<PagedResult<FunctionListItem>> { Status = "200", Message = "Lấy danh sách chức năng thành công.", Data = data });
        }

        // ===== CRUD Role =====
        [HttpPost("create-role")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<RoleDetailResponse>>> CreateRole([FromForm] CreateRoleRequest req)
        {
            if (!JwtHelper.IsAdmin(User))
                return Forbid("Chỉ ADMIN mới được phép thực hiện thao tác này.");

            var currentUsername = JwtHelper.GetUsername(User);
            var data = await _svc.CreateRoleAsync(req, currentUsername);
            return Ok(new ApiResponse<RoleDetailResponse> { Status = "200", Message = "Tạo quyền thành công.", Data = data });
        }

        [HttpPut("update-role")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<RoleDetailResponse>>> UpdateRole([FromForm] UpdateRoleRequest req)
        {
            if (!JwtHelper.IsAdmin(User))
                return Forbid("Chỉ ADMIN mới được phép thực hiện thao tác này.");

            var currentUsername = JwtHelper.GetUsername(User);
            var data = await _svc.UpdateRoleAsync(req, currentUsername);
            return Ok(new ApiResponse<RoleDetailResponse> { Status = "200", Message = "Cập nhật quyền thành công.", Data = data });
        }

        [HttpDelete("delete-role")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> DeleteRole([FromQuery] int maQuyen)
        {
            if (!JwtHelper.IsAdmin(User))
                return Forbid("Chỉ ADMIN mới được phép thực hiện thao tác này.");

            var currentUsername = JwtHelper.GetUsername(User);
            var ok = await _svc.DeleteRoleAsync(maQuyen, currentUsername);
            return Ok(new ApiResponse<object> { Status = "200", Message = ok ? "Xóa quyền thành công." : "Không thể xóa quyền.", Data = null! });
        }

        // ===== CRUD Function =====
        [HttpPost("create-function")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<FunctionDetailResponse>>> CreateFunction([FromForm] CreateFunctionRequest req)
        {
            if (!JwtHelper.IsAdmin(User))
                return Forbid("Chỉ ADMIN mới được phép thực hiện thao tác này.");

            var currentUsername = JwtHelper.GetUsername(User);
            var data = await _svc.CreateFunctionAsync(req, currentUsername);
            return Ok(new ApiResponse<FunctionDetailResponse> { Status = "200", Message = "Tạo chức năng thành công.", Data = data });
        }

        [HttpPut("update-function")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<FunctionDetailResponse>>> UpdateFunction([FromForm] UpdateFunctionRequest req)
        {
            if (!JwtHelper.IsAdmin(User))
                return Forbid("Chỉ ADMIN mới được phép thực hiện thao tác này.");

            var currentUsername = JwtHelper.GetUsername(User);
            var data = await _svc.UpdateFunctionAsync(req, currentUsername);
            return Ok(new ApiResponse<FunctionDetailResponse> { Status = "200", Message = "Cập nhật chức năng thành công.", Data = data });
        }

        [HttpDelete("delete-function")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> DeleteFunction([FromQuery] int maChucNang)
        {
            if (!JwtHelper.IsAdmin(User))
                return Forbid("Chỉ ADMIN mới được phép thực hiện thao tác này.");

            var currentUsername = JwtHelper.GetUsername(User);
            var ok = await _svc.DeleteFunctionAsync(maChucNang, currentUsername);
            return Ok(new ApiResponse<object> { Status = "200", Message = ok ? "Xóa chức năng thành công." : "Không thể xóa chức năng.", Data = null! });
        }

        // ===== CRUD Role-Function (by codes) =====
        [HttpPost("create-role-function")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<RoleFunctionDetailResponse>>> CreateRoleFunction([FromForm] CreateRoleFunctionByCodeRequest req)
        {
            if (!JwtHelper.IsAdmin(User))
                return Forbid("Chỉ ADMIN mới được phép thực hiện thao tác này.");

            var currentUsername = JwtHelper.GetUsername(User);
            var data = await _svc.CreateRoleFunctionByCodeAsync(req, currentUsername);
            return Ok(new ApiResponse<RoleFunctionDetailResponse> { Status = "200", Message = "Thêm nhóm chức năng thành công.", Data = data });
        }

        [HttpPut("update-role-function")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<RoleFunctionDetailResponse>>> UpdateRoleFunction([FromForm] UpdateRoleFunctionByCodeRequest req)
        {
            if (!JwtHelper.IsAdmin(User))
                return Forbid("Chỉ ADMIN mới được phép thực hiện thao tác này.");

            var currentUsername = JwtHelper.GetUsername(User);
            var data = await _svc.UpdateRoleFunctionByCodeAsync(req, currentUsername);
            return Ok(new ApiResponse<RoleFunctionDetailResponse> { Status = "200", Message = "Cập nhật nhóm chức năng thành công.", Data = data });
        }

        [HttpDelete("delete-role-function")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> DeleteRoleFunction([FromQuery] int maQuyen, [FromQuery] int maChucNang)
        {
            if (!JwtHelper.IsAdmin(User))
                return Forbid("Chỉ ADMIN mới được phép thực hiện thao tác này.");

            var currentUsername = JwtHelper.GetUsername(User);
            var ok = await _svc.DeleteRoleFunctionByCodeAsync(maQuyen, maChucNang, currentUsername);
            return Ok(new ApiResponse<object> { Status = "200", Message = ok ? "Xóa nhóm chức năng thành công." : "Không thể xóa nhóm chức năng.", Data = null! });
        }

        [HttpGet("role-functions")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PagedResult<RoleFunctionListItem>>>> GetRoleFunctions([FromQuery] RoleFunctionListRequest req)
        {
            var data = await _svc.GetRoleFunctionListAsync(req);
            return Ok(new ApiResponse<PagedResult<RoleFunctionListItem>>
            {
                Status = "200",
                Message = "Lấy danh sách nhóm chức năng thành công.",
                Data = data
            });
        }
    }
}
