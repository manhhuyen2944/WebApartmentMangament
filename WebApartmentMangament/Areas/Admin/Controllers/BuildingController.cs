using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApartmentMangament.Models;

namespace WebApartmentMangament.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, NhanVien")]
    public class BuildingController : Controller
    {
        private QUANLYCHUNGCUContext _context;
        public INotyfService _notyfService { get; }
        public BuildingController(QUANLYCHUNGCUContext repo, INotyfService notyfService)
        {
            _context = repo;
            _notyfService = notyfService;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Buildings.ToListAsync());
        }
        [Authorize(Roles ="Admin")]
        public IActionResult Create()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Building building)
        {
            var toanha = _context.Buildings.FirstOrDefault(x => x.BuildingCode == building.BuildingCode);
            if (toanha != null)
            {
                _notyfService.Error("Mã tòa nhà đã tồn tại");
                return View();
            }
            building.Status = 1;
            _context.Add(building);
            await _context.SaveChangesAsync();
            _notyfService.Success("Thêm thành công");
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Buildings == null)
            {
                return NotFound();
            }
            var ToaNha = await _context.Buildings.FindAsync(id);
            if (ToaNha == null)
            {
                return NotFound();
            }
            return View(ToaNha);
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Buildings == null)
            {
                return NotFound();
            }
            var ToaNha = await _context.Buildings.FindAsync(id);
            if (ToaNha == null)
            {
                return NotFound();
            }
            return View(ToaNha);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Building building)
        {
            var toanha = _context.Buildings.FirstOrDefault(x =>x.BuildingId != building.BuildingId &&  (x.BuildingCode == building.BuildingCode || x.BuildingName == building.BuildingName));
            if (toanha != null)
            {
                _notyfService.Error("Mã hoặc tên tòa nha đã tồn tại");
                return View(building);
            }
            _context.Update(building);
            await _context.SaveChangesAsync();
            _notyfService.Success("Sửa thành công");
            return RedirectToAction("Index");
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Buildings == null)
            {
                return NotFound();
            }
            var ToaNha = await _context.Buildings.FindAsync(id);
            if (ToaNha == null)
            {
                return NotFound();
            }
            return View(ToaNha);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Buildings == null)
            {
                return Problem("Entity set 'QUANLYCHUNGCUContext.Buildings'  is null.");
            }
            var ToaNha = await _context.Buildings.FindAsync(id);
            if (ToaNha != null)
            {
                _context.Buildings.Remove(ToaNha);
            }
            _notyfService.Success("Xóa Thành Công");
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
