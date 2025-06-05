using System;
using System.ComponentModel.DataAnnotations;

namespace WebCUOI.Models
{
    public class GioHangItem
    {
        [Key]
        public int ID { get; set; } // ID của mục trong giỏ hàng
        public int ProductId { get; set; } // ID của AoCuoi hoặc DichVuCuoiHoi
        public string ProductName { get; set; } // Tên sản phẩm/dịch vụ
        public string ProductType { get; set; } // "AoCuoi" hoặc "DichVuCuoiHoi"
        public decimal Price { get; set; } // Giá thuê/dịch vụ
        public string ImageURL { get; set; } // Ảnh đại diện
        public int Quantity { get; set; } // Số lượng (cho dịch vụ, có thể là 1; cho áo cưới, có thể là số lượng áo nếu có nhiều)

        // Các thuộc tính bổ sung cho thuê/đặt dịch vụ
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày Thuê/Sử Dụng")]
        public DateTime? NgayBatDauThue { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày Trả")]
        public DateTime? NgayKetThucThue { get; set; }

        public string KichCo { get; set; } // Kích cỡ áo cưới

        // Tổng giá của một mục
        public decimal TotalPrice => Quantity * Price;
    }
}