using System; 
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCUOI.Models
{
    public class DichVuCuoiHoi
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Tên dịch vụ là bắt buộc.")]
        [StringLength(200, ErrorMessage = "Tên dịch vụ không được vượt quá 200 ký tự.")]
        public string TenDichVu { get; set; }

        [Display(Name = "Mô tả dịch vụ")]
        public string MoTa { get; set; }

        [Required(ErrorMessage = "Giá dịch vụ là bắt buộc.")]
        [DataType(DataType.Currency)]
        [Range(0.01, 1000000000.00, ErrorMessage = "Giá dịch vụ phải lớn hơn 0.")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal GiaDichVu { get; set; }

        [Display(Name = "Thời lượng")]
        [StringLength(100, ErrorMessage = "Thời lượng không được vượt quá 100 ký tự.")]
        public string? ThoiLuong { get; set; }

        [Display(Name = "Tình trạng khả dụng")]
        public bool KhacDung { get; set; } = true;

        // Thêm thuộc tính đường dẫn ảnh
        [Display(Name = "Ảnh dịch vụ")]
        [StringLength(500, ErrorMessage = "Đường dẫn ảnh không được vượt quá 500 ký tự.")]
        public string? HinhAnhURL { get; set; }
        // ĐẢM BẢO CÓ CÁC THUỘC TÍNH NÀY:
        public string? LoaiSanPham { get; set; } // Thuộc tính mới
        public DateTime? NgayBatDauThue { get; set; } // Thuộc tính mới
        public DateTime? NgayKetThucThue { get; set; } // Thuộc tính mới
    }
}
