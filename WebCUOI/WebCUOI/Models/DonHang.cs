using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCUOI.Models
{
    public class DonHang
    {
        [Key] // ✅ Khóa chính
        public int ID { get; set; }

        // 🔑 Khóa ngoại tới KhachHang
        public int KhachHangID { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime NgayDat { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TongTien { get; set; }

        [StringLength(50)]
        public string TrangThai { get; set; } = "Chờ Thanh Toán";

        // === 💵 Phương thức thanh toán (VNPay hoặc Tiền mặt) ===
        [StringLength(50)]
        public string PhuongThucThanhToan { get; set; } = "VNPay"; // hoặc "TienMat"

        // === 🏦 Thông tin thanh toán VNPAY ===
        [StringLength(100)]
        public string? VnpayTxnRef { get; set; } // Mã giao dịch do hệ thống sinh ra

        [StringLength(100)]
        public string? VnpayTransactionNo { get; set; } // Mã giao dịch tại VNPAY

        [DataType(DataType.DateTime)]
        public DateTime? PaymentDate { get; set; } // Thời điểm thanh toán thành công

        // === 🚚 Thông tin giao hàng / liên hệ ===
        [StringLength(255)]
        public string? ShippingAddress { get; set; } // Địa chỉ giao hàng

        [StringLength(20)]
        public string? PhoneNumber { get; set; } // Số điện thoại liên hệ

        [StringLength(500)]
        public string? Notes { get; set; } // Ghi chú của khách hàng

        public DateTime? NgaySuDung { get; set; }

        // === 🔗 Quan hệ với Khách hàng ===
        [ForeignKey("KhachHangID")]
        public virtual KhachHang? KhachHang { get; set; } // ✅ Cho phép null (để tránh lỗi 400)

        // === 📦 Danh sách chi tiết đơn hàng ===
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
    }
}
