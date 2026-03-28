using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using WebCUOI.Data;
using WebCUOI.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using WebCUOI.VNPAY;
using System.Security.Claims;
using System;
using WebCUOI.Services;   // ✅ thêm để dùng NotificationService

namespace WebCUOI.Controllers
{
    [Authorize]
    public class GioHangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly VnpayConfig _vnpayConfig;
        private readonly VnpayLibrary _vnpayLibrary;
        private readonly ILogger<GioHangController> _logger;
        private readonly NotificationService _notify;   // ✅ thêm

        public GioHangController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IOptions<VnpayConfig> vnpayConfig,
            VnpayLibrary vnpayLibrary,
            ILogger<GioHangController> logger,
            NotificationService notify    // ✅ thêm
        )
        {
            _context = context;
            _userManager = userManager;
            _vnpayConfig = vnpayConfig.Value;
            _vnpayLibrary = vnpayLibrary;
            _logger = logger;
            _notify = notify; // ✅ gán
        }

        // GET: GioHang/Index
        public IActionResult Index()
        {
            List<GioHangItem> gioHang = GetGioHang();
            return View(gioHang);
        }

        private List<GioHangItem> GetGioHang()
        {
            var sessionGioHang = HttpContext.Session.GetString("GioHang");
            if (sessionGioHang == null)
            {
                return new List<GioHangItem>();
            }
            return JsonSerializer.Deserialize<List<GioHangItem>>(sessionGioHang);
        }

        private void SaveGioHang(List<GioHangItem> gioHang)
        {
            HttpContext.Session.SetString("GioHang", JsonSerializer.Serialize(gioHang));
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, string productType, DateTime? ngayBatDauThue, DateTime? ngayKetThucThue, string kichCo)
        {
            List<GioHangItem> gioHang = GetGioHang();
            GioHangItem newItem = null;

            if (productType == "AoCuoi")
            {
                var aoCuoi = await _context.AoCuoi.FirstOrDefaultAsync(a => a.ID == productId);
                if (aoCuoi != null)
                {
                    var existingItem = gioHang.FirstOrDefault(item =>
                        item.ProductId == productId &&
                        item.ProductType == productType &&
                        item.KichCo == kichCo &&
                        item.NgayBatDauThue == ngayBatDauThue &&
                        item.NgayKetThucThue == ngayKetThucThue);

                    if (existingItem != null)
                    {
                        existingItem.Quantity += 1;
                    }
                    else
                    {
                        newItem = new GioHangItem
                        {
                            ID = gioHang.Count > 0 ? gioHang.Max(x => x.ID) + 1 : 1,
                            ProductId = aoCuoi.ID,
                            ProductName = aoCuoi.TenAoCuoi,
                            ProductType = "AoCuoi",
                            Price = aoCuoi.GiaThue,
                            ImageURL = aoCuoi.HinhAnhURL,
                            Quantity = 1,
                            NgayBatDauThue = ngayBatDauThue,
                            NgayKetThucThue = ngayKetThucThue,
                            KichCo = kichCo
                        };
                        gioHang.Add(newItem);
                    }
                }
            }
            else if (productType == "DichVuCuoiHoi")
            {
                var dichVu = await _context.DichVuCuoiHoi.FirstOrDefaultAsync(d => d.ID == productId);
                if (dichVu != null)
                {
                    var existingItem = gioHang.FirstOrDefault(item =>
                        item.ProductId == productId &&
                        item.ProductType == productType);

                    if (existingItem == null)
                    {
                        newItem = new GioHangItem
                        {
                            ID = gioHang.Count > 0 ? gioHang.Max(x => x.ID) + 1 : 1,
                            ProductId = dichVu.ID,
                            ProductName = dichVu.TenDichVu,
                            ProductType = "DichVuCuoiHoi",
                            Price = dichVu.GiaDichVu,
                            ImageURL = dichVu.HinhAnhURL,
                            Quantity = 1,
                            NgayBatDauThue = ngayBatDauThue,
                            NgayKetThucThue = null,
                            KichCo = null
                        };
                        gioHang.Add(newItem);
                    }
                    else
                    {
                        TempData["InfoMessage"] = $"Dịch vụ '{dichVu.TenDichVu}' đã có trong giỏ hàng.";
                    }
                }
            }
            SaveGioHang(gioHang);
            TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng!";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int id)
        {
            List<GioHangItem> gioHang = GetGioHang();
            var itemToRemove = gioHang.FirstOrDefault(item => item.ID == id);
            if (itemToRemove != null)
            {
                gioHang.Remove(itemToRemove);
                SaveGioHang(gioHang);
                TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Checkout()
        {
            var gioHang = GetGioHang();
            if (gioHang == null || !gioHang.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống. Vui lòng thêm sản phẩm để thanh toán.";
                return RedirectToAction("Index");
            }
            return View(gioHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVnpayPayment()
        {
            var gioHang = GetGioHang();

            if (gioHang == null || !gioHang.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống. Vui lòng thêm sản phẩm.";
                return RedirectToAction("Index");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để đặt hàng.";
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var khachHang = await _context.KhachHang.FirstOrDefaultAsync(k => k.Email == currentUser.Email);
            if (khachHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng liên kết. Vui lòng liên hệ hỗ trợ.";
                return RedirectToAction("Index");
            }

            long orderTotalAmount = (long)gioHang.Sum(item => item.Price * item.Quantity);
            long vnpayAmount = orderTotalAmount * 100;

            var newDonHang = new DonHang
            {
                KhachHangID = khachHang.ID,
                NgayDat = DateTime.Now,
                TongTien = orderTotalAmount,
                TrangThai = "Chờ Thanh Toán",
            };

            _context.DonHang.Add(newDonHang);
            await _context.SaveChangesAsync();

            // ✅ Thông báo cho admin khi có đơn hàng mới
            await _notify.CreateAsync(
                "ADMIN",
                $"Khách hàng {User.Identity.Name} vừa đặt đơn hàng #{newDonHang.ID} với tổng tiền {orderTotalAmount:N0}₫"
            );

            foreach (var item in gioHang)
            {
                var chiTiet = new ChiTietDonHang
                {
                    DonHangID = newDonHang.ID,
                    TenSanPham = item.ProductName,
                    LoaiSanPham = item.ProductType,
                    SoLuong = item.Quantity,
                    Gia = item.Price,
                    KichCo = item.KichCo,
                    NgayBatDauThue = item.NgayBatDauThue,
                    NgayKetThucThue = item.NgayKetThucThue
                };

                if (item.ProductType == "AoCuoi")
                {
                    chiTiet.AoCuoiID = item.ProductId;
                }
                else if (item.ProductType == "DichVuCuoiHoi")
                {
                    chiTiet.DichVuCuoiHoiID = item.ProductId;
                }
                _context.ChiTietDonHangs.Add(chiTiet);
            }
            await _context.SaveChangesAsync();

            // ==== Tạo request VNPAY (phần này giữ nguyên logic bạn đang dùng) ====
            string vnp_Version = "2.1.0";
            string vnp_Command = "pay";
            string vnp_TmnCode = _vnpayConfig.TmnCode;
            string vnp_Amount = vnpayAmount.ToString();
            string vnp_CreateDate = DateTime.Now.ToString("yyyyMMddHHmmss");
            string vnp_CurrCode = "VND";
            string vnp_IpAddr = _vnpayLibrary.GetIpAddress(HttpContext);
            string vnp_Locale = "vn";
            string vnp_OrderInfo = $"Thanh toan don hang {newDonHang.ID} tai WebCUOI";
            string vnp_OrderType = "other";
            string vnp_ReturnUrl = _vnpayConfig.ReturnUrl;
            string vnp_TxnRef = newDonHang.ID.ToString();

            _vnpayLibrary.AddRequestData("vnp_Version", vnp_Version);
            _vnpayLibrary.AddRequestData("vnp_Command", vnp_Command);
            _vnpayLibrary.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            _vnpayLibrary.AddRequestData("vnp_Amount", vnp_Amount);
            _vnpayLibrary.AddRequestData("vnp_CreateDate", vnp_CreateDate);
            _vnpayLibrary.AddRequestData("vnp_CurrCode", vnp_CurrCode);
            _vnpayLibrary.AddRequestData("vnp_IpAddr", vnp_IpAddr);
            _vnpayLibrary.AddRequestData("vnp_Locale", vnp_Locale);
            _vnpayLibrary.AddRequestData("vnp_OrderInfo", vnp_OrderInfo);
            _vnpayLibrary.AddRequestData("vnp_OrderType", vnp_OrderType);
            _vnpayLibrary.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl);
            _vnpayLibrary.AddRequestData("vnp_TxnRef", vnp_TxnRef);

            string paymentUrl = _vnpayLibrary.CreateRequestUrl(_vnpayConfig.BaseUrl, _vnpayConfig.HashSecret);

            _logger.LogInformation($"VNPAY Redirect URL: {paymentUrl}");

            return Redirect(paymentUrl);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VnpayReturnResult()
        {
            if (HttpContext.Request.Query.Count == 0)
            {
                TempData["ErrorMessage"] = "Truy cập không hợp lệ.";
                return RedirectToAction("Index", "Home");
            }

            // DÒNG NÀY LÀM MỌI THỨ HẾT LỖI TRÙNG KEY!!!
            _vnpayLibrary.ClearResponseData();   // <--- THÊM DÒNG NÀY VÀO!!!

            var vnpayData = Request.Query;
           

            foreach (string s in vnpayData.Keys)
            {
                if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                {
                    _vnpayLibrary.AddResponseData(s, vnpayData[s]);
                }
            }



            string vnp_SecureHash = vnpayData["vnp_SecureHash"];
            string vnp_TxnRef = vnpayData["vnp_TxnRef"];
            string vnp_ResponseCode = vnpayData["vnp_ResponseCode"];
            string vnp_TransactionStatus = vnpayData["vnp_TransactionStatus"];
            string vnp_Amount = vnpayData["vnp_Amount"];
            string vnp_TransactionNo = vnpayData["vnp_TransactionNo"];
            string vnp_PayDate = vnpayData["vnp_PayDate"];

            _logger.LogInformation($"VNPAY Return - TxnRef: {vnp_TxnRef}, ResponseCode: {vnp_ResponseCode}, Status: {vnp_TransactionStatus}, Amount: {vnp_Amount}, SecureHash: {vnp_SecureHash}");

            bool isValidSignature = _vnpayLibrary.ValidateSignature(_vnpayConfig.HashSecret, vnp_SecureHash);

            if (isValidSignature)
            {
                int orderId = 0;
                if (!int.TryParse(vnp_TxnRef, out orderId))
                {
                    TempData["ErrorMessage"] = "Lỗi: Mã đơn hàng không hợp lệ từ VNPAY.";
                    _logger.LogError($"VNPAY Return: Mã đơn hàng không hợp lệ (vnp_TxnRef: {vnp_TxnRef})");
                    return View("VnpayReturnResult");
                }

                var order = await _context.DonHang.FindAsync(orderId);

                if (order != null)
                {
                    if (Convert.ToInt64(vnp_Amount) == order.TongTien * 100)
                    {
                        if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                        {
                            order.TrangThai = "Đã Thanh Toán";
                            order.VnpayTxnRef = vnp_TxnRef;
                            order.VnpayTransactionNo = vnp_TransactionNo;
                            order.PaymentDate = DateTime.ParseExact(vnp_PayDate, "yyyyMMddHHmmss", null);

                            HttpContext.Session.Remove("GioHang");
                            TempData["SuccessMessage"] = $"Đơn hàng {order.ID} đã được thanh toán thành công! Mã giao dịch VNPAY: {vnp_TransactionNo}";
                            _logger.LogInformation($"VNPAY: Đơn hàng {order.ID} thanh toán thành công. Mã VNPAY: {vnp_TransactionNo}");
                        }
                        else
                        {
                            order.TrangThai = "Thanh Toán Thất Bại";
                            order.VnpayTxnRef = vnp_TxnRef;
                            order.VnpayTransactionNo = vnp_TransactionNo;
                            TempData["ErrorMessage"] = $"Thanh toán không thành công cho đơn hàng {order.ID}. Mã lỗi VNPAY: {vnp_ResponseCode} - Trạng thái: {vnp_TransactionStatus}.";
                            _logger.LogWarning($"VNPAY: Đơn hàng {order.ID} thanh toán thất bại. Mã lỗi: {vnp_ResponseCode}, Trạng thái: {vnp_TransactionStatus}");
                        }
                    }
                    else
                    {
                        order.TrangThai = "Lỗi Số Tiền";
                        TempData["ErrorMessage"] = "Lỗi bảo mật: Số tiền thanh toán không khớp!";
                        _logger.LogError($"VNPAY: Sai số tiền cho đơn hàng {orderId}. VNPAY: {vnp_Amount}, Hệ thống: {order.TongTien * 100}");
                    }
                    await _context.SaveChangesAsync();
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng tương ứng.";
                    _logger.LogError($"VNPAY: Không tìm thấy đơn hàng với mã TxnRef: {vnp_TxnRef}");
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Lỗi bảo mật: Chữ ký không hợp lệ từ VNPAY!";
                _logger.LogError($"VNPAY: Chữ ký không hợp lệ cho phản hồi đơn hàng {vnp_TxnRef}. Input Hash: {vnp_SecureHash}");
            }

            return View("VnpayReturnResult");
        }

        [AllowAnonymous]
        public IActionResult ThongKe()
        {
            ViewBag.TongTien = HttpContext.Session.GetString("ThongKe_TongTien");
            ViewBag.TongAoCuoi = HttpContext.Session.GetInt32("ThongKe_TongAoCuoi");
            ViewBag.TongDichVu = HttpContext.Session.GetInt32("ThongKe_TongDichVu");
            ViewBag.TongMatHang = HttpContext.Session.GetInt32("ThongKe_TongMatHang");

            return View();
        }

        public async Task<IActionResult> LichSuMuaHang()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var khachHang = await _context.KhachHang.FirstOrDefaultAsync(kh => kh.Email == user.Email);

            if (khachHang == null)
            {
                ViewBag.ErrorMessage = "Không tìm thấy thông tin khách hàng liên kết với tài khoản của bạn. Vui lòng liên hệ hỗ trợ.";
                return View(new List<WebCUOI.Models.DonHang>());
            }

            var donHangs = await _context.DonHang
                                        .Where(dh => dh.KhachHangID == khachHang.ID)
                                        .Include(dh => dh.ChiTietDonHangs)
                                        .OrderByDescending(dh => dh.NgayDat)
                                        .ToListAsync();

            return View(donHangs);
        }
        // POST: GioHang/CreateCODOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCODOrder()
        {
            var gioHang = GetGioHang();

            if (gioHang == null || !gioHang.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống. Vui lòng thêm sản phẩm.";
                return RedirectToAction("Index");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để đặt hàng.";
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var khachHang = await _context.KhachHang.FirstOrDefaultAsync(k => k.Email == currentUser.Email);
            if (khachHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng liên kết. Vui lòng liên hệ hỗ trợ.";
                return RedirectToAction("Index");
            }

            // Tính tổng tiền
            long orderTotalAmount = (long)gioHang.Sum(item => item.Price * item.Quantity);

            // 1. Tạo Đơn Hàng Mới
            var newDonHang = new DonHang
            {
                KhachHangID = khachHang.ID,
                NgayDat = DateTime.Now,
                TongTien = orderTotalAmount,
                // Trạng thái đơn hàng
                TrangThai = "Chờ Xác Nhận",
                // Thiết lập phương thức thanh toán
                PhuongThucThanhToan = "Thanh Toán Khi Nhận Hàng",
                VnpayTxnRef = null,
                VnpayTransactionNo = null,
                PaymentDate = null
            };

            _context.DonHang.Add(newDonHang);
            await _context.SaveChangesAsync();

            // 2. Thêm Chi Tiết Đơn Hàng
            foreach (var item in gioHang)
            {
                var chiTiet = new ChiTietDonHang
                {
                    DonHangID = newDonHang.ID,
                    TenSanPham = item.ProductName,
                    LoaiSanPham = item.ProductType,
                    SoLuong = item.Quantity,
                    Gia = item.Price,
                    KichCo = item.KichCo,
                    NgayBatDauThue = item.NgayBatDauThue,
                    NgayKetThucThue = item.NgayKetThucThue,
                    AoCuoiID = item.ProductType == "AoCuoi" ? (int?)item.ProductId : null,
                    DichVuCuoiHoiID = item.ProductType == "DichVuCuoiHoi" ? (int?)item.ProductId : null
                };
                _context.ChiTietDonHangs.Add(chiTiet);
            }
            await _context.SaveChangesAsync();

            // 3. Xóa giỏ hàng và thông báo
            HttpContext.Session.Remove("GioHang");

            // ✅ Thông báo cho admin khi có đơn hàng COD mới
            await _notify.CreateAsync(
                "ADMIN",
                $"Khách hàng {User.Identity.Name} vừa đặt đơn hàng COD mới #{newDonHang.ID} với tổng tiền {orderTotalAmount:N0}₫"
            );

            TempData["SuccessMessage"] = $"Đặt hàng thành công! Đơn hàng #{newDonHang.ID} sẽ được xác nhận sớm. Bạn sẽ thanh toán khi nhận hàng/dịch vụ.";

            // Chuyển hướng đến trang chi tiết đơn hàng hoặc lịch sử mua hàng
            return RedirectToAction("LichSuMuaHang");
        }
    }
}
