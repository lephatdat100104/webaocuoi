using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebCUOI.VNPAY;
using WebCUOI.Helpers;
using WebCUOI.Models;

namespace WebCUOI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly VnpayConfig _config;
        private readonly VnpayLibrary _vnpay;

        public PaymentController(IOptions<VnpayConfig> config, VnpayLibrary vnpay)
        {
            _config = config.Value;
            _vnpay = vnpay;
        }

        // 👉 Gọi từ client: POST /api/payment/create
        [HttpPost("create")]
        public IActionResult CreatePayment([FromBody] decimal amount)
        {
            string orderId = DateTime.UtcNow.Ticks.ToString();
            string ip = _vnpay.GetIpAddress(HttpContext);

            _vnpay.AddRequestData("vnp_Version", "2.1.0");
            _vnpay.AddRequestData("vnp_Command", "pay");
            _vnpay.AddRequestData("vnp_TmnCode", _config.TmnCode);
            _vnpay.AddRequestData("vnp_Amount", ((long)(amount * 100)).ToString());
            _vnpay.AddRequestData("vnp_BankCode", "VNPAYQR"); // ép sang QR
            _vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            _vnpay.AddRequestData("vnp_CurrCode", "VND");
            _vnpay.AddRequestData("vnp_IpAddr", ip);
            _vnpay.AddRequestData("vnp_Locale", "vn");
            _vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toán đơn hàng {orderId}");
            _vnpay.AddRequestData("vnp_OrderType", "other");
            _vnpay.AddRequestData("vnp_ReturnUrl", _config.ReturnUrl);
            _vnpay.AddRequestData("vnp_TxnRef", orderId);
            _vnpay.AddRequestData("vnp_ExpireDate", DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss"));

            string paymentUrl = _vnpay.CreateRequestUrl(_config.BaseUrl, _config.HashSecret);
            return Ok(new { paymentUrl });
        }

        // 👉 VNPAY gọi lại sau khi quét QR
        [HttpGet("confirm")]
        public IActionResult Confirm()
        {
            foreach (var q in Request.Query)
                _vnpay.AddResponseData(q.Key, q.Value);

            bool valid = _vnpay.ValidateSignature(_config.HashSecret, Request.Query["vnp_SecureHash"]);
            var res = new VnPaymentResponseModel
            {
                OrderId = long.Parse(Request.Query["vnp_TxnRef"]),
                Amount = long.Parse(Request.Query["vnp_Amount"]) / 100,
                ResponseCode = Request.Query["vnp_ResponseCode"],
                TransactionNo = Request.Query["vnp_TransactionNo"],
                BankCode = Request.Query["vnp_BankCode"],
                PayDate = Request.Query["vnp_PayDate"],
                OrderInfo = Request.Query["vnp_OrderInfo"],
                TmnCode = Request.Query["vnp_TmnCode"],
                CardType = Request.Query["vnp_CardType"],
                SecureHash = Request.Query["vnp_SecureHash"],
                IsSuccess = valid && Request.Query["vnp_ResponseCode"] == "00"
            };

            return Ok(res);
        }
    }
}
