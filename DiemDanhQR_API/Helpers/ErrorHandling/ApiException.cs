// File: Helpers/ErrorHandling/ApiException.cs

namespace DiemDanhQR_API.Helpers
{
    public class ApiException : Exception
    {
        public ApiErrorCode ErrorCode { get; }

        public ApiException(ApiErrorCode code, string? message = null)
            : base(message ?? ApiErrorMessages.GetMessage(code))
        {
            ErrorCode = code;
        }
    }

    public static class ApiExceptionHelper
    {
        public static void Throw(ApiErrorCode code, string? message = null)
        {
            throw new ApiException(code, message);
        }
    }
}
