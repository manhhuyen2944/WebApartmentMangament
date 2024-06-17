using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;
using WebBaiGiang_CKC.Extension;
using WebBaiGiang_CKC.Helper;
using WebApartmentMangament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using System.Data;
using DocumentFormat.OpenXml.EMMA;

namespace WebApartmentMangament.Areas.Admin.Controllers
{
    [Area("Admin")]

    public class AccountController : Controller
    {
        private QUANLYCHUNGCUContext _context; public static string? image;
        public INotyfService _notyfService { get; }
        private readonly IConfiguration _configuration;
        public AccountController(QUANLYCHUNGCUContext repo, INotyfService notyfService, IConfiguration configuration)
        {
            _context = repo;
            _notyfService = notyfService;
            _configuration = configuration;
        }
        [Authorize(Roles = "Admin, NhanVien")]
        //CRUD CU DAN 
        [Route("CuDan")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Accounts.Include(x => x.Apartment).Include(x => x.Relationship).Include(x => x.Info).Where(x => x.RoleId == 3).ToListAsync());
        }
        [Authorize(Roles = "Admin, NhanVien")]
        public IActionResult Create()
        {
            ViewData["ApartmentId"] = new SelectList(_context.Apartments, "ApartmentId", "ApartmentCode");
            return View();
        }
        [Authorize(Roles = "Admin, NhanVien")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Account account, IFormFile fAvatar)
        {
            var mk = "123123";
            var CanHo = await _context.Apartments.FirstOrDefaultAsync(
                x => x.ApartmentId == account.ApartmentId);

            if (CanHo == null)
            {
                return NotFound();
            }

            var dsCuDan = _context.Accounts.Where(x => x.ApartmentId == account.ApartmentId);
            int socudan = dsCuDan.Count() + 1;
            account.Code = "CD" + CanHo?.ApartmentCode?.Substring(CanHo.ApartmentCode.Length - 4) + (socudan < 10 ? "0" : "") + socudan;
            account.UserName = account.Code;
            if (fAvatar != null)
            {
                string extennsion = Path.GetExtension(fAvatar.FileName);
                image = Utilities.ToUrlFriendly(account.UserName) + extennsion;
                account.Avartar = await Utilities.UploadFile(fAvatar, @"User", image.ToLower());
            }
            account.Password = mk.ToMD5();
            account.RoleId = 3;
            account.RelationshipId = 2;
            account.Info.Country = "Việt Nam";
            account.Info.FullName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(account.Info.FullName);
            _context.Add(account);
            await _context.SaveChangesAsync();
            _notyfService.Success("Thêm thành công");
            return RedirectToAction("Index");
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult CreateList(IFormFile formFile)
        {
            try
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                List<int> ChuongIds = new List<int>();
                string ConString = _configuration.GetConnectionString("QuanLyChungCu");
                using (SqlConnection con = new SqlConnection(ConString))
                {
                    string query = "SELECT ApartmentId FROM APARTMENT";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ChuongIds.Add(reader.GetInt32(reader.GetOrdinal("ApartmentId")));
                            }
                        }
                        con.Close();
                    }
                }
                var mainPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Files");
                if (!Directory.Exists(mainPath))
                {
                    Directory.CreateDirectory(mainPath);
                }

                var filePath = Path.Combine(mainPath, $"{Guid.NewGuid()}{Path.GetExtension(formFile.FileName)}");

                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    formFile.CopyTo(stream);
                }

                FileInfo fileInfo = new FileInfo(filePath);
                using (ExcelPackage package = new ExcelPackage(fileInfo))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    DataTable dt = new DataTable();
                    foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
                    {
                        dt.Columns.Add(firstRowCell.Text);
                    }
                    for (var rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                    {
                        var row = worksheet.Cells[rowNumber, 1, rowNumber, worksheet.Dimension.End.Column];
                        var newRow = dt.Rows.Add();
                        foreach (var cell in row)
                        {
                            newRow[cell.Start.Column - 1] = cell.Text;
                        }
                    }
                    // Thực hiện các kiểm tra và chuẩn hóa dữ liệu
                    bool hasInvalidChuongHoc = false;
                    foreach (DataRow row in dt.Rows)
                    {
                        var status = row["Status"].ToString();
                        var Password = row["Password"].ToString();
                        var RoleId = row["RoleId"].ToString();
                        var RelationshipId = row["RelationshipId"].ToString();
                        var ApartmentId = row["ApartmentId"].ToString();

                        row["Password"] = Password.ToString().ToMD5();
                        if (row["ApartmentId"].ToString() == "0")
                        {
                            _notyfService.Error("Căn hộ chưa được tạo !");
                            return RedirectToAction("Index");
                        }

                        if (!ChuongIds.Contains(Convert.ToInt32(kiemtra(ApartmentId))))
                        {
                            _notyfService.Warning("Tòa nhà chưa được tạo !");
                            hasInvalidChuongHoc = true;
                            break;
                        }
                        row["ApartmentId"] = kiemtra(ApartmentId);
                        if (status == "Hoạt động")
                        {
                            row["Status"] = 1;
                        }
                        if (status == "Ngừng hoạt động")
                        {
                            row["Status"] = 2;

                        }
                        if (RelationshipId == "Khách Thuê")
                        {
                            row["RelationshipId"] = 2;
                        }
                        else
                        {
                            _notyfService.Error("Sai quan hệ người dùng");
                            return RedirectToAction("Index");
                        }
                        if (RoleId == "Cư Dân")
                        {
                            row["RoleId"] = 3;
                        }
                        else
                        {
                            _notyfService.Error("Sai quan hệ người dùng");
                            return RedirectToAction("Index");
                        }



                    }

                    if (hasInvalidChuongHoc)
                    {
                        return RedirectToAction("Index");
                    }
                    var conString = _configuration.GetConnectionString("QuanLyChungCu");

                    using (SqlConnection con = new SqlConnection(conString))
                    {
                        using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
                        {
                            sqlBulkCopy.DestinationTableName = "Account";
                            sqlBulkCopy.ColumnMappings.Add("ApartmentId", "ApartmentId");
                            sqlBulkCopy.ColumnMappings.Add("Code", "Code");
                            sqlBulkCopy.ColumnMappings.Add("UserName", "UserName");
                            sqlBulkCopy.ColumnMappings.Add("Password", "Password");
                            sqlBulkCopy.ColumnMappings.Add("Email", "Email");
                            sqlBulkCopy.ColumnMappings.Add("RoleId", "RoleId");
                            sqlBulkCopy.ColumnMappings.Add("RelationshipId", "RelationshipId");
                            sqlBulkCopy.ColumnMappings.Add("Status", "Status");

                            con.Open();
                            sqlBulkCopy.WriteToServer(dt);
                            con.Close();
                        }
                    }
                    _notyfService.Success("Thêm Thành Công!");
                    return RedirectToAction("Index");
                }
            }
            catch (Exception)
            {
                _notyfService.Error("Thêm Thất Bại!");
            }
            return RedirectToAction("Index");
        }
        private int kiemtra(string? phong)
        {
            var canho = _context.Apartments.FirstOrDefault(x => x.ApartmentCode == phong);
            if (canho == null)
            {
                return 0;

            }
            int canhoso = canho.ApartmentId;
            return canhoso; // Trả về giá trị không hợp lệ nếu không tìm thấy số
        }

        [Authorize(Roles = "Admin, NhanVien")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }
            var cudan = await _context.Accounts.Include(x => x.Relationship).Include(x => x.Apartment).Include(x => x.Info).FirstOrDefaultAsync(x => x.AccountId == id);
            if (cudan == null)
            {
                return NotFound();
            }

            return View(cudan);
        }
        [Authorize(Roles = "Admin, NhanVien")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }
            var cudan = await _context.Accounts.Include(x => x.Relationship).Include(x => x.Apartment).Include(x => x.Info).FirstOrDefaultAsync(x => x.AccountId == id);
            if (cudan == null)
            {
                return NotFound();
            }
            ViewData["ApartmentId"] = new SelectList(_context.Apartments, "ApartmentId", "ApartmentCode");
            ViewData["RelationshipId"] = new SelectList(_context.Relationships, "RelationshipId", "RelationshipName");
            return View(cudan);
        }
        [Authorize(Roles = "Admin, NhanVien")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Account account, IFormFile fAvatar)
        {
            try
            {
                var cudanUp = await _context.Accounts.Include(x => x.Info).FirstOrDefaultAsync(x => x.AccountId == account.AccountId);
                if (cudanUp == null)
                {
                    return NotFound();
                }

                if (fAvatar != null)
                {
                    string extennsion = Path.GetExtension(fAvatar.FileName);
                    image = Utilities.ToUrlFriendly(account.UserName) + extennsion;
                    cudanUp.Avartar = await Utilities.UploadFile(fAvatar, @"User", image.ToLower());
                }
                else
                {
                    account.Avartar = _context.Accounts.Where(x => x.AccountId == account.AccountId).Select(x => x.Avartar).FirstOrDefault();
                }
                if (cudanUp.InfoId == null)
                {
                    // Tạo một đối tượng Info mới
                    var newInfo = new InFo
                    {
                        FullName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(account.Info.FullName),
                        BirthDay = account.Info.BirthDay,
                        StreetAddress = account.Info.StreetAddress,
                        Sex = account.Info.Sex,
                        Ward = account.Info.Ward,
                        District = account.Info.District,
                        City = account.Info.City,
                        Country = account.Info.Country,
                    };
                   
                    // Lưu đối tượng Info mới vào cơ sở dữ liệu
                    _context.Add(newInfo);
                    await _context.SaveChangesAsync();

                    // Gán InfoId của đối tượng Info mới tạo cho cudanUp.InfoId
                    cudanUp.InfoId = newInfo.InfoId;

                }
                else
                {
                    cudanUp.Info = account.Info;
                    cudanUp.Info.FullName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(account.Info.FullName);
                    cudanUp.Info.BirthDay = account.Info.BirthDay;
                }
                cudanUp.Email = account.Email;

                cudanUp.Status = account.Status;
                cudanUp.UserName = account.UserName;

                cudanUp.RelationshipId = account.RelationshipId;
                cudanUp.ApartmentId = account.ApartmentId;
              
                var ktCanHo = await _context.Accounts.FirstOrDefaultAsync(x => x.AccountId != account.AccountId && x.ApartmentId == account.ApartmentId);
                if (ktCanHo == null)
                {
                    cudanUp.RelationshipId = account.RelationshipId;
                }
                else
                {
                    if (ktCanHo.RelationshipId == 1)
                    {
                        cudanUp.RelationshipId = 2;
                    }
                }

                var ktemail = await _context.Accounts.FirstOrDefaultAsync(x => x.AccountId != account.AccountId && (x.Email == account.Email ));
                if (ktemail != null)
                {
                    ViewData["ApartmentId"] = new SelectList(_context.Apartments, "ApartmentId", "ApartmentCode");
                    ViewData["RelationshipId"] = new SelectList(_context.Relationships, "RelationshipId", "RelationshipName");
                    _notyfService.Error("Email hay tên đăng nhập đã tồn tại trong hệ thống!");
                    return View(account);
                }

                _notyfService.Success("Sửa thành công!");
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccountsExists(account.AccountId))
                {
                    _notyfService.Error("Lỗi!!!!!!!!!!!!");
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction("Index");
        }
        [Authorize(Roles = "Admin, NhanVien")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }
            var cudan = await _context.Accounts.Include(x => x.Relationship).Include(x => x.Apartment).Include(x => x.Info).FirstOrDefaultAsync(x => x.AccountId == id);
            if (cudan == null)
            {
                return NotFound();
            }

            return View(cudan);
        }
        [Authorize(Roles = "Admin, NhanVien")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cudan = await _context.Accounts.FindAsync(id);
            if (cudan != null)
            {
                _context.Accounts.Remove(cudan);
                var info = await _context.InFos.FindAsync(cudan.InfoId);
                if (info != null)
                {
                    _context.InFos.Remove(info);
                }
            }
            _notyfService.Success("Xóa Thành Công");
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AccountsExists(int id)
        {
            return _context.Accounts.Any(e => e.AccountId == id);
        }

        //======================================== BANG QUẢN LÝ 
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage()
        {
            return View(await _context.Accounts.Include(x => x.Info).Include(x => x.Role).Where(x => (x.RoleId == 1 || x.RoleId == 2) && x.Status == 1).ToListAsync());

        }
        [Authorize(Roles = "Admin")]
        public IActionResult CreateQL()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQL(Account account, IFormFile fAvatar)
        {
            var mk = "123123";
            var dsCuDan = _context.Accounts.Where(x => x.RoleId == 1 || x.RoleId == 2);
            int socudan = dsCuDan.Count() + 1;
            account.Code = "QL7979" + (socudan < 10 ? "0" : "") + socudan;
            account.UserName = account.Code;
            if (fAvatar != null)
            {
                string extennsion = Path.GetExtension(fAvatar.FileName);
                image = Utilities.ToUrlFriendly(account.UserName) + extennsion;
                account.Avartar = await Utilities.UploadFile(fAvatar, @"Admin", image.ToLower());
            }
            account.Password = mk.ToMD5();
            account.Info.Country = "Việt Nam";
            account.Info.FullName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(account.Info.FullName);
            _context.Add(account);
            await _context.SaveChangesAsync();
            _notyfService.Success("Thêm thành công");
            return RedirectToAction("Manage");
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DetailsQL(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }
            var cudan = await _context.Accounts.Include(x => x.Info).Include(x => x.Role).FirstOrDefaultAsync(x => x.AccountId == id);
            if (cudan == null)
            {
                return NotFound();
            }

            return View(cudan);
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditQL(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }
            var cudan = await _context.Accounts.Include(x => x.Info).Include(x => x.Role).FirstOrDefaultAsync(x => x.AccountId == id);
            if (cudan == null)
            {
                return NotFound();
            }

            return View(cudan);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQL(Account account, IFormFile fAvatar)
        {
            try
            {
                var cudanUp = await _context.Accounts.Include(x => x.Info).FirstOrDefaultAsync(x => x.AccountId == account.AccountId);
                if (cudanUp == null)
                {
                    return NotFound();
                }

                if (fAvatar != null)
                {
                    string extennsion = Path.GetExtension(fAvatar.FileName);
                    image = Utilities.ToUrlFriendly(account.UserName) + extennsion;
                    cudanUp.Avartar = await Utilities.UploadFile(fAvatar, @"Admin", image.ToLower());
                }
                else
                {
                    account.Avartar = _context.Accounts.Where(x => x.AccountId == account.AccountId).Select(x => x.Avartar).FirstOrDefault();
                }
                cudanUp.Email = account.Email;
                cudanUp.Info.FullName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(account.Info.FullName);
                cudanUp.Status = account.Status;
                cudanUp.UserName = account.UserName;
                cudanUp.Info.BirthDay = account.Info.BirthDay;

                var ktemail = await _context.Accounts.FirstOrDefaultAsync(x => x.AccountId != account.AccountId
                && (x.Email == account.Email || x.UserName == account.UserName));
                if (ktemail != null)
                {
                    _notyfService.Error("Email hay tên đăng nhập đã tồn tại trong hệ thống!");
                    return View(account);
                }

                _notyfService.Success("Sửa thành công!");
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccountsExists(account.AccountId))
                {
                    _notyfService.Error("Lỗi!!!!!!!!!!!!");
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction("Manage");
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteQL(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }
            var cudan = await _context.Accounts.Include(x => x.Info).Include(x => x.Role).FirstOrDefaultAsync(x => x.AccountId == id);
            if (cudan == null)
            {
                return NotFound();
            }

            return View(cudan);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("DeleteQL")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedQL(int id)
        {
            var Idclam = User.Claims.SingleOrDefault(c => c.Type == "Id");
            int Id = 0;
            if (Idclam != null)
            { Id = Int32.Parse(Idclam.Value); }
            if (Id == id)
            {
                _notyfService.Error("Không thể xóa tài khoản của bạn");
                return RedirectToAction(nameof(Manage));
            }
            var cudan = await _context.Accounts.FindAsync(id);
            if (cudan != null)
            {
                //_context.Accounts.Remove(cudan);
                //var info = await _context.InFos.FindAsync(cudan.InfoId);
                //if (info != null)
                //{
                //    _context.InFos.Remove(info);
                //}
                cudan.Status = 2;
            }
            _notyfService.Success("Xóa Thành Công");
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Manage));
        }

        //=========================== ĐĂNG NHẬP , ĐĂNG KÝ , QUÊN MK  

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string user, string pass)
        {
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
            if (account?.RoleId == 3)
            {
                _notyfService.Error("Tài khoản của bạn là tài khoản người dùng");
                return RedirectToAction("Index", "Home");
            }
            if (account.Status == 2)
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
                        account.RoleId == 1 ? new Claim(ClaimTypes.Role, "Admin") : new Claim(ClaimTypes.Role, "NhanVien"),
                        new Claim("UserName" , account.UserName),
                        new Claim("Id" , account.AccountId.ToString()),
                         new Claim("Avartar", "/contents/Images/Admin/" + account.Avartar) // Thêm đường dẫn đến ảnh đại diện vào claims
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



    }
}
