using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.IO;

namespace CafeManagent.ErrorHandler
{
    public class GlobalExceptionHandler : IExceptionFilter
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(IWebHostEnvironment env, ILogger<GlobalExceptionHandler> logger)
        {
            _env = env;
            _logger = logger;
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

            context.ExceptionHandled = true;
        }

     

       

    
    }
}
