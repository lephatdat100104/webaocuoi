using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity; // <<< THÊM DÒNG NÀY ĐỂ SỬ DỤNG IdentityUser

namespace WebCUOI.Models
{
    public class KhachHang
    {
        [Key]
        public int ID { get; set; }
        
                                                
        [Required(ErrorMessage = "Tên khách hàng là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên khách hàng không được vượt quá 100 ký tự.")]
        [Display(Name = "Tên khách hàng")]
        public string TenKhachHang { get; set; }

        [StringLength(250, ErrorMessage = "Địa chỉ không được vượt quá 250 ký tự.")]
        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        // <<< BỔ SUNG HAI THUỘC TÍNH NÀY ĐỂ LIÊN KẾT VỚI IDENTITY USER >>>
        // Khóa ngoại tới AspNetUsers.Id (ID của IdentityUser)
        // Kiểu dữ liệu là string vì User.Id của IdentityUser là string (GUID)
        public string? UserID { get; set; } // Dùng `string?` để cho phép null nếu một KhachHang không có tài khoản Identity

        [ForeignKey("UserID")]
        // Navigation property đến IdentityUser
        public IdentityUser? User { get; set; }
        // <<< KẾT THÚC BỔ SUNG >>>
    }
}