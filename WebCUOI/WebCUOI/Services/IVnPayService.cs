using Microsoft.AspNetCore.Http; // Cần thiết để truyền HttpContext
using WebCUOI.Models; // Đảm bảo đúng namespace cho PaymentRequestModel

namespace WebCUOI.Services
{
    public interface IVnPayService
    {
        /// <summary>
        /// Tạo URL thanh toán VNPAY.
        /// </summary>
        /// <param name="model">Dữ liệu yêu cầu thanh toán.</param>
        /// <param name="context">HttpContext để lấy thông tin IP.</param>
        /// <returns>URL để redirect đến cổng thanh toán VNPAY.</returns>
        string CreatePaymentUrl(PaymentRequestModel model, HttpContext context);

        /// <summary>
        /// Xử lý kết quả trả về từ VNPAY sau khi thanh toán.
        /// </summary>
        /// <param name="queryString">Query string từ URL trả về của VNPAY.</param>
        /// <returns>Đối tượng VnPaymentResponseModel chứa kết quả xử lý.</returns>
        VnPaymentResponseModel ProcessVnPayReturn(IQueryCollection queryString);
    }
}