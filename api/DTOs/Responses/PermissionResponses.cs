// File: DTOs/Responses/PermissionResponses.cs

using System.Text.Json.Serialization;

namespace api.DTOs
{
    public class PermissionListItem
    {
        public PhanQuyenDTO? PhanQuyen { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public NhomChucNangDTO? NhomChucNang { get; set; }
    }
    public class FunctionListItem
    {
        public ChucNangDTO? ChucNang { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public NhomChucNangDTO? NhomChucNang { get; set; }
    }
    public class RoleDetailResponse
    {
        public PhanQuyenDTO? PhanQuyen { get; set; }
    }
    public class FunctionDetailResponse
    {
        public ChucNangDTO? ChucNang { get; set; }
    }
    public class RoleFunctionDetailResponse
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PhanQuyenDTO? PhanQuyen { get; set; }
        public ChucNangDTO? ChucNang { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public NhomChucNangDTO? NhomChucNang { get; set; }
    }

    public class RoleFunctionListItem
    {
        public PhanQuyenDTO? PhanQuyen { get; set; }
        public ChucNangDTO? ChucNang { get; set; }
        public NhomChucNangDTO? NhomChucNang { get; set; }
    }
}
