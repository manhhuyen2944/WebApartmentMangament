using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApartmentMangament.Models;

namespace WebApartmentMangament.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, NhanVien")]
    public class ServiceController : Controller
    {
        private QUANLYCHUNGCUContext _context;
        public INotyfService _notyfService { get; }
        public ServiceController(QUANLYCHUNGCUContext repo, INotyfService notyfService)
        {
            _context = repo;
            _notyfService = notyfService;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Services.ToListAsync());
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            var Dichvu = _context.Services.FirstOrDefault(x => x.ServiceName == service.ServiceName);
            if (Dichvu != null)
            {
                _notyfService.Error("Dịch vụ đã tồn tại");
                return View();
            }
           
            _context.Add(service);
            await _context.SaveChangesAsync();
            _notyfService.Success("Thêm thành công");
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Services == null)
            {
                return NotFound();
            }
            var Dichvu = await _context.Services.FindAsync(id);
            if (Dichvu == null)
            {
                return NotFound();
            }
            return View(Dichvu);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Services == null)
            {
                return NotFound();
            }
            var Dichvu = await _context.Services.FindAsync(id);
            if (Dichvu == null)
            {
                return NotFound();
            }
            return View(Dichvu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Service service)
        {
            var toanha = _context.Services.FirstOrDefault(x => x.ServiceId != service.ServiceId
            &&  x.ServiceName == service.ServiceName);
            if (toanha != null)
            {
                _notyfService.Error("Trùng với tên dịch vụ khác");
                return View(service);
            }
            _context.Update(service);
            await _context.SaveChangesAsync();
            _notyfService.Success("Sửa thành công");
            return RedirectToAction("Index");
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Services == null)
            {
                return NotFound();
            }
            var Dichvu = await _context.Services.FindAsync(id);
            if (Dichvu == null)
            {
                return NotFound();
            }
            return View(Dichvu);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Services == null)
            {
                return Problem("Entity set 'QUANLYCHUNGCUContext.Services'  is null.");
            }
            var Dichvu = await _context.Services.FindAsync(id);
            if (Dichvu != null)
            {
                _context.Services.Remove(Dichvu);
            }
            _notyfService.Success("Xóa Thành Công");
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
