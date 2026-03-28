using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebCUOI.Data;
using WebCUOI.Models;
using System.Linq;
using System.Threading.Tasks;

namespace WebCUOI.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🟢 Trang danh sách thông báo
        public IActionResult Index()
        {
            var userId = User.Identity.Name;
            var list = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return View(list);
        }

        // 🟢 Đánh dấu một thông báo đã đọc
        public IActionResult MarkRead(int id)
        {
            var noti = _context.Notifications.Find(id);
            if (noti != null)
            {
                noti.IsRead = true;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // 🟢 Lấy 5 thông báo mới nhất
        [HttpGet]
        public IActionResult GetRecent()
        {
            var userId = User.Identity.Name;
            var list = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToList();

            return PartialView("_NotificationList", list);
        }

        // 🟢 Lấy số lượng thông báo chưa đọc
        [HttpGet]
        public JsonResult GetUnreadCount()
        {
            var userId = User.Identity.Name;
            int count = _context.Notifications.Count(n => n.UserId == userId && !n.IsRead);
            return Json(new { count });
        }

        // 🟢 Đánh dấu tất cả là đã đọc
        [HttpPost]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = User.Identity.Name;
            var notis = _context.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToList();

            if (notis.Any())
            {
                foreach (var n in notis)
                    n.IsRead = true;

                await _context.SaveChangesAsync();
            }

            return Ok();
        }
    }
}
