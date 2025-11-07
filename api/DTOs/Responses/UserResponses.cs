// File: DTOs/Responses/UserResponses.cs

namespace api.DTOs.Responses
{
    public class CreateUserResponse
    {
        public required NguoiDungDTO NguoiDung { get; set; }
        public required PhanQuyenDTO PhanQuyen { get; set; }
    }

    public class UserActivityItem
    {
        public required NguoiDungDTO NguoiDung { get; set; }
        public required LichSuHoatDongDTO LichSuHoatDong { get; set; }
    }

    public class UpdateUserProfileResponse
    {
        public required NguoiDungDTO NguoiDung { get; set; }
    }
}