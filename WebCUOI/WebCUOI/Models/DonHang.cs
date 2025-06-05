using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Để dùng [ForeignKey]

namespace WebCUOI.Models // Thay YourProjectName bằng tên namespace của dự án bạn
{
    public class DonHang
    {
        public int ID { get; set; }

        [Display(Name = "Ngày đặt")]
        [DataType(DataType.Date)]
        public DateTime NgayDat { get; set; } = DateTime.Now;

        [Display(Name = "Ngày thuê/sử dụng")]
        [DataType(DataType.Date)]
        public DateTime NgaySuDung { get; set; }

        [Display(Name = "Ngày trả (nếu có)")]
        [DataType(DataType.Date)]
        public DateTime? NgayTra { get; set; } // Nullable nếu là dịch vụ không có ngày trả

        [Display(Name = "Tổng tiền")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")] // Định dạng chính xác trong DB
        public decimal TongTien { get; set; } = 0; // Mặc định 0

        [Display(Name = "Trạng thái đơn hàng")]
        [StringLength(100)]
        public string TrangThai { get; set; } = "Chờ xử lý"; // Ví dụ: Chờ xử lý, Đã xác nhận, Đã hoàn thành, Đã hủy

        // Khóa ngoại tới KhachHang
        [Display(Name = "Khách hàng")]
        public int KhachHangID { get; set; }
        [ForeignKey("KhachHangID")]
        public KhachHang KhachHang { get; set; } // Navigation property
    }
}