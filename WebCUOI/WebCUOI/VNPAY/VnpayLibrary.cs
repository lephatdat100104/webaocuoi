using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http; // Make sure this is included for HttpContext

namespace WebCUOI.VNPAY
{
    public class VnpayLibrary
    {
        public void ClearResponseData()
        {
            _responseData.Clear();
        }

        public void ClearRequestData()   // nếu cần thì thêm luôn
        {
            _requestData.Clear();
        }

        private SortedList<string, string> _requestData = new SortedList<string, string>(new VnpayCompare());
        private SortedList<string, string> _responseData = new SortedList<string, string>(new VnpayCompare());

        // Method to add request data
        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        // Method to add response data (from VNPAY callback)
        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        // Method to get response data by key
        public string GetResponseData(string key)
        {
            return _responseData.ContainsKey(key) ? _responseData[key] : string.Empty;
        }


        /// <summary>
        /// Tạo URL thanh toán VNPAY
        /// </summary>
        /// <param name="vnp_Url">URL của cổng VNPAY</param>
        /// <param name="vnp_HashSecret">Secret Key</param>
        /// <returns>URL thanh toán</returns>
        public string CreateRequestUrl(string vnp_Url, string vnp_HashSecret)
        {
            StringBuilder data = new StringBuilder();

            // 1. Chuỗi để tạo query string (key không encode, value có encode)
            foreach (KeyValuePair<string, string> kv in _requestData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append(kv.Key + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }

            string queryString = data.ToString().TrimEnd('&');

            // 2. Chuỗi để tính hash: key không encode, value phải encode
            StringBuilder hashData = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in _requestData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    hashData.Append(kv.Key + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }
            string rawHashData = hashData.ToString().TrimEnd('&');

            string vnp_SecureHash = HmacSHA512(vnp_HashSecret, rawHashData);

            return vnp_Url + "?" + queryString + "&vnp_SecureHash=" + vnp_SecureHash;
        }

        /// <summary>
        /// Xác thực chữ ký VNPAY trả về
        /// </summary>
        /// <param name="hashSecret">Secret Key</param>
        /// <param name="secureHash">Chữ ký từ VNPAY</param>
        /// <returns>True nếu chữ ký hợp lệ</returns>
        public bool ValidateSignature(string hashSecret, string secureHash)
        {
            StringBuilder signData = new StringBuilder();

            // Sắp xếp lại theo key (đã có SortedList nên OK)
            foreach (KeyValuePair<string, string> kv in _responseData)
            {
                if (!string.IsNullOrEmpty(kv.Value) && kv.Key != "vnp_SecureHash")
                {
                    // QUAN TRỌNG: phải UrlEncode VALUE, không encode key
                    signData.Append(kv.Key + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }

            string rawData = signData.ToString().TrimEnd('&');
            string myHash = HmacSHA512(hashSecret, rawData);

            return myHash.Equals(secureHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(inputBytes);
                foreach (byte b in hashBytes)
                {
                    hash.Append(b.ToString("x2"));
                }
            }
            return hash.ToString();
        }

        public string GetIpAddress(HttpContext context)
        {
            string ipAddress = string.Empty;
            if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            }
            else
            {
                ipAddress = context.Connection.RemoteIpAddress?.ToString();
            }
            return ipAddress;
        }
    }

    public class VnpayCompare : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return string.Compare(x, y, StringComparison.Ordinal);
        }
    }



}