using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using WebCUOI.Models;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WebCUOI.Data;

namespace WebCUOI.Controllers
{
    public class DichVuCuoiHoiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public DichVuCuoiHoiController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.DichVuCuoiHoi.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var dichVu = await _context.DichVuCuoiHoi.FirstOrDefaultAsync(m => m.ID == id);
            if (dichVu == null) return NotFound();

            return View(dichVu);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var dichVu = await _context.DichVuCuoiHoi.FindAsync(id);
            if (dichVu == null) return NotFound();

            return View(dichVu);
        }

        // GET: DichVuCuoiHoi/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var dichVu = await _context.DichVuCuoiHoi.FindAsync(id);
            if (dichVu == null) return NotFound();

            return View(dichVu);
        }

        // POST: DichVuCuoiHoi/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DichVuCuoiHoi model, IFormFile ImageFile)
        {
            if (id != model.ID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var dichVu = await _context.DichVuCuoiHoi.FindAsync(id);
                    if (dichVu == null) return NotFound();

                    // Cập nhật các trường
                    dichVu.TenDichVu = model.TenDichVu;
                    dichVu.MoTa = model.MoTa;
                    dichVu.GiaDichVu = model.GiaDichVu;
                    dichVu.ThoiLuong = model.ThoiLuong;
                    dichVu.KhacDung = model.KhacDung;

                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        // Xóa ảnh cũ (nếu có)
                        if (!string.IsNullOrEmpty(dichVu.HinhAnhURL))
                        {
                            string oldPath = Path.Combine(_hostEnvironment.WebRootPath, dichVu.HinhAnhURL.TrimStart('/'));
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }

                        // Tạo thư mục uploads nếu chưa có
                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Lưu ảnh mới
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(fileStream);
                        }

                        dichVu.HinhAnhURL = "/uploads/" + uniqueFileName;
                    }
                    else
                    {
                        // Nếu không upload ảnh mới thì giữ nguyên ảnh cũ
                        dichVu.HinhAnhURL = model.HinhAnhURL;
                    }

                    _context.Update(dichVu);
                    await _context.SaveChangesAsync();

                    // Sau khi lưu thành công, redirect về Index hoặc trang khác
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật dữ liệu: " + ex.Message);
                }
            }

            return View(model);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dichVu = await _context.DichVuCuoiHoi.FindAsync(id);
            if (dichVu == null) return NotFound();

            // Xóa ảnh
            if (!string.IsNullOrEmpty(dichVu.HinhAnhURL))
            {
                string filePath = Path.Combine(_hostEnvironment.WebRootPath, dichVu.HinhAnhURL.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.DichVuCuoiHoi.Remove(dichVu);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Create và Edit action đã có sẵn như bạn gửi.
    }
}
