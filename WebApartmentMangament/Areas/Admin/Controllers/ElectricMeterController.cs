using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Data;
using WebApartmentMangament.Models;
namespace WebApartmentMangament.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, NhanVien")]
    public class ElectricMeterController : Controller
    {
        private QUANLYCHUNGCUContext _context;
        public INotyfService _notyfService { get; }
        private readonly IConfiguration _configuration;
        public ElectricMeterController(QUANLYCHUNGCUContext repo, INotyfService notyfService, IConfiguration configuration)
        {
            _context = repo;
            _notyfService = notyfService;
            _configuration = configuration;
        }
        public async Task< IActionResult> Index()
        {
            return View(await _context.ElectricMeters.Include(x => x.Apartment).ToListAsync());
        }
        
        public IActionResult Create()
        {
            ViewData["ApartmentId"] = new SelectList(_context.Apartments, "ApartmentId", "ApartmentCode");
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ElectricMeter electricMeter)
        {
            var ToaNha = await _context.Apartments.FirstOrDefaultAsync(x => x.ApartmentId == electricMeter.ApartmentId);
            var DongHo = await _context.ElectricMeters.FirstOrDefaultAsync(x=>x.Code == electricMeter.Code || x.ApartmentId == electricMeter.ApartmentId);
            if (DongHo != null)
            {
                _notyfService.Error("Căn hộ hoặc đòng hồ đã tồn tại!");
                ViewData["ApartmentId"] = new SelectList(_context.Apartments, "ApartmentId", "ApartmentCode");
                return View();
            }
            electricMeter.Code = "Đ" + ((ToaNha.ApartmentCode).Substring(ToaNha.ApartmentCode.Length - 6)).ToString();
            electricMeter.DeadingDate = new DateTime(2023, 1, 30);
            electricMeter.Price = 4000;
            _context.Add(electricMeter);
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
                        var ApartmentId = row["ApartmentId"].ToString();
                        if (row["ApartmentId"].ToString() == "0")
                        {
                            _notyfService.Error("Căn hộ chưa được tạo !");
                            return RedirectToAction("Index");
                        }

                        if (!ChuongIds.Contains(Convert.ToInt32(kiemtra(ApartmentId))))
                        {
                            _notyfService.Warning("Căn hộ chưa được tạo !");
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
                            sqlBulkCopy.DestinationTableName = "ElectricMeter";
                            sqlBulkCopy.ColumnMappings.Add("ApartmentId", "ApartmentId");
                            sqlBulkCopy.ColumnMappings.Add("Code", "Code");
                            sqlBulkCopy.ColumnMappings.Add("RegistrationDate", "RegistrationDate");
                            sqlBulkCopy.ColumnMappings.Add("NumberOne", "NumberOne");
                            sqlBulkCopy.ColumnMappings.Add("NumberEnd", "NumberEnd");
                            sqlBulkCopy.ColumnMappings.Add("Price", "Price");
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
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ElectricMeters == null)
            {
                return NotFound();
            }
            var DongHo = await _context.ElectricMeters.FindAsync(id);
            if (DongHo == null)
            {
                return NotFound();
            }
            return View(DongHo);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ElectricMeters == null)
            {
                return NotFound();
            }
            var DongHo = await _context.ElectricMeters.FindAsync(id);
            if (DongHo == null)
            {
                return NotFound();
            }
            ViewData["ApartmentId"] = new SelectList(_context.Apartments, "ApartmentId", "ApartmentCode");
            return View(DongHo);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ElectricMeter electricMeter)
        {
            var DongHo = await _context.ElectricMeters.FirstOrDefaultAsync(
                x => (x.Code == electricMeter.Code ||
                x.ApartmentId == electricMeter.ApartmentId) && x.ElectricMeterId != electricMeter.ElectricMeterId);
            if (DongHo != null)
            {
                _notyfService.Error("Căn hộ hoặc đòng hồ đã tồn tại!");
                ViewData["ApartmentId"] = new SelectList(_context.Apartments, "ApartmentId", "ApartmentCode");
                return View();
            }
            _context.Update(electricMeter);
            await _context.SaveChangesAsync();
            _notyfService.Success("Sửa thành công");
            return RedirectToAction("Index");
        }
        
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ElectricMeters == null)
            {
                return NotFound();
            }
            var DongHo = await _context.ElectricMeters.FindAsync(id);
            if (DongHo == null)
            {
                return NotFound();
            }
            return View(DongHo);
        }
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ElectricMeters == null)
            {
                return Problem("Entity set 'QUANLYCHUNGCUContext.ElectricMeters'  is null.");
            }
            var DongHo = await _context.ElectricMeters.FindAsync(id);
            if (DongHo != null)
            {
                _context.ElectricMeters.Remove(DongHo);
            }
            _notyfService.Success("Xóa Thành Công");
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
