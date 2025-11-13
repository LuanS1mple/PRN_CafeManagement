using System.Net;

namespace CafeManagent.ErrorHandler
{
    public class AppException : Exception
    {
        public ErrorCode ErrorCode { get; set; }
        public string Root {  get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
        public AppException(ErrorCode errorCode, string root)
        {
            ErrorCode = errorCode;
            Root = root;
        }
        public AppException(ErrorCode errorCode, Exception e)
        {
            ErrorCode = errorCode;
            Root = e.InnerException.Message;
        }
    }
}
