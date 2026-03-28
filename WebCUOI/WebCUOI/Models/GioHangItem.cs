using System;

namespace WebCUOI.Models
{
    public class GioHangItem
    {
        public int ID { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; } // "AoCuoi" hoặc "DichVu"
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? ImageURL { get; set; }
        public string? KichCo { get; set; }
        public DateTime? NgayBatDauThue { get; set; }
        public DateTime? NgayKetThucThue { get; set; }
    }
}
