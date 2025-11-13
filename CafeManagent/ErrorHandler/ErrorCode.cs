using System.Net;

namespace CafeManagent.ErrorHandler
{
    public class ErrorCode
    {
        public HttpStatusCode StatusCode {  get; set; }
        public string Message { get; set; }
        public ErrorCode (HttpStatusCode statusCode, string message) {  StatusCode = statusCode; Message = message; }
        public static ErrorCode DefaultError = new ErrorCode(HttpStatusCode.BadRequest, "Không thể thực hiện thao tác");
        public static ErrorCode khongthechapnhan = new ErrorCode(HttpStatusCode.BadRequest, "khong th thay doi ca lam do loi ki thuat");
    }
}
