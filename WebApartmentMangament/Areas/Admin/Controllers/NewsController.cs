using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;
using WebApartmentMangament.Models;
using UnidecodeSharpFork;
using WebBaiGiang_CKC.Helper;

using System.Security.Principal;
using Microsoft.AspNetCore.Authorization;

namespace WebApartmentMangament.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, NhanVien")]
    public class NewsController : Controller
    {
        private QUANLYCHUNGCUContext _context;
        public INotyfService _notyfService { get; }
        public static string image;
        public NewsController(QUANLYCHUNGCUContext repo, INotyfService notyfService)
        {
            _context = repo;
            _notyfService = notyfService;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.News.ToListAsync());
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Models.News news, IFormFile fAvatar)
        {
            var toanha = _context.News.FirstOrDefault(x => x.Title == news.Title);
            if (toanha != null)
            {
                _notyfService.Error("Tiêu đề đã tồn tại");
                return View();
            }
         
            news.CreateDay = DateTime.Now;
            news.Slug = ConvertToSlug(news.Title);
            if (fAvatar != null)
            {
                string extennsion = Path.GetExtension(fAvatar.FileName);
                image = Utilities.ToUrlFriendly(news.Slug) + extennsion;
                news.Image = await Utilities.UploadFile(fAvatar, @"News", image.ToLower());
            }
            news.Status = 1;
            _context.Add(news);
            await _context.SaveChangesAsync();
            _notyfService.Success("Thêm thành công");
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.News == null)
            {
                return NotFound();
            }
            var newss = await _context.News.FindAsync(id);
            if (newss == null)
            {
                return NotFound();
            }
            return View(newss);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.News == null)
            {
                return NotFound();
            }
            var newss = await _context.News.FindAsync(id);
            if (newss == null)
            {
                return NotFound();
            }
            return View(newss);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Models.News news , IFormFile fAvatar)
        {
            var new_db = _context.News.FirstOrDefault(x => x.NewsId != news.NewsId && x.Title == news.Title);
            if (new_db != null)
            {
                _notyfService.Error("Tiêu đề đã tồn tại đã tồn tại");
                return View(news);
            }
            new_db.Title = news.Title;
            new_db.Slug = ConvertToSlug(news.Title);
            new_db.Status = 1;
            if (fAvatar != null)
            {
                string extennsion = Path.GetExtension(fAvatar.FileName);
                image = Utilities.ToUrlFriendly(news.Slug) + extennsion;
                new_db.Image = await Utilities.UploadFile(fAvatar, @"User", image.ToLower());
            }
            else
            {
                news.Image = _context.News.Where(x => x.NewsId == news.NewsId).Select(x => x.Image).FirstOrDefault();
            }
            _context.Update(new_db);
            await _context.SaveChangesAsync();
            _notyfService.Success("Sửa thành công");
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.News == null)
            {
                return NotFound();
            }
            var newss = await _context.News.FindAsync(id);
            if (newss == null)
            {
                return NotFound();
            }
            return View(newss);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.News == null)
            {
                return Problem("Entity set 'QUANLYCHUNGCUContext.News'  is null.");
            }
            var newss = await _context.News.FindAsync(id);
            if (newss != null)
            {
                _context.News.Remove(newss);
            }
            _notyfService.Success("Xóa Thành Công");
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public string ConvertToSlug(string title)
        {
            // Chuyển đổi chuỗi sang không dấu
            string slug = title.Unidecode();

            // Xóa các ký tự không phải chữ cái, số, hoặc dấu gạch ngang
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

            // Thay thế khoảng trắng bằng dấu gạch ngang
            slug = slug.Replace(" ", "-").Trim();

            // Xóa các dấu gạch ngang liên tiếp
            slug = Regex.Replace(slug, @"-+", "-");

            // Xóa dấu gạch ngang ở đầu và cuối chuỗi
            slug = slug.Trim('-');

            return slug;
        }
    }
}
