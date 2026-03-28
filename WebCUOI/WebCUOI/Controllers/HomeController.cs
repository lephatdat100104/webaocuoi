using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebCUOI.Models;

namespace WebCUOI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        
        public IActionResult Index()
        {
            // Set the title for Home Page (Page 1)
            ViewData["Title"] = "Trang Ch? - WebCUOI (Trang 1)";
            return View();
        }

        // New action for Home Page (Page 2)
        public IActionResult HomePagePart2()
        {
            // Set the title for Home Page (Page 2)
            ViewData["Title"] = "Trang Ch? - WebCUOI (Trang 2)";
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult About()
        {
            ViewData["Title"] = "Gi?i Thi?u V? Chúng Tôi"; // Corrected Vietnamese character for consistency
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}