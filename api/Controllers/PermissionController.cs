// // File: Controllers/PermissionController.cs
// // Bảng PhanQuyen + ChucNang + NhomChucNang
// using DiemDanhQR_API.DTOs.Requests;
// using DiemDanhQR_API.DTOs.Responses;
// using DiemDanhQR_API.Helpers;
// using DiemDanhQR_API.Services.Interfaces;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;

// namespace DiemDanhQR_API.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class PermissionController : ControllerBase
//     {
//         private readonly IPermissionService _svc;
//         public PermissionController(IPermissionService svc) => _svc = svc;

//         // GET: /api/permission/list
//         [HttpGet("list")]
//         [Authorize] // tuỳ nhu cầu có thể siết [Authorize(Roles="ADMIN")]
//         public async Task<ActionResult<ApiResponse<PagedResult<PermissionListItem>>>> GetList([FromQuery] PermissionListRequest req)
//         {
//             var data = await _svc.GetListAsync(req);

//             return Ok(new ApiResponse<PagedResult<PermissionListItem>>
//             {
//                 Status = 200,
//                 Message = "Lấy danh sách phân quyền thành công.",
//                 Data = data
//             });
//         }

//         // GET: /api/permission/functions
//         [HttpGet("functions")]
//         [Authorize] // có thể siết Roles="ADMIN" nếu cần
//         public async Task<ActionResult<ApiResponse<PagedResult<FunctionListItem>>>> GetFunctions([FromQuery] FunctionListRequest req)
//         {
//             var data = await _svc.GetFunctionListAsync(req);

//             return Ok(new ApiResponse<PagedResult<FunctionListItem>>
//             {
//                 Status = 200,
//                 Message = "Lấy danh sách chức năng thành công.",
//                 Data = data
//             });
//         }

//         // ===== CRUD Role =====
//         [HttpPost("create-role")]
//         [Authorize(Roles = "ADMIN")]
//         public async Task<ActionResult<ApiResponse<RoleDetailResponse>>> CreateRole([FromForm] CreateRoleRequest req)
//         {
//             var currentUsername = HelperFunctions.GetUserIdFromClaims(User);
//             var data = await _svc.CreateRoleAsync(req, currentUsername);
//             return Ok(new ApiResponse<RoleDetailResponse> { Status = 200, Message = "Tạo quyền thành công.", Data = data });
//         }

//         [HttpPut("update-role")]
//         [Authorize(Roles = "ADMIN")]
//         public async Task<ActionResult<ApiResponse<RoleDetailResponse>>> UpdateRole([FromForm] UpdateRoleRequest req)
//         {
//             var currentUsername = HelperFunctions.GetUserIdFromClaims(User);
//             var data = await _svc.UpdateRoleAsync(req, currentUsername);
//             return Ok(new ApiResponse<RoleDetailResponse> { Status = 200, Message = "Cập nhật quyền thành công.", Data = data });
//         }

//         // DELETE /api/permission/delete-role
//         [HttpDelete("delete-role")]
//         [Authorize(Roles = "ADMIN")]
//         public async Task<ActionResult<ApiResponse<object>>> DeleteRole([FromQuery] int maQuyen)
//         {
//             var currentUsername = HelperFunctions.GetUserIdFromClaims(User);
//             var ok = await _svc.DeleteRoleAsync(maQuyen, currentUsername);
//             return Ok(new ApiResponse<object> { Status = 200, Message = ok ? "Xóa quyền thành công." : "Không thể xóa quyền.", Data = null! });
//         }

//         // ===== CRUD Function =====
//         [HttpPost("create-function")]
//         [Authorize(Roles = "ADMIN")]
//         public async Task<ActionResult<ApiResponse<FunctionDetailResponse>>> CreateFunction([FromForm] CreateFunctionRequest req)
//         {
//             var currentUsername = HelperFunctions.GetUserIdFromClaims(User);
//             var data = await _svc.CreateFunctionAsync(req, currentUsername);
//             return Ok(new ApiResponse<FunctionDetailResponse> { Status = 200, Message = "Tạo chức năng thành công.", Data = data });
//         }

//         [HttpPut("update-function")]
//         [Authorize(Roles = "ADMIN")]
//         public async Task<ActionResult<ApiResponse<FunctionDetailResponse>>> UpdateFunction([FromForm] UpdateFunctionRequest req)
//         {
//             var currentUsername = HelperFunctions.GetUserIdFromClaims(User);
//             var data = await _svc.UpdateFunctionAsync(req, currentUsername);
//             return Ok(new ApiResponse<FunctionDetailResponse> { Status = 200, Message = "Cập nhật chức năng thành công.", Data = data });
//         }

//         // DELETE /api/permission/delete-function
//         [HttpDelete("delete-function")]
//         [Authorize(Roles = "ADMIN")]
//         public async Task<ActionResult<ApiResponse<object>>> DeleteFunction([FromQuery] int maChucNang)
//         {
//             var currentUsername = HelperFunctions.GetUserIdFromClaims(User);
//             var ok = await _svc.DeleteFunctionAsync(maChucNang, currentUsername);
//             return Ok(new ApiResponse<object> { Status = 200, Message = ok ? "Xóa chức năng thành công." : "Không thể xóa chức năng.", Data = null! });
//         }

//         // ===== CRUD Role-Function (by codes) =====
//         [HttpPost("create-role-function")]
//         [Authorize(Roles = "ADMIN")]
//         public async Task<ActionResult<ApiResponse<RoleFunctionDetailResponse>>> CreateRoleFunction([FromForm] CreateRoleFunctionByCodeRequest req)
//         {
//             var currentUsername = HelperFunctions.GetUserIdFromClaims(User);
//             var data = await _svc.CreateRoleFunctionByCodeAsync(req, currentUsername);
//             return Ok(new ApiResponse<RoleFunctionDetailResponse> { Status = 200, Message = "Thêm nhóm chức năng thành công.", Data = data });
//         }

//         [HttpPut("update-role-function")]
//         [Authorize(Roles = "ADMIN")]
//         public async Task<ActionResult<ApiResponse<RoleFunctionDetailResponse>>> UpdateRoleFunction([FromForm] UpdateRoleFunctionByCodeRequest req)
//         {
//             var currentUsername = HelperFunctions.GetUserIdFromClaims(User);
//             var data = await _svc.UpdateRoleFunctionByCodeAsync(req, currentUsername);
//             return Ok(new ApiResponse<RoleFunctionDetailResponse> { Status = 200, Message = "Cập nhật nhóm chức năng thành công.", Data = data });
//         }

//         // DELETE /api/permission/delete-role-function?codeQuyen=...&codeChucNang=...
//         [HttpDelete("delete-role-function")]
//         [Authorize(Roles = "ADMIN")]
//         public async Task<ActionResult<ApiResponse<object>>> DeleteRoleFunction([FromQuery] string codeQuyen, [FromQuery] string codeChucNang)
//         {
//             var currentUsername = HelperFunctions.GetUserIdFromClaims(User);
//             var ok = await _svc.DeleteRoleFunctionByCodeAsync(codeQuyen, codeChucNang, currentUsername);
//             return Ok(new ApiResponse<object> { Status = 200, Message = ok ? "Xóa nhóm chức năng thành công." : "Không thể xóa nhóm chức năng.", Data = null! });
//         }
//     }
// }
