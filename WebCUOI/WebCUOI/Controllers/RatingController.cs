using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCUOI.Data;
using WebCUOI.Models;
using WebCUOI.Services;

public class RatingController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly NotificationService _notify;
    private readonly UserManager<IdentityUser> _userManager;

    public RatingController(ApplicationDbContext context, NotificationService notify, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _notify = notify;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Create(int serviceId)
    {
        return View(new Rating { ServiceId = serviceId });
    }

    [HttpPost]
    public async Task<IActionResult> Create(Rating model)
    {
        var user = await _userManager.GetUserAsync(User);
        model.UserId = user?.Id ?? "Guest";

        _context.Ratings.Add(model);
        await _context.SaveChangesAsync();

        await _notify.CreateAsync("ADMIN", $"Có đánh giá mới cho dịch vụ {model.ServiceId}");

        return RedirectToAction("Index");
    }

   

    public IActionResult Index()
    {
        var list = _context.Ratings
            .Include(r => r.User) // Lấy dữ liệu User
            .ToList();

        return View(list);
    }

}
