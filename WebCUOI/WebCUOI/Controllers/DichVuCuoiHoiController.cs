using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using WebCUOI.Models;
using System.IO;
using System.Threading.Tasks;
using System;
using WebCUOI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

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

        // GET: Danh sách dịch vụ cưới hỏi
        public async Task<IActionResult> Index()
        {
            var list = await _context.DichVuCuoiHoi.ToListAsync();
            return View(list);
        }

        // GET: Chi tiết dịch vụ
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var dichVu = await _context.DichVuCuoiHoi.FirstOrDefaultAsync(m => m.ID == id);
            if (dichVu == null) return NotFound();

            return View(dichVu);
        }

        // GET: Tạo mới dịch vụ
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tạo mới dịch vụ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DichVuCuoiHoi model, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadsFolder); // Tạo nếu chưa có

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }

                    model.HinhAnhURL = "/uploads/" + uniqueFileName;
                }

                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Chỉnh sửa dịch vụ
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var dichVu = await _context.DichVuCuoiHoi.FindAsync(id);
            if (dichVu == null) return NotFound();

            return View(dichVu);
        }

        // POST: Chỉnh sửa dịch vụ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DichVuCuoiHoi model, IFormFile ImageFile)
        {
            if (id != model.ID) return NotFound();

            var existingDichVu = await _context.DichVuCuoiHoi.AsNoTracking().FirstOrDefaultAsync(d => d.ID == id);
            if (existingDichVu == null) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        // Xóa ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(existingDichVu.HinhAnhURL))
                        {
                            string oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, existingDichVu.HinhAnhURL.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Lưu ảnh mới
                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                        Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(fileStream);
                        }

                        model.HinhAnhURL = "/uploads/" + uniqueFileName;
                    }
                    else
                    {
                        model.HinhAnhURL = existingDichVu.HinhAnhURL;
                    }

                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DichVuCuoiHoiExists(model.ID)) return NotFound();
                    else throw;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi cập nhật dữ liệu: " + ex.Message);
                }
            }

            return View(model);
        }

        // GET: Xác nhận xóa dịch vụ
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var dichVu = await _context.DichVuCuoiHoi.FirstOrDefaultAsync(m => m.ID == id);
            if (dichVu == null) return NotFound();

            return View(dichVu);
        }

        // POST: Xóa dịch vụ
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dichVu = await _context.DichVuCuoiHoi.FindAsync(id);
            if (dichVu != null)
            {
                if (!string.IsNullOrEmpty(dichVu.HinhAnhURL))
                {
                    string imagePath = Path.Combine(_hostEnvironment.WebRootPath, dichVu.HinhAnhURL.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.DichVuCuoiHoi.Remove(dichVu);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DichVuCuoiHoiExists(int id)
        {
            return _context.DichVuCuoiHoi.Any(e => e.ID == id);
        }
    }
}
