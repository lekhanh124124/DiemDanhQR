// File: ErrorHandling/ApiExceptionMiddleware.cs
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using api.DTOs;

namespace api.ErrorHandling
{
    public class ApiExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Bọc response để có thể đọc/ghi lại sau khi MVC xử lý
            var originalBody = context.Response.Body;
            await using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            try
            {
                await _next(context);
            }
            catch (ApiException ex)
            {
                await WriteApiErrorAsync(context, ex.ErrorCode, ex.Message, originalBody);
                return;
            }
            catch (ValidationException ex)
            {
                await WriteApiErrorAsync(context, ApiErrorCode.ValidationError, ex.Message, originalBody);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                await WriteApiErrorAsync(context, ApiErrorCode.Unauthorized, ex.Message, originalBody);
                return;
            }
            catch (Exception ex)
            {
                await WriteApiErrorAsync(context, ApiErrorCode.InternalServerError, ex.Message, originalBody);
                return;
            }

            // Không có exception: kiểm tra status code trả về (ví dụ 400 ModelState)
            memStream.Seek(0, SeekOrigin.Begin);
            var raw = await new StreamReader(memStream).ReadToEndAsync();

            // Nếu success (2xx) thì pass-through
            if (context.Response.StatusCode < 400)
            {
                memStream.Seek(0, SeekOrigin.Begin);
                await memStream.CopyToAsync(originalBody);
                context.Response.Body = originalBody;
                return;
            }

            // 4xx/5xx: chuẩn hoá sang ApiResponse
            var code = ApiErrorMessages.FromHttpStatus(context.Response.StatusCode);

            // Cố gắng rút message từ ProblemDetails/ValidationProblemDetails
            string? message = TryExtractProblemMessage(raw);
            if (string.IsNullOrWhiteSpace(message))
            {
                message = code == ApiErrorCode.BadRequest
                    ? ApiErrorMessages.GetMessage(ApiErrorCode.ValidationError)
                    : ApiErrorMessages.GetMessage(code);
            }

            await WriteApiErrorAsync(context, code, message, originalBody);
        }

        // ==== Helpers ====

        private static string? TryExtractProblemMessage(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return null;

            try
            {
                var node = JsonNode.Parse(body);
                if (node == null) return null;

                var title = node["title"]?.GetValue<string>();
                var detail = node["detail"]?.GetValue<string>();

                // Ưu tiên detail rồi đến title
                var baseMsg = !string.IsNullOrWhiteSpace(detail) ? detail : title;

                // Thêm 1-2 lỗi đầu tiên từ "errors"
                var errorsNode = node["errors"] as JsonObject;
                if (errorsNode != null)
                {
                    var firstError = errorsNode
                        .Select(kvp => kvp.Value as JsonArray)
                        .Where(arr => arr != null && arr.Count > 0)
                        .Select(arr => arr![0]?.ToString())
                        .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));

                    if (!string.IsNullOrWhiteSpace(firstError))
                        return string.IsNullOrWhiteSpace(baseMsg) ? firstError : $"{baseMsg} - {firstError}";
                }

                return baseMsg;
            }
            catch
            {
                return null;
            }
        }

        private static async Task WriteApiErrorAsync(
            HttpContext context,
            ApiErrorCode code,
            string? message,
            Stream? originalBodyOverride = null)
        {
            // Map ApiErrorCode -> HTTP status hợp lệ
            var statusCode = code switch
            {
                ApiErrorCode.ValidationError => StatusCodes.Status400BadRequest,
                ApiErrorCode.BadRequest => StatusCodes.Status400BadRequest,
                ApiErrorCode.Unauthorized => StatusCodes.Status401Unauthorized,
                ApiErrorCode.Forbidden => StatusCodes.Status403Forbidden,
                ApiErrorCode.NotFound => StatusCodes.Status404NotFound,
                ApiErrorCode.MethodNotAllowed => StatusCodes.Status405MethodNotAllowed,
                ApiErrorCode.Conflict => StatusCodes.Status409Conflict,                
                ApiErrorCode.InternalServerError => StatusCodes.Status500InternalServerError,
                ApiErrorCode.InternalError => StatusCodes.Status500InternalServerError, // domain error -> 500
                _ => StatusCodes.Status500InternalServerError
            };

            var payload = new ApiResponse<string>
            {
                Status = ((int)code).ToString(),
                Message = string.IsNullOrWhiteSpace(message)
                    ? ApiErrorMessages.GetMessage(code)
                    : message!,
                Data = null!
            };

            var json = JsonSerializer.Serialize(payload);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            if (originalBodyOverride != null)
            {
                context.Response.Body = originalBodyOverride;
            }

            await context.Response.WriteAsync(json);
        }
    }
}
