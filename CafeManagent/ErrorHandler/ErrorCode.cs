using System.Net;

namespace CafeManagent.ErrorHandler
{
    public class ErrorCode
    {
        public HttpStatusCode StatusCode {  get; set; }
        public string Message { get; set; }
        public ErrorCode (HttpStatusCode statusCode, string message) {  StatusCode = statusCode; Message = message; }
        public static ErrorCode DefaultError = new ErrorCode(HttpStatusCode.BadRequest, "Không thể thực hiện thao tác");
    }
}
