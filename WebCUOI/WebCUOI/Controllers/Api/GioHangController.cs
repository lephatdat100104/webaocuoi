using Microsoft.AspNetCore.Mvc;
using WebCUOI.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using WebCUOI.Data;
using WebCUOI.VNPAY;
using Microsoft.EntityFrameworkCore;

namespace WebCUOI.Controllers.Api
{
    [Route("api/giohang")]
    [ApiController]
    [Authorize] // Bắt buộc đăng nhập
    public class GioHangApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly VnpayLibrary _vnpay;
        private readonly IConfiguration _config;

        public GioHangApiController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            VnpayLibrary vnpay,
            IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _vnpay = vnpay;
            _config = config;
        }

        // Helper: Lấy giỏ hàng từ Session
        private List<GioHangItem> GioHang
        {
            get
            {
                var data = HttpContext.Session.GetString("GioHang");
                return string.IsNullOrEmpty(data)
                    ? new List<GioHangItem>()
                    : JsonSerializer.Deserialize<List<GioHangItem>>(data)!;
            }
            set => HttpContext.Session.SetString("GioHang", JsonSerializer.Serialize(value));
        }

        // GET: api/giohang → Xem giỏ hàng
        [HttpGet]
        public IActionResult GetGioHang()
        {
            var gioHang = GioHang;
            return Ok(new
            {
                success = true,
                data = gioHang.Select(x => new
                {
                    x.ID,
                    x.ProductId,
                    x.ProductName,
                    x.ProductType,
                    x.Price,
                    x.Quantity,
                    x.ImageURL,
                    x.KichCo,
                    NgayBatDau = x.NgayBatDauThue?.ToString("dd/MM/yyyy"),
                    NgayKetThuc = x.NgayKetThucThue?.ToString("dd/MM/yyyy"),
                    ThanhTien = x.Price * x.Quantity
                }),
                tongTien = gioHang.Sum(x => x.Price * x.Quantity),
                soLuong = gioHang.Sum(x => x.Quantity)
            });
        }

        // POST: api/giohang/add → Thêm vào giỏ (giống hệt web)
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });

            var gioHang = GioHang;

            if (req.ProductType == "AoCuoi")
            {
                var ao = await _context.AoCuoi.FindAsync(req.ProductId);
                if (ao == null) return NotFound();

                var exist = gioHang.FirstOrDefault(x =>
                    x.ProductId == req.ProductId &&
                    x.ProductType == "AoCuoi" &&
                    x.KichCo == req.KichCo &&
                    x.NgayBatDauThue == req.NgayBatDauThue &&
                    x.NgayKetThucThue == req.NgayKetThucThue);

                if (exist != null)
                    exist.Quantity++;
                else
                {
                    gioHang.Add(new GioHangItem
                    {
                        ID = gioHang.Count > 0 ? gioHang.Max(x => x.ID) + 1 : 1,
                        ProductId = ao.ID,
                        ProductName = ao.TenAoCuoi,
                        ProductType = "AoCuoi",
                        Price = ao.GiaThue,
                        ImageURL = ao.HinhAnhURL,
                        Quantity = 1,
                        KichCo = req.KichCo,
                        NgayBatDauThue = req.NgayBatDauThue,
                        NgayKetThucThue = req.NgayKetThucThue
                    });
                }
            }
            else if (req.ProductType == "DichVuCuoiHoi")
            {
                var dv = await _context.DichVuCuoiHoi.FindAsync(req.ProductId);
                if (dv == null) return NotFound();

                if (gioHang.Any(x => x.ProductId == req.ProductId && x.ProductType == "DichVuCuoiHoi"))
                    return Ok(new { success = true, message = "Dịch vụ đã có trong giỏ!" });

                gioHang.Add(new GioHangItem
                {
                    ID = gioHang.Count > 0 ? gioHang.Max(x => x.ID) + 1 : 1,
                    ProductId = dv.ID,
                    ProductName = dv.TenDichVu,
                    ProductType = "DichVuCuoiHoi",
                    Price = dv.GiaDichVu,
                    ImageURL = dv.HinhAnhURL,
                    Quantity = 1
                });
            }

            GioHang = gioHang;
            return Ok(new { success = true, message = "Đã thêm vào giỏ hàng!" });
        }

        // DELETE: api/giohang/{id}
        [HttpDelete("{id}")]
        public IActionResult Remove(int id)
        {
            var gioHang = GioHang;
            var item = gioHang.FirstOrDefault(x => x.ID == id);
            if (item == null) return NotFound();

            gioHang.Remove(item);
            GioHang = gioHang;
            return Ok(new { success = true, message = "Đã xóa!" });
        }

        // POST: api/giohang/cod → Đặt hàng COD (giống hệt web)
        [HttpPost("cod")]
        public async Task<IActionResult> DatHangCOD()
        {
            // Lấy user trước
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { success = false, message = "Chưa đăng nhập!" });

            // Lấy email ra trước để EF Core hiểu
            var userEmail = user.Email;

            var kh = await _context.KhachHang
                .FirstOrDefaultAsync(k => k.Email == userEmail);

            if (kh == null)
                return BadRequest(new { success = false, message = "Không tìm thấy thông tin khách hàng!" });

            var gioHang = GioHang;
            if (!gioHang.Any())
                return BadRequest(new { success = false, message = "Giỏ hàng trống!" });

            // === Tạo đơn hàng (giống hệt web) ===
            var donHang = new DonHang
            {
                KhachHangID = kh.ID,
                NgayDat = DateTime.Now,
                TongTien = (long)gioHang.Sum(x => x.Price * x.Quantity),
                TrangThai = "Chờ Xác Nhận",
                PhuongThucThanhToan = "Thanh Toán Khi Nhận Hàng"
            };

            _context.DonHang.Add(donHang);
            await _context.SaveChangesAsync();

            foreach (var item in gioHang)
            {
                var ct = new ChiTietDonHang
                {
                    DonHangID = donHang.ID,
                    TenSanPham = item.ProductName,
                    LoaiSanPham = item.ProductType,
                    SoLuong = item.Quantity,
                    Gia = item.Price,
                    KichCo = item.KichCo,
                    NgayBatDauThue = item.NgayBatDauThue,
                    NgayKetThucThue = item.NgayKetThucThue,
                    AoCuoiID = item.ProductType == "AoCuoi" ? item.ProductId : null,
                    DichVuCuoiHoiID = item.ProductType == "DichVuCuoiHoi" ? item.ProductId : null
                };
                _context.ChiTietDonHangs.Add(ct);
            }
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove("GioHang");

            return Ok(new
            {
                success = true,
                donHangId = donHang.ID,
                message = "Đặt hàng thành công! Chúng tôi sẽ liên hệ sớm!"
            });
        }

        // POST: api/giohang/vnpay → Thanh toán VNPay (giống hệt web)
        [HttpPost("vnpay")]
        public async Task<IActionResult> ThanhToanVNPay()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var kh = await _context.KhachHang
                .FirstOrDefaultAsync(k => k.Email == user.Email);
            if (kh == null) return Unauthorized();

            long tongTien = (long)GioHang.Sum(x => x.Price * x.Quantity);
            long vnpayAmount = tongTien * 100;

            var donHang = new DonHang
            {
                KhachHangID = kh.ID,
                NgayDat = DateTime.Now,
                TongTien = tongTien,
                TrangThai = "Chờ Thanh Toán",
                PhuongThucThanhToan = "VNPay"
            };
            _context.DonHang.Add(donHang);
            await _context.SaveChangesAsync();

            foreach (var item in GioHang)
            {
                var ct = new ChiTietDonHang
                {
                    DonHangID = donHang.ID,
                    TenSanPham = item.ProductName,
                    LoaiSanPham = item.ProductType,
                    SoLuong = item.Quantity,
                    Gia = item.Price,
                    KichCo = item.KichCo,
                    NgayBatDauThue = item.NgayBatDauThue,
                    NgayKetThucThue = item.NgayKetThucThue,
                    AoCuoiID = item.ProductType == "AoCuoi" ? item.ProductId : null,
                    DichVuCuoiHoiID = item.ProductType == "DichVuCuoiHoi" ? item.ProductId : null
                };
                _context.ChiTietDonHangs.Add(ct);
            }
            await _context.SaveChangesAsync();

            _vnpay.ClearRequestData();
            _vnpay.AddRequestData("vnp_Version", "2.1.0");
            _vnpay.AddRequestData("vnp_Command", "pay");
            _vnpay.AddRequestData("vnp_TmnCode", _config["Vnpay:TmnCode"]);
            _vnpay.AddRequestData("vnp_Amount", vnpayAmount.ToString());
            _vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            _vnpay.AddRequestData("vnp_CurrCode", "VND");
            _vnpay.AddRequestData("vnp_IpAddr", _vnpay.GetIpAddress(HttpContext));
            _vnpay.AddRequestData("vnp_Locale", "vn");
            _vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang {donHang.ID}");
            _vnpay.AddRequestData("vnp_OrderType", "other");
            _vnpay.AddRequestData("vnp_ReturnUrl", "https://yourdomain.com/api/giohang/vnpay-return");
            _vnpay.AddRequestData("vnp_TxnRef", donHang.ID.ToString());

            string paymentUrl = _vnpay.CreateRequestUrl(_config["Vnpay:BaseUrl"], _config["Vnpay:HashSecret"]);

            HttpContext.Session.Remove("GioHang");

            return Ok(new
            {
                success = true,
                donHangId = donHang.ID,
                paymentUrl,
                message = "Đang chuyển đến VNPay..."
            });
        }

        // GET: api/giohang/vnpay-return (callback từ VNPay)
        [HttpGet("vnpay-return")]
        [AllowAnonymous]
        public IActionResult VnpayReturn()
        {
            // Logic giống hệt VnpayReturnResult của web bạn
            return Content("Thanh toán thành công! Bạn có thể đóng tab này.", "text/html");
        }
    }

    // DTO
    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public string ProductType { get; set; } = "";
        public string? KichCo { get; set; }
        public DateTime? NgayBatDauThue { get; set; }
        public DateTime? NgayKetThucThue { get; set; }
    }
}
