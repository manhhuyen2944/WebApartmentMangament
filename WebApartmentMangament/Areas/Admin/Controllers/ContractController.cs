using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApartmentMangament.Models;

namespace WebApartmentMangament.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, NhanVien")]
    public class ContractController : Controller
    {
        private QUANLYCHUNGCUContext _context;
        public INotyfService _notyfService { get; }
        public ContractController(QUANLYCHUNGCUContext repo, INotyfService notyfService)
        {
            _context = repo;
            _notyfService = notyfService;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Contracts.Include(x=>x.Apartment).Include(x=>x.Account).ThenInclude(x=>x.Info).ToListAsync());
        }
        public async Task<IActionResult> Create()
        {

            ViewData["ApartmentId"] = new SelectList(_context.Apartments.Where(x => x.Status == 1), "ApartmentId", "ApartmentCode");
            ViewData["AccountId"] = new SelectList(_context.Accounts.Include(x => x.Info).Where(x => x.InfoId != null), "AccountId", "Info.FullName");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contract contract)
        {
            var HopDong = _context.Contracts.FirstOrDefault(
                x => x.ApartmentId == contract.ApartmentId && x.AccountId == contract.AccountId);
            if (HopDong != null)
            {
                _notyfService.Error("Căn hộ đã có chủ");
                ViewData["ApartmentId"] = new SelectList(_context.Apartments.Where(x => x.Status == 1), "ApartmentId", "ApartmentCode");
                ViewData["AccountId"] = new SelectList(_context.Accounts.Include(x => x.Info).Where(x => x.InfoId != null), "AccountId", "Info.FullName");
                return View();
            }
            _context.Add(contract);
            await _context.SaveChangesAsync();
            _notyfService.Success("Thêm thành công");
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Contracts == null)
            {
                return NotFound();
            }
            var HopDong = await _context.Contracts.FindAsync(id);
            if (HopDong == null)
            {
                return NotFound();
            }
            return View(HopDong);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Contracts == null)
            {
                return NotFound();
            }
            var HopDong = await _context.Contracts.FindAsync(id);
            if (HopDong == null)
            {
                return NotFound();
            }
            ViewData["ApartmentId"] = new SelectList(_context.Apartments.Where(x => x.Status == 1), "ApartmentId", "ApartmentCode");
            ViewData["AccountId"] = new SelectList(_context.Accounts.Include(x => x.Info).Where(x => x.InfoId != null), "AccountId", "Info.FullName");
            return View(HopDong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Contract contract)
        {
            _context.Update(contract);
            await _context.SaveChangesAsync();
            _notyfService.Success("Sửa thành công");
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Contracts == null)
            {
                return NotFound();
            }
            var HopDong = await _context.Contracts.FindAsync(id);
            if (HopDong == null)
            {
                return NotFound();
            }
            return View(HopDong);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Contracts == null)
            {
                return Problem("Entity set 'QUANLYCHUNGCUContext.Contracts'  is null.");
            }
            var HopDong = await _context.Contracts.FindAsync(id);
            if (HopDong != null)
            {
                _context.Contracts.Remove(HopDong);
            }
            _notyfService.Success("Xóa Thành Công");
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
