using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCUOI.Data;
using WebCUOI.Models;
using WebCUOI.Services;

public class FeedbackController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly NotificationService _notify;

    public FeedbackController(ApplicationDbContext context, NotificationService notify)
    {
        _context = context;
        _notify = notify;
    }

    // ✅ TRANG ADMIN XEM TẤT CẢ PHẢN HỒI
    public IActionResult Index()
    {
        var list = _context.Feedbacks
            .Include(f => f.User)
            .OrderByDescending(f => f.CreatedAt)
            .ToList();

        return View(list);
    }


    [HttpPost]
    public async Task<IActionResult> Create(string message, string? UserName)
    {
        var fb = new Feedback
        {
            Message = message,
            CreatedAt = DateTime.Now
        };

        // Nếu có tài khoản đăng nhập
        if (User.Identity.IsAuthenticated)
        {
            fb.UserId = User.Identity.Name;
        }
        else
        {
            // Nếu khách vãng lai thì lưu tên nhập tay
            fb.UserId = UserName ?? "Khách vãng lai";
        }

        _context.Feedbacks.Add(fb);
        await _context.SaveChangesAsync();

        await _notify.CreateAsync("ADMIN", "Có phản hồi mới từ khách hàng!");

        return RedirectToAction("Index", "Home");
    }


    // ✅ ADMIN TRẢ LỜI PHẢN HỒI
    [HttpPost]
    public async Task<IActionResult> Reply(int id, string reply)
    {
        var fb = _context.Feedbacks.Find(id);
        if (fb == null) return NotFound();

        fb.Reply = reply;
        fb.RepliedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        await _notify.CreateAsync(fb.UserId, "Phản hồi của bạn đã được trả lời.");

        return RedirectToAction("Index");  // quay lại trang danh sách để thấy reply
    }
    public IActionResult MyFeedback()
    {
        var userId = User.Identity.Name;
        var list = _context.Feedbacks
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToList();

        return View(list);
    }
}
