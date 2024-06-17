using AspNetCoreHero.ToastNotification.Abstractions;
using ClosedXML.Excel;
using iText.Html2pdf;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System;
using System.Data;
using System.Reflection;
using System.Text;
using WebApartmentMangament.Models;

namespace WebApartmentMangament.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, NhanVien")]
    public class RevenueController : Controller
    {
        private QUANLYCHUNGCUContext _context;
        public INotyfService _notyfService { get; }
        public RevenueController(QUANLYCHUNGCUContext repo, INotyfService notyfService)
        {
            _context = repo;
            _notyfService = notyfService;
        }
        [Route("Admin/Revenue/Congno")]
        public IActionResult Index()
        {
            var totalmony = _context.Revenues.Include(x => x.Apartment).Include(y => y.Account).Where(x => x.Payments == 2).ToList();
            return View(totalmony);
        }
        public IActionResult PhieuThu()
        {
            var totalmony = _context.Revenues.Include(x => x.Apartment).Include(y => y.Account).ThenInclude(y => y.Info).Where(x => x.Payments == 1).ToList();
            return View(totalmony);
        }
        public async Task<IActionResult> Create()
        {
            DateTime currentDate = DateTime.Now;

            var exit = await _context.Revenues
                .OrderByDescending(x => x.DayCreat)
                .FirstOrDefaultAsync();

            if (exit != null)
            {
                DateTime exitPlus30Days = exit.DayCreat.Value.AddDays(30);
                if (exitPlus30Days > currentDate)
                {
                    _notyfService.Error("Chưa tới ngày tính công nợ");
                    return RedirectToAction("Index");
                }
            }

            List<Apartment> canho = await _context.Apartments
                .Include(x => x.Accounts)
                .Include(x => x.WaterMeters)
                .Include(x => x.ElectricMeters)
                .Include(x => x.Contracts)
                .Include(x => x.ApartmentServices)
                .ThenInclude(x => x.Service)
                .Where(x => x.Status == 1)
                .ToListAsync();

            List<Revenue> doanhthu = new List<Revenue>();

            foreach (var item in canho)
            {
                double? numberEnd = item.ElectricMeters.FirstOrDefault()?.NumberEnd;
                double? numberOne = item.ElectricMeters.FirstOrDefault()?.NumberOne;
                decimal price = item.ElectricMeters.FirstOrDefault()?.Price ?? 0M;
                decimal tiendien = ((decimal)(numberEnd - numberOne)) * price;

                double? numberEnd1 = item.WaterMeters.FirstOrDefault()?.NumberEnd;
                double? numberOne1 = item.WaterMeters.FirstOrDefault()?.NumberOne;
                decimal price1 = item.WaterMeters.FirstOrDefault()?.Price ?? 0M;
                decimal tiennuoc = ((decimal)(numberEnd1 - numberOne1)) * price1;

                decimal phidv = item.ApartmentServices
                    .Where(x => x.Status == 1)
                    .Sum(x => (decimal)x.Service.ServiceFee);
                decimal? tiennha = item.Contracts.FirstOrDefault()?.MonthlyRent;
                doanhthu.Add(new Revenue
                {
                    ApartmentId = item.ApartmentId,
                    TotalMoney = tiendien + tiennuoc + phidv + tiennha,
                    Pay = 0,
                    Debt = 0,
                    ServiceFee = phidv,
                    Payments = 2,
                    Status = 1,
                    DayCreat = DateTime.Now,
                    WaterNumber = numberEnd1 - numberOne1,
                    ElectricNumber = numberEnd - numberOne,

                });
                var tongtien = tiendien + tiennuoc + phidv + tiennha;
                // Gửi email chứa token đến địa chỉ email của người dùng
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("Quản Lý Chung Cư CENTANA", "admin@example.com"));
                email.To.Add(MailboxAddress.Parse($"{item.Accounts.FirstOrDefault().Email}"));
                email.Subject = "Hóa đơn thu phí hằng tháng";

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = @$"<table style='border-collapse: collapse;'>
                             <caption style='font-weight: bold; text-align: center;'>Hóa đơn chi tiết</caption>
                            <tr>
                                <th style='border: 1px solid black; padding: 5px;'>Tên dịch vụ</th>
                                <th style='border: 1px solid black; padding: 5px;'>Đơn giá</th>
                                <th style='border: 1px solid black; padding: 5px;'>Số lượng</th>
                                <th style='border: 1px solid black; padding: 5px;'>Thành tiền</th>
                            </tr>
                            <tr>
                                <td style='border: 1px solid black; padding: 5px;'>Tiền Điện</td>
                                <td style='border: 1px solid black; padding: 5px;'> {string.Format("{0:N0}", price)}VND</td>
                                <td style='border: 1px solid black; padding: 5px;'>{numberEnd - numberOne}</td>
                                <td style='border: 1px solid black; padding: 5px;'> {string.Format("{0:N0}", tiendien)}VNĐ</td>
                            </tr>
                            <tr>
                                <td style='border: 1px solid black; padding: 5px;'>Tiền nước</td>
                                <td style='border: 1px solid black; padding: 5px;'>{string.Format("{0:N0}", price1)}VND</td>
                                <td style='border: 1px solid black; padding: 5px;'>{numberEnd1 - numberOne1}</td>
                                <td style='border: 1px solid black; padding: 5px;'>{string.Format("{0:N0}", tiennuoc)}VNĐ</td>
                            </tr> 
                                <tr>
                                <td style='border: 1px solid black; padding: 5px;'>Tiền Nhà</td>
                                <td style='border: 1px solid black; padding: 5px;'>{string.Format("{0:N0}", tiennha)}VND</td>
                                <td style='border: 1px solid black; padding: 5px;'>1</td>
                                <td style='border: 1px solid black; padding: 5px;'>{string.Format("{0:N0}", tiennha)}VNĐ</td>
                            </tr>";
    
                            foreach (var dichvu in item.ApartmentServices)
                            {
                                bodyBuilder.HtmlBody += @$"<tr>
                                            <td style='border: 1px solid black; padding: 5px;'>{dichvu.Service.ServiceName}</td>
                                            <td style='border: 1px solid black; padding: 5px;'>{string.Format("{0:N0}", dichvu.Service.ServiceFee)}VND</td>
                                            <td style='border: 1px solid black; padding: 5px;'>1</td>
                                            <td style='border: 1px solid black; padding: 5px;'>{string.Format("{0:N0}", dichvu.Service.ServiceFee)}VNĐ</td>
                                        </tr>";
                            }

                

                bodyBuilder.HtmlBody += @$"
                                <tfoot>
                                <tr>
                                    <td colspan='4' style='border: 1px solid black; padding: 5px; text-align: right;'>Tổng tiền: {string.Format("{0:N0}", tongtien)}VNĐ</td>
                                </tr>
                            </tfoot>
                            <tfoot>
                                <tr>
                                    <td colspan='4' style='border: 1px solid black; padding: 5px; text-align: right;'>Ghi chú: Đơn giá đã bao gồm thuế VAT.</td>
                                </tr>
                            </tfoot>
                          </table>
                        <p>Ghi chú: Hóa đơn thanh toán các khoản phí căn hộ của bạn phải trả hằng tháng</p>
";
                // Đặt phần HTML là nội dung chính của email
                email.Body = bodyBuilder.ToMessageBody();
                using var smtp = new SmtpClient();
                smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                smtp.Authenticate("khangchannel19@gmail.com", "yyvpxefotptdyhel");
                smtp.Send(email);
                smtp.Disconnect(true);
                ////////
                var dienadd = item.ElectricMeters.FirstOrDefault();
                var nuocadd = item.WaterMeters.FirstOrDefault();
                dienadd.NumberOne = numberEnd;
                nuocadd.NumberOne = numberEnd1;
            }


            _context.Revenues.AddRange(doanhthu);
            await _context.SaveChangesAsync();
            _notyfService.Success("Tạo danh sách công nợ thành công");
            return RedirectToAction("Index");
        }
        [Authorize(Roles = "Admin")]
        [Route("Admin/Revenue/XoaPhieuThu")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Revenues == null)
            {
                return NotFound();
            }
            var totalmony = await _context.Revenues.Include(x => x.Apartment).Include(y => y.Account).FirstOrDefaultAsync(x => x.RevenueId == id);
            if (totalmony == null)
            {
                return NotFound();
            }

            return View(totalmony);
        }
        [Route("Admin/Revenue/ThanhToan")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Revenues == null)
            {
                return NotFound();
            }
            var totalmony = await _context.Revenues
                .Include(x => x.Apartment)
                .Include(y => y.Account)
                .FirstOrDefaultAsync(x => x.RevenueId == id);
            if (totalmony == null)
            {
                return NotFound();
            }

            return View(totalmony);
        }

        [HttpPost]
        [Route("Admin/Revenue/ThanhToan")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Revenue revenue, int? id)
        {
            var Idclam = User.Claims.SingleOrDefault(c => c.Type == "Id");
            int Id = 0;
            if (Idclam != null)
            { Id = Int32.Parse(Idclam.Value); }
            else
            {
                _notyfService.Error("Vui lòng đăng nhập!");
                return RedirectToAction("Index");
            }
            var conno = await _context.Revenues.FirstOrDefaultAsync(x => x.RevenueId == revenue.RevenueId);
            if (conno == null)
            {
                return NotFound();
            }
            conno.Pay = revenue.Pay;
            conno.Debt = conno.TotalMoney - revenue.Pay;
            conno.DayPay = DateTime.Now;
            conno.AccountId = Id;
            if (conno.Debt == 0)
            {
                conno.Payments = 1;
            }
            if (conno.Debt < 0)
            {
                _notyfService.Error("Vui lòng nhập đúng số tiền thanh toán");
                return RedirectToAction("Index");
            }
            _notyfService.Success("Thanh toán thành công!");
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");

        }

        public async Task<IActionResult> TatToan(int? id)
        {
            var Idclam = User.Claims.SingleOrDefault(c => c.Type == "Id");
            int Id = 0;
            if (Idclam != null)
            { Id = Int32.Parse(Idclam.Value); }
            else
            {
                _notyfService.Error("Vui lòng đăng nhập!");
                return RedirectToAction("Index");
            }
            var conno = await _context.Revenues.FirstOrDefaultAsync(x => x.RevenueId == id);
            if (conno == null)
            {
                return NotFound();
            }
            conno.Pay = conno.TotalMoney;
            conno.Debt = 0;
            conno.DayPay = DateTime.Now;
            conno.AccountId = Id;
            conno.Payments = 1;

            _notyfService.Success("Tất toán thành công!");
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> XuatPhieu(int? id)
        {
            // Gọi hàm XuatPhieuPDF để tạo tệp PDF
            var pdfContentResult = await XuatPhieuPDF(id);

            // Trả về tệp PDF như là phản hồi HTTP
            return pdfContentResult;
        }

        public async Task<IActionResult> XuatPhieuPDF(int? id)
        {
            var doanhthu = await _context.Apartments
                .Include(x => x.Accounts)
                .ThenInclude(x => x.Info)
                .Include(x => x.Revenues)
                .Include(x => x.WaterMeters)
                .Include(x => x.ElectricMeters)
                .Include(x => x.Contracts)
                .Include(x => x.ApartmentServices)
                .ThenInclude(x => x.Service)
                .FirstOrDefaultAsync(x => x.Revenues.FirstOrDefault().RevenueId == id);
            if (doanhthu == null)
            {
                return NotFound();
            }
            var document = new StringBuilder();
            var htmlcontent = @"
                <!DOCTYPE html>
                <html>
                <head>
                  <title>Phiếu xuất</title>
                  <style>
                    body {
                      font-family: Arial, sans-serif;
                      size: 210mm 297mm;
                      margin: 0 ;
      
                    }
                                .container {
                                  max-width: 794px;
                                  margin: 0 auto;
                                  font-family: Helvetica;
                                 line-height: 1.5;
                                }

                    table {
                      width: 100%;
                      border-collapse: collapse;
                      margin-top: 20px;
                    }

                    th, td {
                      padding: 10px;
                      text-align: left;
                      border-bottom: 1px solid #ddd;
                    }

                    th {
                      background-color: #f2f2f2;
                      font-weight: bold;
                    }

                    .total {
                      font-weight: bold;
                      background-color: #f2f2f2;
                    }

                    .total td {
                      border-top: 2px solid #333;
                    }
                  </style>
                </head>
                <body>";
            htmlcontent += @$"<div class='container'>
                  <div class='header'>
                    <p style='font-weight: bold;'>BAN QUẢN LÝ CHUNG CƯ CENTANA THỦ THIÊN</p>
                    <p>36 Mai Chí Thọ, Phường An Phú, Quận 2, TP.Thủ Đức, TP.HCM</p>
                    <p>Email: info@abc.com Hotline: 0964431054</p>
   
                  </div>
                <div style='text-align: center;'>
                  <h1 style='font-weight: bold;'>Phiếu xuất</h1>
                  <p>Xuất phiếu thu ngày: {(doanhthu?.Revenues?.FirstOrDefault()?.DayPay?.ToString("dd/MM/yyyy HH:mm") ?? "N/A")}</p>
                </div>
                <p>Căn hộ : {doanhthu?.ApartmentCode}({doanhthu?.ApartmentName})</p>
                <p>Tên chủ hộ : {doanhthu?.Contracts.FirstOrDefault()?.Account?.Info?.FullName}</p>
                <p>Lý do thu : Thu phí hằng tháng</p>
                  <table>
                    <tr>
                      <th>Tên dịch vụ</th>
                      <th>Số lượng</th>
                      <th>Đơn giá</th>
                      <th>Thành tiền</th>
                    </tr>
                    <tr>
                      <td>Điện</td>";

            htmlcontent += @$"<td>{doanhthu?.Revenues.FirstOrDefault()?.ElectricNumber}</ td > 
                            <td>{string.Format("{0:N0}", doanhthu?.ElectricMeters.FirstOrDefault()?.Price)} đ</td>
                      <td>{string.Format("{0:N0}", (decimal?)doanhthu?.Revenues.FirstOrDefault()?.ElectricNumber * doanhthu?.ElectricMeters.FirstOrDefault()?.Price)} đ</td>
                    </tr>
                    <tr>
                      <td>Nước</td>
                      <td>{doanhthu?.Revenues.FirstOrDefault()?.WaterNumber}</td>
                      <td>{string.Format("{0:N0}", doanhthu?.WaterMeters.FirstOrDefault()?.Price)} đ</td>
                      <td>{string.Format("{0:N0}", (decimal?)doanhthu?.Revenues.FirstOrDefault()?.WaterNumber * doanhthu?.WaterMeters.FirstOrDefault()?.Price)} đ</td>
                    </tr>";

            foreach (var item in doanhthu.ApartmentServices)
            {
                htmlcontent += @$"<tr>
                      <td>{item.Service.ServiceName}</td>
                      <td>1</td>
                      <td>{string.Format("{0:N0}", item?.Service.ServiceFee)} đ</td>
                      <td>{string.Format("{0:N0}", item?.Service.ServiceFee)} đ</td>
                    </tr>";
            }

            htmlcontent += @$"<tr>
                      <td>Tiền nhà</td>
                      <td>1</td>
                      <td>{string.Format("{0:N0}", doanhthu?.Contracts.FirstOrDefault()?.MonthlyRent)} đ</td>
                      <td>{string.Format("{0:N0}", doanhthu?.Contracts.FirstOrDefault()?.MonthlyRent)} đ</td>
                    </tr>
                    <tr class='total'>
                      <td colspan='3'>Tổng tiền</td>
                      <td>{string.Format("{0:N0}", doanhthu?.Revenues.FirstOrDefault()?.TotalMoney)} đ</td>
                    </tr>
                  </table> 
                  <p style='font-weight: bold;'>Cư dân kiểm tra kỹ phiếu thu trước khi ký và nhận phiếu thu. Phiếu thu cư dân vui lòng giữ lại 60 ngày</p>
                <div style='overflow: hidden;'>
                 <div style='float: left; max-width: 50%; text-align: center; margin-left: 20%;'>
                  <p>
                    Người lập phiếu
                  </p> <p>
                   Ký , họ tên
                  </p>
                 </div>
                 <div style='text-align: center; max-width: 100%; margin-left: 80%;'>
                  <p>
                    Người nộp
                  </p> <p>
                   Ký , họ tên
                  </p>
                 </div>
                </div>

                </div>

                </body>
                </html>
                                ";
            var pdfStream = new MemoryStream();
            var pdfWriter = new PdfWriter(pdfStream);
            var pdfDocument = new PdfDocument(pdfWriter);
            pdfDocument.SetDefaultPageSize(PageSize.A4);
            HtmlConverter.ConvertToPdf(htmlcontent, pdfStream);

            return new FileContentResult(pdfStream.ToArray(), "application/pdf");

        }

        public IActionResult ExportExcel()
        {
            try
            {
                var data = _context.Revenues.Include(x => x.Apartment).Where(x => x.Payments == 1).ToList();

                if (data != null && data.Count() > 0)
                {
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        wb.Worksheets.Add(ToConvertDataTable(data.ToList()));

                        using (MemoryStream stream = new MemoryStream())
                        {

                            wb.SaveAs(stream);
                            string fileName = $"DoanhThu_{DateTime.Now.ToString("dd/MM/yyyy")}.xlsx";
                            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocuments.spreadsheetml.sheet", fileName);
                        }

                    }

                }
                _notyfService.Error("Kỳ kiểm tra này không có điểm của sinh viên");
            }
            catch (Exception)
            {
                _notyfService.Error("Xuất Excel Thất Bại!");
            }
            return RedirectToAction("Index");
        }

        public DataTable ToConvertDataTable<T>(List<T> items)
        {
            DataTable dt = new DataTable(typeof(T).Name);
            PropertyInfo[] propInfo = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Dictionary<string, string> englishToVietnamese = new Dictionary<string, string>
    {
        { "ApartmentId", "Mã căn hộ" },
        { "TotalMoney", "Tổng tiền" },
        { "Pay", "Thanh toán" },
        { "Debt", "Nợ" },
        { "ServiceFee", "Phí dịch vụ" },
        { "DayCreat", "Ngày tạo" },
        { "DayPay", "Ngày thanh toán" },
        { "ElectricNumber", "Số điện" },
        { "WaterNumber", "Số nước" }
    };

            List<string> columnsToExport = new List<string>(englishToVietnamese.Keys);

            foreach (PropertyInfo prop in propInfo)
            {
                if (columnsToExport.Contains(prop.Name))
                {
                    dt.Columns.Add(englishToVietnamese[prop.Name]);
                }
            }

            foreach (T item in items)
            {
                var values = new object[columnsToExport.Count];
                int j = 0;
                for (int i = 0; i < propInfo.Length; i++)
                {
                    if (columnsToExport.Contains(propInfo[i].Name))
                    {
                        if (propInfo[i].Name == "ApartmentId")
                        {
                            // Lấy giá trị của ApartmentCode từ ApartmentId
                            int apartmentId = (int)propInfo[i].GetValue(item, null);
                            string apartmentCode = GetApartmentCodeFromId(apartmentId);
                            values[j] = apartmentCode;
                        }
                        else
                        {
                            values[j] = propInfo[i].GetValue(item, null);
                        }
                        j++;
                    }
                }
                dt.Rows.Add(values);
            }

            return dt;
        }
        private string GetApartmentCodeFromId(int apartmentId)
        {
            var code = _context.Apartments.FirstOrDefault(x => x.ApartmentId == apartmentId);
            return code.ApartmentCode;
           
        }

    }
}
