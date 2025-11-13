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
                // Lấy thông tin TimeZoneInfo theo ID (vd: SE Asia Standard Time)
                TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                // Chuyển đổi thời gian hiện tại sang múi giờ đó
                return TimeZoneInfo.ConvertTime(dateTime, timeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                // Nếu không tìm thấy múi giờ (chỉ xảy ra trên một số hệ điều hành non-Windows),
                // thì dùng thời gian UTC để tránh lỗi, tuy nhiên VNPay yêu cầu múi giờ chuẩn.
                return dateTime.ToUniversalTime();
            }
            catch (Exception)
            {
                return dateTime;
            }
        }
    }


    public class VnPayHelper
    {
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
            var data = new StringBuilder();
            foreach (var key in _requestData.Keys)
            {
                data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(_requestData[key]) + "&");
            }

            string url = baseUrl + "?" + data.ToString();
            string rawData = GetHashData();
            string secureHash = HmacSHA512(hashSecret, rawData);
            return url + "vnp_SecureHash=" + secureHash;
        }

        private string GetHashData()
        {
            var data = new StringBuilder();
            foreach (var key in _requestData.Keys)
            {
                data.Append(key + "=" + _requestData[key] + "&");
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
                    data.Append(key + "=" + _responseData[key] + "&");
                }
            }
            if (data.Length > 0)
            {
                data.Length--;
            }
            string rawData = data.ToString();
            string calculatedHash = HmacSHA512(hashSecret, rawData);
            return calculatedHash.Equals(receivedHash, StringComparison.OrdinalIgnoreCase);
        }

        private string HmacSHA512(string key, string input)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(inputBytes);
                foreach (var b in hashBytes)
                {
                    hash.Append(b.ToString("x2")); 
                }
            }
            return hash.ToString();
        }
    }
}