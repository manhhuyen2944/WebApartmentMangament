using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApartmentMangament.Models;

namespace WebApartmentMangament.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, NhanVien")]
    public class HomeController : Controller
    {
        private QUANLYCHUNGCUContext _context;
        public INotyfService _notyfService { get; }

        public HomeController(QUANLYCHUNGCUContext repo, INotyfService notyfService)
        {
            _context = repo;
            _notyfService = notyfService;
        }
        [Route("admin")]
        public async Task<IActionResult> Index()
        {
            var socudan = await _context.Accounts.Where(x=>x.Status == 1).ToListAsync();
            ViewBag.CuDan = socudan.Count();
            var CanHo = await _context.Apartments.ToListAsync();
            ViewBag.CanHo = CanHo.Count();
            var ToaNha = await _context.Buildings.Where(x => x.Status == 1).ToListAsync();
            ViewBag.ToaNha = ToaNha.Count();
            return View();
        }
    }
}
