using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebCUOI.Data;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using WebCUOI.Models;
using Microsoft.EntityFrameworkCore;
using WebCUOI.Models.ViewModels;

namespace WebCUOI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> QuanLyNguoiDung()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    Roles = roles.ToList()
                });
            }

            return View(userList);
        }
        // GET
        public IActionResult DangKyNguoiDung()
        {
            return View();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKyNguoiDung(IdentityUser user, string Password)
        {
            if (ModelState.IsValid)
            {
                var result = await _userManager.CreateAsync(user, Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");
                    return RedirectToAction("QuanLyNguoiDung");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View(user);
        }

        // POST cho sửa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaNguoiDung(string id, IdentityUser model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.Email = model.Email;
            user.UserName = model.UserName;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("QuanLyNguoiDung");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        public async Task<IActionResult> SuaNguoiDung(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        public async Task<IActionResult> XoaNguoiDung(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("XoaNguoiDung")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanXoaNguoiDung(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction("QuanLyNguoiDung");
        }


        public IActionResult QuanLyAoCuoi()
        {
            var aoCuoiList = _context.AoCuoi.ToList();
            return View(aoCuoiList);
        }

        // GET: Admin/TaoAoCuoi
        public IActionResult TaoAoCuoi()
        {
            return View();
        }

        // POST: Admin/TaoAoCuoi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoAoCuoi([Bind("ID,TenAoCuoi,MoTa,GiaThue,KichCo,TinhTrang,HinhAnhURL,NgayNhapKho")] AoCuoi aoCuoi)
        {
            if (ModelState.IsValid)
            {
                aoCuoi.NgayNhapKho = DateTime.Now;
                _context.Add(aoCuoi);
                await _context.SaveChangesAsync();
                return RedirectToAction("QuanLyAoCuoi");
            }
            return View(aoCuoi);
        }

        // GET: Admin/SuaAoCuoi/5
        public async Task<IActionResult> SuaAoCuoi(int? id)
        {
            if (id == null) return NotFound();

            var aoCuoi = await _context.AoCuoi.FindAsync(id);
            return aoCuoi == null ? NotFound() : View(aoCuoi);
        }

        // POST: Admin/SuaAoCuoi/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaAoCuoi(int id, AoCuoi aoCuoi, IFormFile ImageFile)
        {
            if (id != aoCuoi.ID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/aocuoi", fileName);

                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }

                        aoCuoi.HinhAnhURL = "/images/aocuoi/" + fileName;
                    }

                    _context.Update(aoCuoi);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("QuanLyAoCuoi");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AoCuoiExists(aoCuoi.ID)) return NotFound();
                    else throw;
                }
            }

            return View(aoCuoi);
        }

        // GET: Admin/XoaAoCuoi/5
        public async Task<IActionResult> XoaAoCuoi(int? id)
        {
            if (id == null) return NotFound();

            var aoCuoi = await _context.AoCuoi.FirstOrDefaultAsync(m => m.ID == id);
            return aoCuoi == null ? NotFound() : View(aoCuoi);
        }

        // POST: Admin/XoaAoCuoi/5
        [HttpPost, ActionName("XoaAoCuoi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanXoaAoCuoi(int id)
        {
            var aoCuoi = await _context.AoCuoi.FindAsync(id);
            if (aoCuoi != null)
            {
                _context.AoCuoi.Remove(aoCuoi);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("QuanLyAoCuoi");
        }

        private bool AoCuoiExists(int id)
        {
            return _context.AoCuoi.Any(e => e.ID == id);
        }


        // GET: Admin/QuanLyDichVu
        public IActionResult QuanLyDichVu()
        {
            var dichVuList = _context.DichVuCuoiHoi.ToList();
            return View(dichVuList);
        }

        // GET: Admin/TaoDichVu
        public IActionResult TaoDichVu()
        {
            return View();
        }

        // POST: Admin/TaoDichVu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoDichVu(DichVuCuoiHoi dichVu, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/dichvu", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    dichVu.HinhAnhURL = "/images/dichvu/" + fileName;
                }

                _context.DichVuCuoiHoi.Add(dichVu);
                await _context.SaveChangesAsync();
                return RedirectToAction("QuanLyDichVu");
            }

            return View(dichVu);
        }

        // GET: Admin/SuaDichVu/5
        public IActionResult SuaDichVu(int id)
        {
            var dichVu = _context.DichVuCuoiHoi.Find(id);
            if (dichVu == null) return NotFound();
            return View(dichVu);
        }

        // POST: Admin/SuaDichVu/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaDichVu(int id, DichVuCuoiHoi dichVu, IFormFile ImageFile)
        {
            if (id != dichVu.ID) return NotFound();

            var existing = await _context.DichVuCuoiHoi.AsNoTracking().FirstOrDefaultAsync(d => d.ID == id);
            if (existing == null) return NotFound();

            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(existing.HinhAnhURL))
                    {
                        string oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existing.HinhAnhURL.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/dichvu", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    dichVu.HinhAnhURL = "/images/dichvu/" + fileName;
                }
                else
                {
                    dichVu.HinhAnhURL = existing.HinhAnhURL;
                }

                _context.Update(dichVu);
                await _context.SaveChangesAsync();
                return RedirectToAction("QuanLyDichVu");
            }

            return View(dichVu);
        }

        // GET: Admin/XoaDichVu/5
        public IActionResult XoaDichVu(int id)
        {
            var dichVu = _context.DichVuCuoiHoi.Find(id);
            if (dichVu == null) return NotFound();
            return View(dichVu);
        }

        // POST: Admin/XoaDichVu/5
        [HttpPost, ActionName("XoaDichVu")]
        [ValidateAntiForgeryToken]
        public IActionResult XacNhanXoaDichVu(int id)
        {
            var dichVu = _context.DichVuCuoiHoi.Find(id);
            if (dichVu == null) return NotFound();

            if (!string.IsNullOrEmpty(dichVu.HinhAnhURL))
            {
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", dichVu.HinhAnhURL.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }

            _context.DichVuCuoiHoi.Remove(dichVu);
            _context.SaveChanges();
            return RedirectToAction("QuanLyDichVu");
        }
        public async Task<IActionResult> QuanLyDonHang()
        {
            var donHangList = await _context.DonHang
                .Include(d => d.KhachHang)
                .ToListAsync();
            return View(donHangList);
        }
        public async Task<IActionResult> ChiTietDonHang(int id)
        {
            var donHang = await _context.DonHang
                .Include(d => d.KhachHang)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.AoCuoi)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.DichVuCuoiHoi)
                .FirstOrDefaultAsync(d => d.ID == id);

            if (donHang == null) return NotFound();

            return View(donHang);
        }

        
    }

}
