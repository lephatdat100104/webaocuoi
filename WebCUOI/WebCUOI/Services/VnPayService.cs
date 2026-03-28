using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System;
using WebCUOI.Models;
using WebCUOI.VNPAY;

namespace WebCUOI.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;

        public VnPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreatePaymentUrl(PaymentRequestModel model, HttpContext context)
        {
            var vnpay = new VnpayLibrary();

            string ipAddress = vnpay.GetIpAddress(context);

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", _configuration["VnpayConfig:TmnCode"]?.Trim());
            vnpay.AddRequestData("vnp_Amount", (model.Amount * 100).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_ReturnUrl", model.ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", model.OrderId.ToString());
            vnpay.AddRequestData("vnp_OrderInfo", model.OrderDescription);
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);
            vnpay.AddRequestData("vnp_Locale", model.Language);

            if (!string.IsNullOrEmpty(model.BankCode))
                vnpay.AddRequestData("vnp_BankCode", model.BankCode);

            // Lấy URL và HashSecret từ appsettings.json
            string vnpUrl = _configuration["VnpayConfig:BaseUrl"]?.Trim();
            string hashSecret = _configuration["VnpayConfig:HashSecret"]?.Trim();

            if (string.IsNullOrEmpty(hashSecret))
                throw new Exception("Cấu hình VNPAY: HashSecret đang bị null hoặc sai. Hãy kiểm tra appsettings.json.");

            string paymentUrl = vnpay.CreateRequestUrl(vnpUrl, hashSecret);

            // Debug log
            Console.WriteLine("PaymentUrl: " + paymentUrl);

            return paymentUrl;
        }

        public VnPaymentResponseModel ProcessVnPayReturn(IQueryCollection queryString)
        {
            var vnpay = new VnpayLibrary();
            string hashSecret = _configuration["VnpayConfig:HashSecret"]?.Trim();

            foreach (var key in queryString.Keys)
                vnpay.AddResponseData(key, queryString[key]);

            long orderId = Convert.ToInt64(vnpay.GetResponseData("vnp_TxnRef"));
            long vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;
            string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            string vnp_TransactionNo = vnpay.GetResponseData("vnp_TransactionNo");
            string vnp_BankCode = vnpay.GetResponseData("vnp_BankCode");
            string vnp_PayDate = vnpay.GetResponseData("vnp_PayDate");
            string vnp_OrderInfo = vnpay.GetResponseData("vnp_OrderInfo");
            string vnp_TmnCode = vnpay.GetResponseData("vnp_TmnCode");
            string vnp_CardType = vnpay.GetResponseData("vnp_CardType");
            string vnp_SecureHash = vnpay.GetResponseData("vnp_SecureHash");

            bool checkSignature = vnpay.ValidateSignature(hashSecret, vnp_SecureHash);

            // Debug log
            Console.WriteLine("CheckSignature: " + checkSignature);
            Console.WriteLine("VNPay SecureHash: " + vnp_SecureHash);

            return new VnPaymentResponseModel
            {
                OrderId = orderId,
                Amount = vnp_Amount,
                ResponseCode = vnp_ResponseCode,
                TransactionNo = vnp_TransactionNo,
                BankCode = vnp_BankCode,
                PayDate = vnp_PayDate,
                OrderInfo = vnp_OrderInfo,
                TmnCode = vnp_TmnCode,
                CardType = vnp_CardType,
                SecureHash = vnp_SecureHash,
                IsSuccess = checkSignature && vnp_ResponseCode == "00"
            };
        }
    }
}
