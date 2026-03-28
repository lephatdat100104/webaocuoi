using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebCUOI.Data;
using Microsoft.EntityFrameworkCore;

namespace WebCUOI.Controllers.Api
{
    [Route("api/khachhang")]
    [ApiController]
    [Authorize] // Tất cả API trong controller này đều yêu cầu đã login + có JWT hợp lệ
    public class KhachHangController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public KhachHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/khachhang/me
        // → Lấy thông tin cá nhân của người đang đăng nhập
        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "Token không hợp lệ" });

            var khachHang = await _context.KhachHang
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.UserID == userId);

            if (khachHang == null)
                return NotFound(new { success = false, message = "Không tìm thấy thông tin khách hàng" });

            var result = new
            {
                success = true,
                data = new
                {
                    khachHang.ID,
                    khachHang.TenKhachHang,
                    khachHang.Email,
                    khachHang.SoDienThoai,
                    khachHang.DiaChi
                }
            };

            return Ok(result);
        }

        // PUT: api/khachhang/me
        // → Cập nhật thông tin cá nhân (tên, sđt, địa chỉ)
        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "Token không hợp lệ" });

            var khachHang = await _context.KhachHang
                .FirstOrDefaultAsync(k => k.UserID == userId);

            if (khachHang == null)
                return NotFound(new { success = false, message = "Không tìm thấy khách hàng" });

            // Chỉ cập nhật những field được gửi lên (nếu null thì giữ nguyên)
            khachHang.TenKhachHang = dto.TenKhachHang?.Trim() ?? khachHang.TenKhachHang;
            khachHang.SoDienThoai = dto.SoDienThoai?.Trim() ?? khachHang.SoDienThoai;
            khachHang.DiaChi = dto.DiaChi?.Trim() ?? khachHang.DiaChi;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Cập nhật thông tin thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi server", detail = ex.Message });
            }
        }
    }

    // DTO để nhận dữ liệu từ Flutter
    public class UpdateProfileDto
    {
        public string? TenKhachHang { get; set; }
        public string? SoDienThoai { get; set; }
        public string? DiaChi { get; set; }
    }
}
