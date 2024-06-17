using AspNetCoreHero.ToastNotification.Abstractions;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Reflection;
using WebApartmentMangament.Models;

namespace WebApartmentMangament.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BackUpController : Controller
    {
        private QUANLYCHUNGCUContext _context;
        public INotyfService _notyfService { get; }
        private readonly IConfiguration _configuration;
        public BackUpController(QUANLYCHUNGCUContext repo, INotyfService notyfService, IConfiguration configuration)
        {
            _context = repo;
            _notyfService = notyfService;
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ExportExcel()
        {
            try
            {
                var tableProperties = new Dictionary<string, List<string>>
        {
                      { "ElectricMeters", new List<string> { "ElectricMeterId", "ApartmentId", "RegistrationDate", "Code", "DeadingDate", "NumberOne", "NumberEnd", "Price", "Status" } },
            { "WaterMeters", new List<string> { "WaterMeterId", "ApartmentId", "RegistrationDate", "Code", "DeadingDate", "NumberOne", "NumberEnd", "Price", "Status" } },

            { "Accounts", new List<string> { "AccountId", "ApartmentId", "Code", "Avartar", "UserName", "Password", "Email", "InfoId", "RoleId", "RelationshipId", "Status" } },
            { "Apartments", new List<string> { "ApartmentId", "BuildingId", "ApartmentCode", "ApartmentName", "ApartmentNumber", "FloorNumber", "StartDay", "Area", "Status" } },
            { "ApartmentServices", new List<string> { "ApartmentId", "ServiceId", "StartDay", "EndDay", "Status" } },
            { "Buildings", new List<string> { "BuildingId", "BuildingName", "BuildingCode", "Address", "City", "Zip", "FloorNumber", "ApartmentNumber", "AccNumber", "Status" } },
            { "Contracts", new List<string> { "ContractId", "ApartmentId", "AccountId", "StartDay", "EndDay", "Monthly_rent", "Deposit", "Status" } },
            { "InFos", new List<string> { "InfoId", "FullName", "BirthDay", "Sex", "CMND_CCCD", "PhoneNumber", "Country", "City", "District", "Ward", "StreetAddress" } },
            { "News", new List<string> { "NewsId", "Title", "Slug", "Image", "description", "CreateDay", "Status" } },
            { "Relationships", new List<string> { "RelationshipId", "RelationshipName" } },
            { "Revenues", new List<string> { "RevenueId", "ApartmentId", "TotalMoney", "Pay", "Debt", "ServiceFee", "CodeVoucher", "DayCreat", "DayPay", "Payments", "AccountId", "Status", "ElectricNumber", "WaterNumber" } },
            { "Roles", new List<string> { "RoleId", "RoleName" } },
            { "Services", new List<string> { "ServiceId", "ServiceName", "description", "ServiceFee", "Status" } },


        };

                var tableNames = tableProperties.Keys.ToList();

                using (XLWorkbook wb = new XLWorkbook())
                {
                    foreach (var tableName in tableNames)
                    {
                        var data = GetDataFromTable(tableName);
                        var propertiesToExport = tableProperties[tableName];

                        if (data != null && data.Count() > 0)
                        {
                            var sheet = wb.Worksheets.Add(tableName);
                            sheet.Cell(1, 1).Value = tableName;
                            sheet.Cell(2, 1).InsertTable(ToConvertDataTable(data, propertiesToExport));
                        }
                    }

                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        string fileName = $"Export_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }
            }
            catch (Exception)
            {
                _notyfService.Error("Xuất Excel Thất Bại!");
            }

            return RedirectToAction("Index");
        }


        private List<object> GetDataFromTable(string tableName)
        {
            // Lấy dữ liệu từ bảng tableName trong cơ sở dữ liệu của bạn
            // Ví dụ: sử dụng Entity Framework Core để truy vấn dữ liệu từ bảng
            switch (tableName)
            {
                case "ElectricMeters":
                    return _context.ElectricMeters.Cast<object>().ToList();
                case "WaterMeters":
                    return _context.WaterMeters.Cast<object>().ToList();
                case "Accounts":
                    return _context.Accounts.Cast<object>().ToList();
                case "Apartments":
                    return _context.Apartments.Cast<object>().ToList();
                case "ApartmentServices":
                    return _context.ApartmentServices.Cast<object>().ToList();
                case "Buildings":
                    return _context.Buildings.Cast<object>().ToList();
                case "Contracts":
                    return _context.Contracts.Cast<object>().ToList();

                case "InFos":
                    return _context.InFos.Cast<object>().ToList();
                case "News":
                    return _context.News.Cast<object>().ToList();
                case "Relationships":
                    return _context.Relationships.Cast<object>().ToList();
                case "Revenues":
                    return _context.Revenues.Cast<object>().ToList();
                case "Roles":
                    return _context.Roles.Cast<object>().ToList();
                case "Services":
                    return _context.Services.Cast<object>().ToList();
            
                // Thêm các trường hợp cho các bảng khác nếu cần

                default:
                    return null;
            }
        }
        private DataTable ToConvertDataTable(List<object> data, List<string> propertiesToExport)
        {
            DataTable dataTable = new DataTable();
            PropertyInfo[] properties = data[0].GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (propertiesToExport.Contains(property.Name))
                {
                    Type propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                    dataTable.Columns.Add(property.Name, propertyType);
                }
            }

            foreach (object item in data)
            {
                DataRow row = dataTable.NewRow();
                foreach (PropertyInfo property in properties)
                {
                    if (propertiesToExport.Contains(property.Name))
                    {
                        object value = property.GetValue(item);
                        if (value != null && value.GetType() == typeof(DateTime))
                        {
                            // Chuyển đổi giá trị DateTime thành chuỗi ngày tháng
                            value = ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        row[property.Name] = value ?? DBNull.Value;
                    }
                }
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
    }
}
