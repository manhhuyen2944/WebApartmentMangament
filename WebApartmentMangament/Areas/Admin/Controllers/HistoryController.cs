using Microsoft.AspNetCore.Mvc;

namespace WebApartmentMangament.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HistoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
