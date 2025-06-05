using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using WebCUOI.Data;
using WebCUOI.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebCUOI.Controllers
{
    public class GioHangController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GioHangController(ApplicationDbContext context)
        {
            _context = context;
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
                }
            }
            else if (productType == "DichVuCuoiHoi")
            {
                var dichVu = await _context.DichVuCuoiHoi.FirstOrDefaultAsync(d => d.ID == productId);
                if (dichVu != null)
                {
                    newItem = new GioHangItem
                    {
                        ID = gioHang.Count > 0 ? gioHang.Max(x => x.ID) + 1 : 1,
                        ProductId = dichVu.ID,
                        ProductName = dichVu.TenDichVu,
                        ProductType = "DichVuCuoiHoi",
                        Price = dichVu.GiaDichVu,
                        ImageURL = null,
                        Quantity = 1,
                        NgayBatDauThue = ngayBatDauThue,
                        NgayKetThucThue = null,
                        KichCo = null
                    };
                }
            }

            if (newItem != null)
            {
                gioHang.Add(newItem);
                SaveGioHang(gioHang);
            }

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
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Checkout()
        {
            var gioHang = GetGioHang();

            if (gioHang == null || !gioHang.Any())
            {
                TempData["Error"] = "Giỏ hàng trống. Vui lòng thêm sản phẩm.";
                return RedirectToAction("Index");
            }

            // TODO: Lưu đơn hàng vào DB nếu muốn

            HttpContext.Session.Remove("GioHang");

            TempData["Success"] = "Đặt hàng thành công!";
            return RedirectToAction("Index");
        }
    }
}
