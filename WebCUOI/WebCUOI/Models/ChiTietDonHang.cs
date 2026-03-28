using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System; // Thêm dòng này để dùng DateTime

namespace WebCUOI.Models
{
    [Table("ChiTietDonHang")]
    public class ChiTietDonHang
    {
        [Key] // Đảm bảo có Key cho ID
        public int ID { get; set; }

        // Khóa ngoại đến DonHang
        public int DonHangID { get; set; }
        [ForeignKey("DonHangID")]
        public virtual DonHang? DonHang { get; set; }// Sử dụng 'virtual' để kích hoạt Lazy Loading

        // Các thuộc tính chi tiết sản phẩm/dịch vụ
        [Required] // Tên sản phẩm là bắt buộc
        [StringLength(255)] // Giới hạn độ dài chuỗi
        public string TenSanPham { get; set; }

        [Required] // Loại sản phẩm là bắt buộc
        [StringLength(100)] // Giới hạn độ dài chuỗi
        public string LoaiSanPham { get; set; } // Ví dụ: "AoCuoi", "DichVuCuoiHoi"

        [StringLength(50)] // Kích cỡ có thể là S, M, L, XL,...
        public string? KichCo { get; set; } // Nullable nếu không phải lúc nào cũng có

        [DataType(DataType.Date)] // Chỉ lưu ngày, không lưu giờ
        public DateTime? NgayBatDauThue { get; set; } // Nullable nếu không phải sản phẩm thuê

        [DataType(DataType.Date)] // Chỉ lưu ngày, không lưu giờ
        public DateTime? NgayKetThucThue { get; set; } // Nullable nếu không phải sản phẩm thuê

        [Required] // Số lượng là bắt buộc
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")] // Đảm bảo số lượng hợp lệ
        public int SoLuong { get; set; }

        [Required] // Giá là bắt buộc
        [Column(TypeName = "decimal(18,2)")] // Định dạng decimal cho tiền tệ
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0.")] // Giá trị hợp lệ
        public decimal Gia { get; set; }

        // Thuộc tính không ánh xạ vào database, chỉ dùng trong code
        [NotMapped]
        public decimal ThanhTien => Gia * SoLuong;

        // Liên kết tùy chọn đến AoCuoi và DichVuCuoiHoi (cho biết đây là sản phẩm nào)
        // Các trường này có thể null vì một chi tiết đơn hàng chỉ là AoCuoi HOẶC DichVuCuoiHoi
        public int? AoCuoiID { get; set; }
        [ForeignKey("AoCuoiID")]
        public virtual AoCuoi? AoCuoi { get; set; } // Sử dụng 'virtual' và '?'. AoCuoi có thể null.

        public int? DichVuCuoiHoiID { get; set; }
        [ForeignKey("DichVuCuoiHoiID")]
        public virtual DichVuCuoiHoi? DichVuCuoiHoi { get; set; } // Sử dụng 'virtual' và '?'. DichVuCuoiHoi có thể null.
    }
}