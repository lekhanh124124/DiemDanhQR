// File: DTOs/Responses/LecturerResponses.cs

namespace api.DTOs.Responses
{
    public class CreateLecturerResponse
    {
        public NguoiDungDTO? NguoiDung { get; set; }
        public GiangVienDTO? GiangVien { get; set; }
        public PhanQuyenDTO? PhanQuyen { get; set; }
    }

    public class LecturerInfoResponse
    {
        public NguoiDungDTO? NguoiDung { get; set; }
        public GiangVienDTO? GiangVien { get; set; }
        public PhanQuyenDTO? PhanQuyen { get; set; }
    }
    public class LecturerListItemResponse
    {
        public NguoiDungDTO? NguoiDung { get; set; }
        public GiangVienDTO? GiangVien { get; set; }

    }

    public class UpdateLecturerResponse
    {
        public NguoiDungDTO? NguoiDung { get; set; }
        public GiangVienDTO? GiangVien { get; set; }
        public PhanQuyenDTO? PhanQuyen { get; set; }
    }
}
