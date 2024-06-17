using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApartmentMangament.Models;

namespace WebApartmentMangament.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, NhanVien")]
    public class Apartment_ServiceController : Controller
    {
        private QUANLYCHUNGCUContext _context;
        public INotyfService _notyfService { get; }
        public Apartment_ServiceController(QUANLYCHUNGCUContext repo, INotyfService notyfService)
        {
            _context = repo;
            _notyfService = notyfService;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.ApartmentServices.ToListAsync());
        }
        public async Task<IActionResult> Create( int id)
        {
            var apartment = await _context.Apartments.FindAsync(id);
            ViewData["ApartmentId"] = new SelectList(_context.Apartments, "ApartmentId", "ApartmentCode", apartment?.ApartmentId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "ServiceName");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApartmentService apartmentService)
        {
            var Dichvu = _context.ApartmentServices.FirstOrDefault(x =>
            x.ApartmentId == apartmentService.ApartmentId &&
            x.ServiceId == apartmentService.ServiceId);
            if (Dichvu != null)
            {
                _notyfService.Error("Dịch vụ đã có trong căn hộ");
                return View(apartmentService);
            }
            _context.ApartmentServices.Add(apartmentService);
            await _context.SaveChangesAsync();
            _notyfService.Success("Thêm thành công");
            return RedirectToAction("Index" , "Apartment");
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ApartmentServices == null)
            {
                return NotFound();
            }
            var Dichvu = await _context.ApartmentServices.FindAsync(id);
            if (Dichvu == null)
            {
                return NotFound();
            }
            return View(Dichvu);
        }

        public async Task<IActionResult> Edit(int? id ,int di)
        {
            if (id == null || _context.ApartmentServices == null)
            {
                return NotFound();
            }
            var Dichvu = await _context.ApartmentServices.FirstOrDefaultAsync(x=>x.ApartmentId == id && x.ServiceId == di );
            if (Dichvu == null)
            {
                return NotFound();
            }
            ViewData["ApartmentId"] = new SelectList(_context.Apartments, "ApartmentId", "ApartmentCode");
            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "ServiceName");

            return View(Dichvu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ApartmentService apartmentService)
        {
 
            _context.ApartmentServices.Update(apartmentService);
            await _context.SaveChangesAsync();
            _notyfService.Success("Sửa thành công");
            return RedirectToAction("Index" , "Apartment");
        }

        public async Task<IActionResult> Delete(int? id, int di)
        {
            if (id == null || _context.ApartmentServices == null)
            {
                return NotFound();
            }
            var Dichvu = await _context.ApartmentServices.Include(x=>x.Apartment).Include(x=>x.Service).FirstOrDefaultAsync(x => x.ApartmentId == id && x.ServiceId == di);

            if (Dichvu == null)
            {
                return NotFound();
            }
            return View(Dichvu);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id, int? di)
        {
            if (_context.ApartmentServices == null)
            {
                return Problem("Entity set 'QUANLYCHUNGCUContext.ApartmentServices'  is null.");
            }
            var Dichvu = await _context.ApartmentServices.FirstOrDefaultAsync(x => x.ApartmentId == id && x.ServiceId == di);

            if (Dichvu != null)
            {
                _context.ApartmentServices.Remove(Dichvu);
            }
            _notyfService.Success("Xóa Thành Công");
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Apartment");
        }

    }
}
