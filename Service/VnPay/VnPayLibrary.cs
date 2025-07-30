using Final_Project.Models.VnPay;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Final_Project.Service.VnPay
{
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new(new VnPayCompare());
        private readonly SortedList<string, string> _responseData = new(new VnPayCompare());

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                _requestData.Add(key, value);
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                _responseData.Add(key, value);
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
        }

        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            var data = new StringBuilder();
            foreach (var (key, value) in _requestData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
                data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");

            var queryString = data.ToString();
            var signData = queryString.TrimEnd('&');
            var vnpSecureHash = HmacSha512(vnpHashSecret, signData);
            return $"{baseUrl}?{queryString}vnp_SecureHash={vnpSecureHash}";
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var rspRaw = GetResponseRawData();
            var myChecksum = HmacSha512(secretKey, rspRaw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string GetResponseRawData()
        {
            var data = new StringBuilder();
            _responseData.Remove("vnp_SecureHashType");
            _responseData.Remove("vnp_SecureHash");

            foreach (var (key, value) in _responseData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
                data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");

            return data.Length > 0 ? data.ToString().TrimEnd('&') : string.Empty;
        }

        private string HmacSha512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using var hmac = new HMACSHA512(keyBytes);
            foreach (var b in hmac.ComputeHash(inputBytes))
                hash.Append(b.ToString("x2"));
            return hash.ToString();
        }

        public string GetIpAddress(HttpContext context)
        {
            try
            {
                var ip = context.Connection.RemoteIpAddress;
                if (ip != null && ip.AddressFamily == AddressFamily.InterNetworkV6)
                    ip = Dns.GetHostEntry(ip).AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                return ip?.ToString() ?? "127.0.0.1";
            }
            catch { return "127.0.0.1"; }
        }

        public PaymentResponseModel GetFullResponseData(IQueryCollection collection, string hashSecret)
        {
            foreach (var (key, value) in collection)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                    AddResponseData(key, value);
            }

            var orderId = GetResponseData("vnp_TxnRef");
            var transactionNo = GetResponseData("vnp_TransactionNo");
            var responseCode = GetResponseData("vnp_ResponseCode");
            var orderInfo = GetResponseData("vnp_OrderInfo");
            var secureHash = collection["vnp_SecureHash"];

            var isValid = ValidateSignature(secureHash, hashSecret);

            return new PaymentResponseModel
            {
                Success = isValid && responseCode == "00",
                OrderId = orderId,
                TransactionId = transactionNo,
                PaymentId = transactionNo,
                OrderDescription = orderInfo,
                VnPayResponseCode = responseCode,
                PaymentMethod = "VNPAY",
                Token = secureHash
            };
        }
    }

    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return string.Compare(x, y, CultureInfo.InvariantCulture, CompareOptions.Ordinal);
        }
    }
}
