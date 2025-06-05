using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebCUOI.Models;

namespace WebCUOI.Models // Thay YourProjectName bằng tên namespace của dự án bạn
{
    public class ChiTietDonHang
    {
        public int ID { get; set; }

        // Khóa ngoại tới DonHang
        public int DonHangID { get; set; }
        [ForeignKey("DonHangID")]
        public DonHang DonHang { get; set; } // Navigation property

        // Có thể là AoCuoi hoặc DichVuCuoiHoi
        public int? AoCuoiID { get; set; } // Dùng int? để cho phép null
        [ForeignKey("AoCuoiID")]
        public AoCuoi AoCuoi { get; set; }

        public int? DichVuCuoiHoiID { get; set; } // Dùng int? để cho phép null
        [ForeignKey("DichVuCuoiHoiID")]
        public DichVuCuoiHoi DichVuCuoiHoi { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        public int SoLuong { get; set; }

        [Required(ErrorMessage = "Đơn giá là bắt buộc.")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DonGia { get; set; }

        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal ThanhTien { get; set; } // SoLuong * DonGia
    }
}