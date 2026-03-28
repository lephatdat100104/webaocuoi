using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebCUOI.Data;
using WebCUOI.Models;
using Microsoft.EntityFrameworkCore;
namespace WebCUOI.Controllers.Api
{
    [Route("api/auth")]
    [ApiController]
    public class ApiAccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context; // Thêm cái này để lưu KhachHang
        private readonly IConfiguration _config;

        public ApiAccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context,
            IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _config = config;
        }

        // ==================== ĐĂNG KÝ ====================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // TẠO KHÁCH HÀNG TRONG BẢNG KHACHHANG (QUAN TRỌNG!)
                var khachHang = new KhachHang
                {
                    TenKhachHang = model.TenKhachHang ?? model.Email.Split('@')[0],
                    Email = model.Email,
                    SoDienThoai = model.SoDienThoai ?? "",
                    DiaChi = model.DiaChi ?? "",
                    UserID = user.Id // Liên kết với Identity
                };

                _context.KhachHang.Add(khachHang);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Đăng ký thành công!",
                    data = new { userId = user.Id }
                });
            }

            return BadRequest(new
            {
                success = false,
                message = "Đăng ký thất bại",
                errors = result.Errors.Select(e => e.Description)
            });
        }

        // ==================== ĐĂNG NHẬP + JWT ====================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized(new { success = false, message = "Email không tồn tại" });

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

            if (result.Succeeded)
            {
                var token = GenerateJwtToken(user);

                // Lấy thông tin khách hàng để Flutter hiển thị tên
                var khachHang = await _context.KhachHang
                    .FirstOrDefaultAsync(k => k.UserID == user.Id);

                return Ok(new
                {
                    success = true,
                    message = "Đăng nhập thành công",
                    token,
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        tenKhachHang = khachHang?.TenKhachHang ?? "Khách hàng",
                        soDienThoai = khachHang?.SoDienThoai
                    }
                });
            }

            return Unauthorized(new { success = false, message = "Sai mật khẩu" });
        }

        // ==================== TẠO JWT TOKEN ====================
        private string GenerateJwtToken(IdentityUser user)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _config["JwtSettings:Key"] ?? "YourSuperSecretKey1234567890123456"));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // ==================== DTOs HOÀN CHỈNH ====================
    public class RegisterModel
    {
       public string Email { get; set; } = "";
         public string Password { get; set; } = "";
        public string? TenKhachHang { get; set; }
        public string? SoDienThoai { get; set; }
        public string? DiaChi { get; set; }
    }

    public class LoginModel
    {
         public string Email { get; set; } = "";
         public string Password { get; set; } = "";
    }
}
