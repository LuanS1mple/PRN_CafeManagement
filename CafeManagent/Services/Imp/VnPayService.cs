using CafeManagent.dto.Configurations;
using CafeManagent.dto.response;
using CafeManagent.Services.Interface;
using CafeManagent.Ulties;
using Microsoft.Extensions.Options;

namespace CafeManagent.Services.Imp
{
    public class VnPayService : IVnPayService
    {
        private readonly VnPayConfig _vnpayConfig;

        // Inject configuration
        public VnPayService(IOptions<VnPayConfig> vnpayConfig)
        {
            _vnpayConfig = vnpayConfig.Value;
        }

        public string CreatePaymentUrl(string orderId, decimal amount, HttpContext context)
        {
            var timeZoneId = "SE Asia Standard Time"; // Timezone cho VN
            var createDate = DateTime.Now.InTimezone(timeZoneId);

            var vnpay = new VnPayHelper();

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", _vnpayConfig.TmnCode);
            long vnpAmount = (long)(amount * 100);
            vnpay.AddRequestData("vnp_Amount", vnpAmount.ToString());
            vnpay.AddRequestData("vnp_CreateDate", createDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", context.Connection.RemoteIpAddress?.ToString());
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang {orderId}");
            vnpay.AddRequestData("vnp_OrderType", "other"); // Hoặc billpayment
            vnpay.AddRequestData("vnp_ReturnUrl", _vnpayConfig.ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", orderId); // Mã giao dịch của bạn (tạm thời dùng OrderId)


            string paymentUrl = vnpay.CreateRequestUrl(_vnpayConfig.BaseUrl, _vnpayConfig.HashSecret);
            return paymentUrl;
        }

        public VnPayResponse ProcessVnPayReturn(IQueryCollection collections)
        {
            var vnpay = new VnPayHelper();
            foreach (var key in collections.Keys)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, collections[key]);
                }
            }

            var vnpSecureHash = vnpay.GetResponseData("vnp_SecureHash");
            var vnpOrderId = vnpay.GetResponseData("vnp_TxnRef");
            var vnpResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            bool isValidSignature = vnpay.ValidateSignature(vnpSecureHash, _vnpayConfig.HashSecret);

            if (isValidSignature)
            {
                // Chữ ký hợp lệ
                if (vnpResponseCode == "00")
                {
                    // Giao dịch thành công
                    return new VnPayResponse
                    {
                        IsSuccess = true,
                        OrderId = vnpOrderId,
                        ResponseCode = vnpResponseCode,
                        Message = "Giao dịch thành công."
                    };
                }
                else
                {
                    // Giao dịch thất bại (có thể do khách hàng hủy, sai thông tin thẻ...)
                    return new VnPayResponse
                    {
                        IsSuccess = false,
                        OrderId = vnpOrderId,
                        ResponseCode = vnpResponseCode,
                        Message = $"Giao dịch thất bại. Mã lỗi: {vnpResponseCode}"
                    };
                }
            }
            else
            {
                // Chữ ký không hợp lệ (Dữ liệu bị giả mạo)
                return new VnPayResponse
                {
                    IsSuccess = false,
                    OrderId = vnpOrderId,
                    ResponseCode = "97", // Mã lỗi do bạn tự đặt
                    Message = "Lỗi chữ ký điện tử (dữ liệu không toàn vẹn)."
                };
            }
        }
    }
}
