using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace CafeManagent.Ulties
{
    /// <summary>
    /// Hàm mở rộng cho DateTime để chuyển đổi múi giờ.
    /// Cần thiết để tạo timestamp theo múi giờ chuẩn của VNPay.
    /// </summary>
    public static class DateTimeExtensions
    {
        public static DateTime InTimezone(this DateTime dateTime, string timeZoneId)
        {
            try
            {
                // Sử dụng TryGetValue để hỗ trợ cả Windows (SE Asia Standard Time) và Linux (Asia/Bangkok)
                if (TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out TimeZoneInfo timeZone))
                {
                    // Chuyển đổi thời gian hiện tại sang múi giờ đó
                    return TimeZoneInfo.ConvertTime(dateTime, timeZone);
                }
                // Nếu không tìm thấy TimeZoneId, chuyển về UTC
                return dateTime.ToUniversalTime();
            }
            catch (Exception)
            {
                // Xử lý các lỗi khác, có thể giữ nguyên hoặc chuyển về UTC
                return dateTime;
            }
        }
    }


    public class VnPayHelper
    {
        // Sử dụng StringComparer.Ordinal để đảm bảo sắp xếp theo bảng chữ cái chính xác (case-sensitive)
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(StringComparer.Ordinal);
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(StringComparer.Ordinal);

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        public string GetResponseData(string key)
        {
            return _responseData.ContainsKey(key) ? _responseData[key] : string.Empty;
        }

        public string CreateRequestUrl(string baseUrl, string hashSecret)
        {
            var paramData = new StringBuilder();

            // 1. Tạo chuỗi tham số ĐÃ UrlEncode để GỬI lên VNPAY
            foreach (var key in _requestData.Keys)
            {
                // VNPAY yêu cầu UrlEncode giá trị để tạo URL hợp lệ
                paramData.Append(key + "=" + WebUtility.UrlEncode(_requestData[key]) + "&");
            }

            // Xóa ký tự '&' cuối cùng
            if (paramData.Length > 0)
            {
                paramData.Length--;
            }

            // 2. Tạo Secure Hash
            // Lỗi chữ ký đã được khắc phục ở GetHashData()
            string rawData = GetHashData();
            System.Diagnostics.Debug.WriteLine($"VNPAY REQUEST RAW DATA TO HASH: {rawData}"); // Gỡ lỗi
            string secureHash = HmacSHA512(hashSecret, rawData);

            // 3. Nối Secure Hash vào URL
            string url = baseUrl + "?" + paramData.ToString() + "&vnp_SecureHash=" + secureHash;

            return url;
        }

        /// <summary>
        /// Tạo chuỗi băm thô (rawData) từ các tham số VNPAY.
        /// CHUẨN VNPAY: Giá trị (value) phải được UrlEncode khi tạo chuỗi băm thô.
        /// </summary>
        private string GetHashData()
        {
            var data = new StringBuilder();
            foreach (var key in _requestData.Keys)
            {
                // 💡 ĐÃ SỬA LỖI: Giá trị (value) phải được WebUtility.UrlEncode
                // trước khi đưa vào chuỗi băm thô.
                data.Append(key + "=" + WebUtility.UrlEncode(_requestData[key]) + "&");
            }

            if (data.Length > 0)
            {
                data.Length--;
            }
            return data.ToString();
        }

        public bool ValidateSignature(string receivedHash, string hashSecret)
        {
            var data = new StringBuilder();
            foreach (var key in _responseData.Keys)
            {
                if (key != "vnp_SecureHash")
                {
                    // 💡 ĐÃ SỬA LỖI: Khi xác thực, giá trị (value) nhận được từ QueryString
                    // cần phải được WebUtility.UrlEncode LẠI để tái tạo chuỗi băm thô.
                    data.Append(key + "=" + WebUtility.UrlEncode(_responseData[key]) + "&");
                }
            }

            if (data.Length > 0)
            {
                data.Length--;
            }

            string rawData = data.ToString();
            System.Diagnostics.Debug.WriteLine($"VNPAY RETURN RAW DATA TO HASH: {rawData}"); // Gỡ lỗi
            string calculatedHash = HmacSHA512(hashSecret, rawData);

            // So sánh Hash (không phân biệt chữ hoa/thường)
            return calculatedHash.Equals(receivedHash, StringComparison.OrdinalIgnoreCase);
        }

        private string HmacSHA512(string key, string input)
        {
            var hash = new StringBuilder();
            // Đảm bảo khóa bí mật không có khoảng trắng thừa
            key = key.Trim();

            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(inputBytes);
                foreach (var b in hashBytes)
                {
                    // Chuyển sang chuỗi hex viết thường (x2)
                    hash.Append(b.ToString("x2"));
                }
            }
            return hash.ToString();
        }
    }
}