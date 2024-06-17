using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using WebBaiGiang_CKC.Extension;
using WebApartmentMangament.Models;

namespace WebApartmentMangament.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, NhanVien")]
    public class ApartmentController : Controller
    {
        private QUANLYCHUNGCUContext _context;
        public INotyfService _notyfService { get; }
        private readonly IConfiguration _configuration;
        public ApartmentController(QUANLYCHUNGCUContext repo, INotyfService notyfService, IConfiguration configuration)
        {
            _context = repo;
            _notyfService = notyfService;
            _configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Apartments.Include(x => x.Building).ToListAsync());
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            ViewData["BuildingId"] = new SelectList(_context.Buildings, "BuildingId", "BuildingName");
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Apartment apartment)
        {
            var ToaNha = await _context.Buildings.FirstOrDefaultAsync(x => x.BuildingId == apartment.BuildingId);
            var CanHo = await _context.Apartments.FirstOrDefaultAsync(x => x.FloorNumber == apartment.FloorNumber && x.ApartmentNumber == apartment.ApartmentNumber && x.Building.BuildingId == apartment.BuildingId);
            if (CanHo != null)
            {
                _notyfService.Error("Căn hộ trùng với căn hộ đã thêm trước đó");
                return View();
            }
            apartment.ApartmentCode = ToaNha?.BuildingCode + "-" + (apartment.FloorNumber < 10 ? "0" : "")
                + apartment.FloorNumber + (apartment.ApartmentNumber < 10 ? "0" : "") + apartment.ApartmentNumber;

            apartment.ApartmentName = (ToaNha?.BuildingName?[ToaNha.BuildingName.Length - 1]).ToString()
                + "-" + apartment.FloorNumber + "-" + apartment.ApartmentNumber;
            _context.Add(apartment);
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
                    string query = "SELECT BuildingId FROM BUILDING";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ChuongIds.Add(reader.GetInt32(reader.GetOrdinal("BuildingId")));
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
                        var ApartmentNumber = row["ApartmentNumber"].ToString();
                        row["ApartmentNumber"]= kiemtra(ApartmentNumber);

                        if (!ChuongIds.Contains(Convert.ToInt32(row["BuildingId"])))
                        {
                            _notyfService.Warning("Tòa nhà chưa được tạo !");
                            hasInvalidChuongHoc = true;
                            break;
                        }

                        if(status == "Đã bán")
                        {
                            row["Status"] = 1;
                        }
                        else if(status == "Chưa bán")
                        {
                            row["Status"] = 2;

                        }
                        else if (status == "Bảo trì")
                        {
                            row["Status"] = 3;
                        }
                        else
                        {
                            _notyfService.Error("Căn hộ chưa có trạng thái hoặc sai trạng thái");
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
                            sqlBulkCopy.DestinationTableName = "Apartment";
                            sqlBulkCopy.ColumnMappings.Add("BuildingId", "BuildingId");
                            sqlBulkCopy.ColumnMappings.Add("ApartmentCode", "ApartmentCode");
                            sqlBulkCopy.ColumnMappings.Add("ApartmentName", "ApartmentName");
                            sqlBulkCopy.ColumnMappings.Add("ApartmentNumber", "ApartmentNumber");
                            sqlBulkCopy.ColumnMappings.Add("FloorNumber", "FloorNumber");
                            sqlBulkCopy.ColumnMappings.Add("Area", "Area");
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
            string targetWord = "";

            for (int i = phong.Length - 1; i >= 0; i--)
            {
                if (phong[i] == ' ')
                {
                    break; // Dừng lại khi gặp khoảng trắng
                }
                targetWord = phong[i] + targetWord;
            }

            for (int i = 1; i <= 100; i++)
            {
                if (i.ToString() == targetWord)
                {
                    return i;
                }
            }

            return 0; // Trả về giá trị không hợp lệ nếu không tìm thấy số
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Apartments == null)
            {
                return NotFound();
            }
            var CanHo = await _context.Apartments.FindAsync(id);
            if (CanHo == null)
            {
                return NotFound();
            }
            var Dichvu = _context.ApartmentServices.Include(x => x.Service).Include(x => x.Apartment).Where(x => x.ApartmentId == id);
            ViewBag.Dichvu = Dichvu;
            return View(CanHo);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Apartments == null)
            {
                return NotFound();
            }
            var CanHo = await _context.Apartments.FindAsync(id);
            if (CanHo == null)
            {
                return NotFound();
            }
            ViewData["BuildingId"] = new SelectList(_context.Buildings, "BuildingId", "BuildingName");
            return View(CanHo);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Apartment apartment)
        {
            var ToaNha = await _context.Buildings.FirstOrDefaultAsync(x => x.BuildingId == apartment.BuildingId);
            var CanHo = await _context.Apartments.FirstOrDefaultAsync(x => x.ApartmentId != apartment.ApartmentId
            && x.FloorNumber == apartment.FloorNumber && x.ApartmentNumber == apartment.ApartmentNumber && x.Building.BuildingId == apartment.BuildingId);
            if (CanHo != null)
            {
                _notyfService.Error("Căn hộ trùng với căn hộ đã có");
                ViewData["BuildingId"] = new SelectList(_context.Buildings, "BuildingId", "BuildingName");
                return View();
            }

            apartment.ApartmentCode = ToaNha?.BuildingCode + "-" + (apartment.FloorNumber < 10 ? "0" : "")
                + apartment.FloorNumber + (apartment.ApartmentNumber < 10 ? "0" : "") + apartment.ApartmentNumber;
            apartment.ApartmentName = (ToaNha?.BuildingName?[ToaNha.BuildingName.Length - 1]).ToString()
                + "-" + apartment.FloorNumber + "-" + apartment.ApartmentNumber;
            _context.Update(apartment);
            await _context.SaveChangesAsync();
            _notyfService.Success("Sửa thành công");
            return RedirectToAction("Index");
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Apartments == null)
            {
                return NotFound();
            }
            var CanHo = await _context.Apartments.FindAsync(id);
            if (CanHo == null)
            {
                return NotFound();
            }

            return View(CanHo);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Apartments == null)
            {
                return Problem("Entity set 'QUANLYCHUNGCUContext.Apartments'  is null.");
            }
            var CanHo = await _context.Apartments.FindAsync(id);
            if (CanHo != null)
            {
                _context.Apartments.Remove(CanHo);
            }
            _notyfService.Success("Xóa Thành Công");
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


    }
}
