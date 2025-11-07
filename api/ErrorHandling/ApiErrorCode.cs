// File: ErrorHandling/ApiErrorCode.cs
namespace api.ErrorHandling
{
    public enum ApiErrorCode
    {
        Unknown = 0,
        NotFound = 404,
        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        MethodNotAllowed = 405,
        InternalServerError = 500,
        ValidationError = 1001,
        InternalError = 1002,
        Conflict = 1003,

        // Thêm các mã lỗi khác nếu cần
    }

    public static class ApiErrorMessages
    {
        public static string GetMessage(ApiErrorCode code) => code switch
        {
            ApiErrorCode.NotFound => "Không tìm thấy tài nguyên.",
            ApiErrorCode.BadRequest => "Yêu cầu không hợp lệ.",
            ApiErrorCode.Unauthorized => "Chưa xác thực.",
            ApiErrorCode.Forbidden => "Không có quyền truy cập.",
            ApiErrorCode.MethodNotAllowed => "Phương thức HTTP không được hỗ trợ cho endpoint này.",
            ApiErrorCode.InternalServerError => "Lỗi máy chủ.",
            ApiErrorCode.ValidationError => "Dữ liệu không hợp lệ.",
            ApiErrorCode.InternalError => "Lỗi nội bộ.",
            ApiErrorCode.Conflict => "Xung đột dữ liệu.",
            _ => "Lỗi không xác định."
        };

        // Chuyển đổi từ mã trạng thái HTTP sang ApiErrorCode (phục vụ cho việc bắt lỗi controller trước khi vào service)
        public static ApiErrorCode FromHttpStatus(int statusCode) => statusCode switch
        {
            400 => ApiErrorCode.BadRequest,
            401 => ApiErrorCode.Unauthorized,
            403 => ApiErrorCode.Forbidden,
            404 => ApiErrorCode.NotFound,
            405 => ApiErrorCode.MethodNotAllowed,
            >= 500 => ApiErrorCode.InternalServerError,
            _ => ApiErrorCode.Unknown
        };
    }
}
