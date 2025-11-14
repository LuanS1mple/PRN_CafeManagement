using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text;

namespace CafeManagent.ErrorHandler
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;
        private readonly string _logFilePath;

        public ErrorHandlerMiddleware(RequestDelegate next, IWebHostEnvironment env, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _env = env;
            _logger = logger;
            _logFilePath = Path.Combine(_env.ContentRootPath, "ErrorLog.txt");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // Chạy pipeline tiếp theo
            }
            catch (Exception ex)
            {
                // Ghi log ra file
                WriteToLogFile(ex);

                // Tạo AppException nếu muốn đồng bộ
                AppException appEx = ex as AppException ?? new AppException(ErrorCode.DefaultError, ex);

                // Tạo ViewResult như filter
                var viewResult = new ViewResult
                {
                    ViewName = "Error",
                    ViewData = new ViewDataDictionary(new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
                                                      new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary())
                    {
                        ["ErrorMessage"] = appEx.Message,
                        ["Root"] = appEx.Root ?? ex.Message,
                        ["Time"] = appEx.Time
                    }
                };

                // Thực thi ViewResult
                var actionContext = new ActionContext
                {
                    HttpContext = context,
                    RouteData = context.GetRouteData(),
                    ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
                };

                await viewResult.ExecuteResultAsync(actionContext);
            }
        }

        private void WriteToLogFile(Exception ex)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("--------------------------------------------------");
                sb.AppendLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Exception Type: {ex.GetType().FullName}");
                sb.AppendLine($"Message: {ex.Message}");
                sb.AppendLine($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    sb.AppendLine($"Inner Exception: {ex.InnerException}");
                }
                sb.AppendLine();

                File.AppendAllText(_logFilePath, sb.ToString());
            }
            catch
            {
                // Không làm gì nếu ghi log thất bại
            }
        }
    }

    // Extension method để dễ đăng ký middleware
    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlerMiddleware>();
        }
    }
}
