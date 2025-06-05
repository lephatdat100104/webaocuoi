
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Chỉ cần cho DonHang và ChiTietDonHang
namespace WebCUOI.Models // Thay YourProjectName bằng tên namespace của dự án bạn (ví dụ: WebThueAoCuoi)
{
    public class AoCuoi
    {
        public int ID { get; set; } // Khóa chính

        [Required(ErrorMessage = "Tên áo cưới là bắt buộc.")]
        [StringLength(200, ErrorMessage = "Tên áo cưới không được vượt quá 200 ký tự.")]
        public string TenAoCuoi { get; set; }

        [Display(Name = "Mô tả")]
        public string MoTa { get; set; }

        [Required(ErrorMessage = "Giá thuê là bắt buộc.")]
        [DataType(DataType.Currency)] // Định dạng tiền tệ
        [Range(0.01, 1000000000.00, ErrorMessage = "Giá thuê phải lớn hơn 0.")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal GiaThue { get; set; }

        [Display(Name = "Kích cỡ")]
        [StringLength(50, ErrorMessage = "Kích cỡ không được vượt quá 50 ký tự.")]
        public string KichCo { get; set; } // Ví dụ: S, M, L, XL hoặc kích thước cụ thể

        [Display(Name = "Tình trạng")]
        [StringLength(100, ErrorMessage = "Tình trạng không được vượt quá 100 ký tự.")]
        public string TinhTrang { get; set; } // Ví dụ: "Mới", "Đã sử dụng", "Cần sửa chữa"

        [Display(Name = "Đường dẫn ảnh")]
        [StringLength(500, ErrorMessage = "Đường dẫn ảnh không được vượt quá 500 ký tự.")]
        public string HinhAnhURL { get; set; } // Đường dẫn lưu trữ ảnh của áo cưới

        [Display(Name = "Ngày nhập kho")]
        [DataType(DataType.Date)] // Chỉ lưu ngày tháng, không có thời gian
        public DateTime NgayNhapKho { get; set; } = DateTime.Now; // Mặc định là ngày hiện tại

    }
}