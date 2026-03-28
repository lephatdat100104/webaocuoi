namespace WebCUOI.Models
{
    public class PaymentRequestModel
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string OrderDescription { get; set; }
        public string Language { get; set; } // "vn" hoặc "en"
        public string IpAddress { get; set; }
        public string ReturnUrl { get; set; }
        public string? BankCode { get; set; } // Optional, for specific bank payment
        // public string? CreatedDate { get; set; } // Nếu bạn muốn truyền từ ngoài vào, ví dụ DateTime.Now.ToString("yyyyMMddHHmmss")
    }
}