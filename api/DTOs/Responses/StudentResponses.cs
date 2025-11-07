// File: DTOs/Responses/StudentResponses.cs
namespace api.DTOs.Responses
{
    public class CreateStudentResponse
    {
        public NguoiDungDTO? NguoiDung { get; set; }
        public SinhVienDTO? SinhVien { get; set; }
        public PhanQuyenDTO? PhanQuyen { get; set; }

    }
    public class StudentInfoResponse
    {
        public NguoiDungDTO? NguoiDung { get; set; }
        public SinhVienDTO? SinhVien { get; set; }
        public PhanQuyenDTO? PhanQuyen { get; set; }
    }
    public class StudentListItemResponse
    {
        public NguoiDungDTO? NguoiDung { get; set; }
        public SinhVienDTO? SinhVien { get; set; }
    }

    public class UpdateStudentResponse
    {
        public NguoiDungDTO? NguoiDung { get; set; }
        public SinhVienDTO? SinhVien { get; set; }
        public PhanQuyenDTO? PhanQuyen { get; set; }
    }

    public class AddStudentToCourseResponse
    {
        public SinhVienDTO? SinhVien { get; set; }
        public LopHocPhanDTO? LopHocPhan { get; set; }
        public ThamGiaLopDTO? ThamGiaLop { get; set; }
    }
    public class RemoveStudentFromCourseResponse
    {
        public SinhVienDTO? SinhVien { get; set; }
        public LopHocPhanDTO? LopHocPhan { get; set; }
        public ThamGiaLopDTO? ThamGiaLop { get; set; }
    }
}
