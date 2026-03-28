namespace WebCUOI.Models
{
    public class VnPaymentResponseModel
    {
        public long OrderId { get; set; }
        public long Amount { get; set; } // Số tiền đã trả (đã chia 100 từ VNPAY trả về)
        public string ResponseCode { get; set; }
        public string TransactionNo { get; set; } // Mã giao dịch tại VNPAY
        public string BankCode { get; set; }
        public string PayDate { get; set; } // Thời gian thanh toán yyyyMMddHHmmss
        public string OrderInfo { get; set; }
        public string TmnCode { get; set; }
        public string CardType { get; set; }
        public string SecureHash { get; set; }
        public bool IsSuccess { get; set; } // Trạng thái thanh toán thành công hay thất bại
    }
}