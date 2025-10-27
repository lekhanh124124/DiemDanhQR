// File: Controllers/PermissionController.cs
// Bảng PhanQuyen + ChucNang + NhomChucNang
using DiemDanhQR_API.DTOs.Requests;
using DiemDanhQR_API.DTOs.Responses;
using DiemDanhQR_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiemDanhQR_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _svc;
        public PermissionController(IPermissionService svc) => _svc = svc;

        // GET: /api/permission/list
        [HttpGet("list")]
        [Authorize] // tuỳ nhu cầu có thể siết [Authorize(Roles="ADMIN")]
        public async Task<ActionResult<ApiResponse<PagedResult<PermissionListItem>>>> GetList([FromQuery] PermissionListRequest req)
        {
            var data = await _svc.GetListAsync(req);

            return Ok(new ApiResponse<PagedResult<PermissionListItem>>
            {
                Status = 200,
                Message = "Lấy danh sách phân quyền thành công.",
                Data = data
            });
        }

        // GET: /api/permission/functions
        [HttpGet("functions")]
        [Authorize] // có thể siết Roles="ADMIN" nếu cần
        public async Task<ActionResult<ApiResponse<PagedResult<FunctionListItem>>>> GetFunctions([FromQuery] FunctionListRequest req)
        {
            var data = await _svc.GetFunctionListAsync(req);

            return Ok(new ApiResponse<PagedResult<FunctionListItem>>
            {
                Status = 200,
                Message = "Lấy danh sách chức năng thành công.",
                Data = data
            });
        }
    }
}
