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
        private readonly string _logFilePath;

        public ErrorHandlerMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            _next = next;
            _env = env;
            _logFilePath = Path.Combine(_env.ContentRootPath, "ErrorLog.txt");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); 
            }
            catch (Exception ex)
            {
                WriteToLogFile(ex);

                AppException appEx = ex as AppException ?? new AppException(ErrorCode.DefaultError, ex);

                var viewResult = new ViewResult
                {
                    ViewName = "Error",
                    ViewData = new ViewDataDictionary(new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
                                                      new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary())
                    {
                        ["ErrorMessage"] = appEx.ErrorCode.Message,
                        ["Time"] = appEx.Time
                    }
                };

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
            }
        }
    }

    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlerMiddleware>();
        }
    }
}
