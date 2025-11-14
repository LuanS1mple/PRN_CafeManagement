using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.IO;
using System.Text;

namespace CafeManagent.ErrorHandler
{
    public class GlobalExceptionHandler : IExceptionFilter
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly string _logFilePath;

        public GlobalExceptionHandler(IWebHostEnvironment env, ILogger<GlobalExceptionHandler> logger)
        {
            _env = env;
            _logger = logger;
            _logFilePath = Path.Combine(_env.ContentRootPath, "ErrorLog.txt");
        }

        public void OnException(ExceptionContext context)
        {
            // Lấy lỗi gốc
            var exception = context.Exception;

            // Nếu là AppException thì dùng thông tin có sẵn
            if (exception is AppException appEx)
            {

                context.Result = new ViewResult
                {
                    ViewName = "Error",
                    ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(
                        new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
                        context.ModelState)
                    {
                        ["ErrorMessage"] = appEx.Message,
                        ["Root"] = appEx.Root,
                        ["Time"] = appEx.Time
                    }
                };
            }
            else // Nếu là Exception thường
            {

                context.Result = new ViewResult
                {
                    ViewName = "Error",
                    ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(
                        new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
                        context.ModelState)
                    {
                        ["ErrorMessage"] = "Hệ thống gặp lỗi, vui lòng thử lại sau.",
                        ["Root"] = exception.Message,
                        ["Time"] = DateTime.Now
                    }
                };
            }
            WriteToLogFile(exception);
            context.ExceptionHandled = true;
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
}
