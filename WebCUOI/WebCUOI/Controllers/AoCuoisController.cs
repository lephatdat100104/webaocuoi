using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebCUOI.Models;
using WebCUOI.Data;
using Microsoft.AspNetCore.Authorization; // Thay WebCUOI bằng tên dự án của bạn

namespace WebCUOI.Controllers // Thay WebCUOI bằng tên dự án của bạn
{
    [Authorize]
    public class AoCuoiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AoCuoiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AoCuoi
        public async Task<IActionResult> Index()
        {
            // Lấy tất cả áo cưới từ database và truyền sang View
            return View(await _context.AoCuoi.ToListAsync());
        }

        // GET: AoCuoi/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Tìm áo cưới theo ID
            var aoCuoi = await _context.AoCuoi
                .FirstOrDefaultAsync(m => m.ID == id);
            if (aoCuoi == null)
            {
                return NotFound();
            }

            return View(aoCuoi);
        }

        // GET: AoCuoi/Create (Hiển thị form thêm mới)
        public IActionResult Create()
        {
            return View();
        }

        // POST: AoCuoi/Create (Xử lý khi submit form thêm mới)
        [HttpPost]
        [ValidateAntiForgeryToken] // Chống tấn công CSRF
        public async Task<IActionResult> Create([Bind("ID,TenAoCuoi,MoTa,GiaThue,KichCo,TinhTrang,HinhAnhURL,NgayNhapKho")] AoCuoi aoCuoi)
        {
            if (ModelState.IsValid) // Kiểm tra dữ liệu hợp lệ theo Model
            {
                aoCuoi.NgayNhapKho = DateTime.Now; // Đảm bảo ngày nhập kho là ngày hiện tại
                _context.Add(aoCuoi); // Thêm vào bộ nhớ đệm của DbContext
                await _context.SaveChangesAsync(); // Lưu thay đổi vào database
                return RedirectToAction(nameof(Index)); // Chuyển hướng về trang danh sách
            }
            return View(aoCuoi); // Nếu không hợp lệ, hiển thị lại form với dữ liệu đã nhập
        }

        // GET: AoCuoi/Edit/5 (Hiển thị form chỉnh sửa)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aoCuoi = await _context.AoCuoi.FindAsync(id); // Tìm áo cưới theo ID
            if (aoCuoi == null)
            {
                return NotFound();
            }
            return View(aoCuoi);
        }

        // POST: AoCuoi/Edit/5 (Xử lý khi submit form chỉnh sửa)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AoCuoi aoCuoi, IFormFile ImageFile)
        {
            if (id != aoCuoi.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Nếu có ảnh mới
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/aocuoi", fileName);

                        // Tạo thư mục nếu chưa tồn tại
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }

                        // Gán đường dẫn ảnh mới
                        aoCuoi.HinhAnhURL = "/images/aocuoi/" + fileName;
                    }

                    _context.Update(aoCuoi);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AoCuoiExists(aoCuoi.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(aoCuoi);
        }

        // GET: AoCuoi/Delete/5 (Hiển thị xác nhận xóa)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aoCuoi = await _context.AoCuoi
                .FirstOrDefaultAsync(m => m.ID == id);
            if (aoCuoi == null)
            {
                return NotFound();
            }

            return View(aoCuoi);
        }

        // POST: AoCuoi/Delete/5 (Xử lý khi xác nhận xóa)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var aoCuoi = await _context.AoCuoi.FindAsync(id);
            if (aoCuoi != null)
            {
                _context.AoCuoi.Remove(aoCuoi); // Xóa khỏi bộ nhớ đệm
                await _context.SaveChangesAsync(); // Lưu thay đổi
            }

            return RedirectToAction(nameof(Index));
        }

        // Phương thức kiểm tra áo cưới có tồn tại không
        private bool AoCuoiExists(int id)
        {
            return _context.AoCuoi.Any(e => e.ID == id);
        }
    }
}