using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using AspNetCoreHero.ToastNotification.Abstractions;
using WebApartmentMangament.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebBaiGiang_CKC.Extension;
using Microsoft.EntityFrameworkCore.Migrations;
using WebBaiGiang_CKC.Helper;

namespace WebApartmentMangament.Controllers
{
    public class AccountController : Controller
    {
        private QUANLYCHUNGCUContext _context; public static string image;
        public INotyfService _notyfService { get; }
        public AccountController(QUANLYCHUNGCUContext repo, INotyfService notyfService)
        {
            _context = repo;
            _notyfService = notyfService;
        }

        public IActionResult Index()
        {
            return View();
        }
		public async Task< IActionResult> CanHo()
        {
            var Idclam = User.Claims.SingleOrDefault(c => c.Type == "Id");
            int Id = 0;
            if (Idclam != null)
            { Id = Int32.Parse(Idclam.Value); }
            var canho = await _context.Apartments
                .Include(x => x.Building)
                .Include(x => x.WaterMeters)
                .Include(x => x.ElectricMeters)
                .Include(x => x.Contracts)
                .Include(x => x.ApartmentServices)
                .ThenInclude(x => x.Service)
                .FirstOrDefaultAsync(x => x.Accounts.FirstOrDefault().AccountId == Id);
              var thanhvien =await _context.Accounts.Include(x=>x.Info).Where(x=>x.ApartmentId == canho.ApartmentId).ToListAsync();
            ViewBag.Account = thanhvien;
            return View(canho);
		}
		public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string user, string pass)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var password = pass.ToMD5();
            //var anh = _context.GiaoVien.ToList();
            // Kiểm tra tên đăng nhập và mật khẩu
            var account = await _context.Accounts.Include(x => x.Info).FirstOrDefaultAsync(u => u.UserName == user && u.Password == password);

            if (account == null)
            {
                // Tên đăng nhập hoặc mật khẩu không đúng
                _notyfService.Error("Thông tin đăng nhập không chính xác");
                return RedirectToAction("Index", "Home");
            }
            if (account?.RoleId == 1 || account?.RoleId == 2)
            {
                _notyfService.Error("Tài khoản của bạn là tài khoản Admin");
                return RedirectToAction("Index", "Home");
            }
            if (account?.Status == 2)
            {
                _notyfService.Error("Tài khoản đã bị khóa");
                return RedirectToAction("Index", "Home");
            }
            if (account != null)
            {
                // Lưu thông tin người dùng vào cookie xác thực
                List<Claim> claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, account.Info.FullName),
                        new Claim("UserName" , account.UserName),
                        new Claim("Id" , account.AccountId.ToString()),
                         new Claim("Avartar", "/contents/Images/User/" + account.Avartar) // Thêm đường dẫn đến ảnh đại diện vào claims
                    };
                //   Response.Cookies.Append("AnhDaiDien", "Images/GiaoVien/" + user.AnhDaiDien);
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                _notyfService.Success("Đăng nhập thành công");
                // Chuyển hướng đến trang chủ
                return RedirectToAction("Index", "Home");
            }
            else
            {
                _notyfService.Warning("Tên đăng nhập hoặc mật khẩu không đúng");
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _notyfService.Success("Đăng xuất thành công");
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string pass, string newpass, string confirmpass)
        {
            if (ModelState.IsValid)
            {
                var tendangnhapclam = User.Claims.SingleOrDefault(c => c.Type == "UserName");
                var tendangnhap = "";
                if (tendangnhapclam != null)
                { tendangnhap = tendangnhapclam.Value; }
                var password = pass.ToMD5();
                var user = await _context.Accounts.FirstOrDefaultAsync(u => u.UserName == tendangnhap);
                if (user?.Password != password)
                {
                    _notyfService.Error("Mật khẩu cũ không chính xác");
                    return RedirectToAction("Index", "Home");
                }
                if (newpass.Length < 6 && newpass.Length < 100)
                {
                    _notyfService.Error("Mật khẩu mới phải trên 6 ký tự và nhỏ hơn 100 ký tự ");
                    return RedirectToAction("Index", "Home");
                }
                if (newpass != confirmpass)
                {
                    _notyfService.Error("Mật khẩu mới không đúng với mật khẩu xác nhận !");
                    return RedirectToAction("Index", "Home");
                }
                user.Password = newpass.ToMD5();
                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                _notyfService.Error("Vui lòng nhập đầy đủ thông mật khẩu !");

            }
            _notyfService.Success("Đổi mật khẩu thành công!");
            return RedirectToAction("Index", "Home");
        }

   
        public async Task<IActionResult> Profile()
        {
            var Idclam = User.Claims.SingleOrDefault(c => c.Type == "Id");
            int Id = 0;
            if (Idclam != null)
            { Id = Int32.Parse(Idclam.Value); }
          
            return View(await _context.Accounts.Include(x => x.Apartment).Include(x => x.Relationship).Include(x => x.Info).FirstOrDefaultAsync(x => x.AccountId == Id));
        }

        public async Task<IActionResult> EditProfile()
        {
            var Idclam = User.Claims.SingleOrDefault(c => c.Type == "Id");
            int Id = 0;
            if (Idclam != null)
            { Id = Int32.Parse(Idclam.Value); }

            return View(await _context.Accounts.Include(x => x.Apartment).Include(x => x.Relationship).Include(x => x.Info).FirstOrDefaultAsync(x => x.AccountId == Id));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

		public async Task<IActionResult> EditProfile(Account account, IFormFile fAvatar)
		{
            var cudan = await _context.Accounts.Include(x=>x.Info).FirstOrDefaultAsync(x=>x.AccountId == account.AccountId);
            if(cudan == null)
            {
                return NotFound();
			}
			var ktemail = await _context.Accounts.FirstOrDefaultAsync(x => x.AccountId != account.AccountId
				&& (x.Email == account.Email));
			if (ktemail != null)
			{
				_notyfService.Error("Email đã tồn tại trong hệ thống!");
				return View(cudan);
			}
			if (fAvatar != null)
			{
				string extennsion = Path.GetExtension(fAvatar.FileName);
				image = Utilities.ToUrlFriendly(cudan.UserName) + extennsion;
				cudan.Avartar = await Utilities.UploadFile(fAvatar, @"User", image.ToLower());
			}
			else
			{
				account.Avartar = _context.Accounts.Where(x => x.AccountId == account.AccountId).Select(x => x.Avartar).FirstOrDefault();
			}

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Lưu thông tin người dùng vào cookie xác thực
            List<Claim> claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, account.Info.FullName),
                        new Claim("UserName" , account.UserName),
                        new Claim("Id" , account.AccountId.ToString()),
                         new Claim("Avartar", "/contents/Images/User/" + cudan.Avartar) // Thêm đường dẫn đến ảnh đại diện vào claims
                    };
            //   Response.Cookies.Append("AnhDaiDien", "Images/GiaoVien/" + user.AnhDaiDien);
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            cudan.Info = account.Info;
			cudan.Email = account.Email;
			_notyfService.Success("Sửa thành công!");
			await _context.SaveChangesAsync();
			return RedirectToAction("Index", "Home");

		}

	}
}
