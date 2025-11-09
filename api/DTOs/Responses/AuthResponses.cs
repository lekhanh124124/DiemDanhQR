// File: DTOs/Responses/AuthResponses.cs

namespace api.DTOs
{
    public class LoginResponse
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public required NguoiDungDTO NguoiDung { get; set; }
        public required PhanQuyenDTO PhanQuyen { get; set; }      
        public required IEnumerable<RoleFunctionDetailResponse> NhomChucNang { get; set; }

    }

    public class LogoutResponse
    {
        public required NguoiDungDTO NguoiDung { get; set; }
    }

    public class RefreshAccessTokenResponse
    {
        public required string AccessToken { get; set; }
        public required NguoiDungDTO NguoiDung { get; set; }
    }

    public class ChangePasswordResponse
    {
        public required NguoiDungDTO NguoiDung { get; set; }
        public required string ChangedAt { get; set; }

    }
    public class RefreshPasswordResponse
    {

        public required NguoiDungDTO NguoiDung { get; set; }
        public required string RefreshAt { get; set; }
    }
}
