using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApartmentMangament.Models;

namespace WebApartmentMangament.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly QUANLYCHUNGCUContext _context;
        public INotyfService _notyfService { get; }
        public HomeController(ILogger<HomeController> logger, QUANLYCHUNGCUContext context,INotyfService notyfService)
        {
            _logger = logger;
            _context = context;
            _notyfService = notyfService;
        }
        public async Task<IActionResult> Index()
        {
            var bangtin = await _context.News
                .Where(x => x.Status == 1)
                .OrderByDescending(x => x.CreateDay)
                .Take(3)
                .ToListAsync();
            ViewBag.Bangtin = bangtin;
            return View();
        } 
        public IActionResult GioiThieu()
        {

            return View();
        }
        public async Task<IActionResult> TinTuc()
        {
            var bangtin = await _context.News.Where(x => x.Status == 1).ToListAsync();

            return View(bangtin);
        }
        public IActionResult LienHe()
        {

            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}