using System.Net;

namespace CafeManagent.ErrorHandler
{
    public class ErrorCode
    {
        public HttpStatusCode StatusCode {  get; set; }
        public string Message { get; set; }


        public ErrorCode(HttpStatusCode statusCode, string message) { StatusCode = statusCode; Message = message; }
        public static ErrorCode LOI_CHAP_NHAN_REQUEST = new ErrorCode(HttpStatusCode.BadRequest, "Lỗi xảy ra khi cố gắng thao tác với request");
        public static ErrorCode LOI_THEM_REQUEST = new ErrorCode(HttpStatusCode.BadRequest, "Không thể thêm request");
        public static ErrorCode LOI_LAY_REQUEST = new ErrorCode(HttpStatusCode.BadRequest, "Không thể lấy request theo yêu cầu");
        public static ErrorCode LOI_PHAN_LOAI_REQUEST = new ErrorCode(HttpStatusCode.BadRequest, "Không thể phân loại request, lỗi hệ thống");
        public static ErrorCode LOI_TU_CHOI_REQUEST = new ErrorCode(HttpStatusCode.BadRequest, "Không thể từ chối requset, hãy thử lại sau");
        public static ErrorCode LOI_XOA_REQUEST = new ErrorCode(HttpStatusCode.BadRequest, "Không thể xóa request, thử lại sau");
        public static ErrorCode DefaultError = new ErrorCode(HttpStatusCode.BadRequest, "Không thể thực hiện thao tác");
        public static ErrorCode khongthechapnhan = new ErrorCode(HttpStatusCode.BadRequest, "khong th thay doi ca lam do loi ki thuat");
    }
}
