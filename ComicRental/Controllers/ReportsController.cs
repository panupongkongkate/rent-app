using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ComicRental.Data;
using ComicRental.Models;
using ClosedXML.Excel;
using System.Data;

namespace ComicRental.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly ComicRentalContext _context;

        public ReportsController(ComicRentalContext context)
        {
            _context = context;
        }

        [HttpGet("export/revenue")]
        public async Task<IActionResult> ExportRevenueReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddMonths(-1);
                var end = endDate ?? DateTime.Today.AddDays(1);

                // Get rental revenue
                var rentalRevenue = await _context.Rentals
                    .Where(r => r.RentalDate >= start && r.RentalDate < end)
                    .GroupBy(r => r.RentalDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        RentalCount = g.Count(),
                        RentalRevenue = g.Sum(r => r.RentalFee),
                        FineRevenue = g.Sum(r => r.FineAmount),
                        TotalRevenue = g.Sum(r => r.TotalAmount)
                    })
                    .OrderBy(r => r.Date)
                    .ToListAsync();

                // Create Excel workbook
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("รายงานรายได้");

                    // Headers
                    worksheet.Cell(1, 1).Value = "วันที่";
                    worksheet.Cell(1, 2).Value = "จำนวนรายการ";
                    worksheet.Cell(1, 3).Value = "รายได้ค่าเช่า";
                    worksheet.Cell(1, 4).Value = "รายได้ค่าปรับ";
                    worksheet.Cell(1, 5).Value = "รายได้รวม";

                    // Style headers
                    var headerRange = worksheet.Range(1, 1, 1, 5);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Data
                    int row = 2;
                    decimal totalRental = 0, totalFine = 0, totalRevenue = 0;
                    
                    foreach (var item in rentalRevenue)
                    {
                        worksheet.Cell(row, 1).Value = item.Date.ToString("yyyy-MM-dd");
                        worksheet.Cell(row, 2).Value = item.RentalCount;
                        worksheet.Cell(row, 3).Value = item.RentalRevenue;
                        worksheet.Cell(row, 4).Value = item.FineRevenue;
                        worksheet.Cell(row, 5).Value = item.TotalRevenue;
                        
                        totalRental += item.RentalRevenue;
                        totalFine += item.FineRevenue;
                        totalRevenue += item.TotalRevenue;
                        row++;
                    }

                    // Total row
                    worksheet.Cell(row, 1).Value = "รวม";
                    worksheet.Cell(row, 3).Value = totalRental;
                    worksheet.Cell(row, 4).Value = totalFine;
                    worksheet.Cell(row, 5).Value = totalRevenue;
                    
                    var totalRange = worksheet.Range(row, 1, row, 5);
                    totalRange.Style.Font.Bold = true;
                    totalRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

                    // Format currency columns
                    worksheet.Columns(3, 5).Style.NumberFormat.Format = "#,##0.00";
                    
                    // Auto-fit columns
                    worksheet.Columns().AdjustToContents();

                    // Return Excel file
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;
                        
                        return File(stream.ToArray(), 
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                            $"Revenue_Report_{start:yyyyMMdd}_{end:yyyyMMdd}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "เกิดข้อผิดพลาดในการสร้างรายงาน", error = ex.Message });
            }
        }

        [HttpGet("export/rentals")]
        public async Task<IActionResult> ExportRentalsReport([FromQuery] string status = "all")
        {
            try
            {
                var query = _context.Rentals
                    .Include(r => r.Customer)
                    .Include(r => r.Book)
                    .ThenInclude(b => b!.Category)
                    .Include(r => r.Staff)
                    .AsQueryable();

                if (status != "all")
                {
                    query = query.Where(r => r.Status == status);
                }

                var rentals = await query
                    .OrderByDescending(r => r.RentalDate)
                    .ToListAsync();

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("รายการยืม-คืน");

                    // Headers
                    worksheet.Cell(1, 1).Value = "รหัสการยืม";
                    worksheet.Cell(1, 2).Value = "วันที่ยืม";
                    worksheet.Cell(1, 3).Value = "วันครบกำหนด";
                    worksheet.Cell(1, 4).Value = "วันที่คืน";
                    worksheet.Cell(1, 5).Value = "ลูกค้า";
                    worksheet.Cell(1, 6).Value = "เบอร์โทร";
                    worksheet.Cell(1, 7).Value = "หนังสือ";
                    worksheet.Cell(1, 8).Value = "หมวดหมู่";
                    worksheet.Cell(1, 9).Value = "ค่าเช่า";
                    worksheet.Cell(1, 10).Value = "ค่าปรับ";
                    worksheet.Cell(1, 11).Value = "รวม";
                    worksheet.Cell(1, 12).Value = "สถานะ";
                    worksheet.Cell(1, 13).Value = "พนักงาน";

                    // Style headers
                    var headerRange = worksheet.Range(1, 1, 1, 13);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Data
                    int row = 2;
                    foreach (var rental in rentals)
                    {
                        worksheet.Cell(row, 1).Value = rental.RentalId;
                        worksheet.Cell(row, 2).Value = rental.RentalDate.ToString("yyyy-MM-dd HH:mm");
                        worksheet.Cell(row, 3).Value = rental.DueDate.ToString("yyyy-MM-dd");
                        worksheet.Cell(row, 4).Value = rental.ReturnDate?.ToString("yyyy-MM-dd HH:mm") ?? "-";
                        worksheet.Cell(row, 5).Value = rental.Customer?.FullName ?? "-";
                        worksheet.Cell(row, 6).Value = rental.Customer?.Phone ?? "-";
                        worksheet.Cell(row, 7).Value = rental.Book?.Title ?? "-";
                        worksheet.Cell(row, 8).Value = rental.Book?.Category?.CategoryName ?? "-";
                        worksheet.Cell(row, 9).Value = rental.RentalFee;
                        worksheet.Cell(row, 10).Value = rental.FineAmount;
                        worksheet.Cell(row, 11).Value = rental.TotalAmount;
                        worksheet.Cell(row, 12).Value = rental.Status;
                        worksheet.Cell(row, 13).Value = rental.Staff?.FullName ?? "-";
                        row++;
                    }

                    // Format columns
                    worksheet.Columns(9, 11).Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;
                        
                        return File(stream.ToArray(), 
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                            $"Rentals_Report_{DateTime.Now:yyyyMMdd}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "เกิดข้อผิดพลาดในการสร้างรายงาน", error = ex.Message });
            }
        }

        [HttpGet("export/overdue")]
        public async Task<IActionResult> ExportOverdueReport()
        {
            try
            {
                var overdueRentals = await _context.Rentals
                    .Include(r => r.Customer)
                    .Include(r => r.Book)
                    .Where(r => r.Status == "Active" && r.DueDate < DateTime.Today)
                    .OrderBy(r => r.DueDate)
                    .ToListAsync();

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("รายการค้างคืน");

                    // Headers
                    worksheet.Cell(1, 1).Value = "รหัสการยืม";
                    worksheet.Cell(1, 2).Value = "ลูกค้า";
                    worksheet.Cell(1, 3).Value = "เบอร์โทร";
                    worksheet.Cell(1, 4).Value = "หนังสือ";
                    worksheet.Cell(1, 5).Value = "วันที่ยืม";
                    worksheet.Cell(1, 6).Value = "วันครบกำหนด";
                    worksheet.Cell(1, 7).Value = "วันเกิน";
                    worksheet.Cell(1, 8).Value = "ค่าปรับ (ประมาณ)";

                    // Style headers
                    var headerRange = worksheet.Range(1, 1, 1, 8);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.Red;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Get fine rate
                    var fineRate = await _context.Settings
                        .Where(s => s.SettingKey == "FinePerDay")
                        .Select(s => s.SettingValue)
                        .FirstOrDefaultAsync() ?? "10";
                    var finePerDay = decimal.Parse(fineRate);

                    // Data
                    int row = 2;
                    decimal totalEstimatedFine = 0;
                    
                    foreach (var rental in overdueRentals)
                    {
                        var daysOverdue = (DateTime.Today - rental.DueDate).Days;
                        var estimatedFine = daysOverdue * finePerDay;
                        
                        worksheet.Cell(row, 1).Value = rental.RentalId;
                        worksheet.Cell(row, 2).Value = rental.Customer?.FullName ?? "-";
                        worksheet.Cell(row, 3).Value = rental.Customer?.Phone ?? "-";
                        worksheet.Cell(row, 4).Value = rental.Book?.Title ?? "-";
                        worksheet.Cell(row, 5).Value = rental.RentalDate.ToString("yyyy-MM-dd");
                        worksheet.Cell(row, 6).Value = rental.DueDate.ToString("yyyy-MM-dd");
                        worksheet.Cell(row, 7).Value = daysOverdue;
                        worksheet.Cell(row, 8).Value = estimatedFine;
                        
                        totalEstimatedFine += estimatedFine;
                        row++;
                    }

                    // Total row
                    worksheet.Cell(row, 1).Value = "รวม";
                    worksheet.Cell(row, 7).Value = "ค่าปรับรวม (ประมาณ)";
                    worksheet.Cell(row, 8).Value = totalEstimatedFine;
                    
                    var totalRange = worksheet.Range(row, 1, row, 8);
                    totalRange.Style.Font.Bold = true;
                    totalRange.Style.Fill.BackgroundColor = XLColor.Orange;

                    // Format columns
                    worksheet.Column(8).Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;
                        
                        return File(stream.ToArray(), 
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                            $"Overdue_Report_{DateTime.Now:yyyyMMdd}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "เกิดข้อผิดพลาดในการสร้างรายงาน", error = ex.Message });
            }
        }

        [HttpGet("export/profit")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportProfitReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddMonths(-1);
                var end = endDate ?? DateTime.Today.AddDays(1);

                // Get all completed rentals
                var rentals = await _context.Rentals
                    .Include(r => r.Book)
                    .ThenInclude(b => b!.Category)
                    .Where(r => r.RentalDate >= start && r.RentalDate < end)
                    .ToListAsync();

                // Group by month
                var monthlyData = rentals
                    .GroupBy(r => new { r.RentalDate.Year, r.RentalDate.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        TotalRentals = g.Count(),
                        RentalRevenue = g.Sum(r => r.RentalFee),
                        FineRevenue = g.Sum(r => r.FineAmount),
                        TotalRevenue = g.Sum(r => r.TotalAmount),
                        // Assuming 70% profit margin
                        EstimatedProfit = g.Sum(r => r.TotalAmount) * 0.7m
                    })
                    .OrderBy(m => m.Year).ThenBy(m => m.Month)
                    .ToList();

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("รายงานกำไร");

                    // Title
                    worksheet.Cell(1, 1).Value = $"รายงานกำไร ({start:dd/MM/yyyy} - {end:dd/MM/yyyy})";
                    worksheet.Range(1, 1, 1, 6).Merge();
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Headers
                    worksheet.Cell(3, 1).Value = "เดือน/ปี";
                    worksheet.Cell(3, 2).Value = "จำนวนรายการ";
                    worksheet.Cell(3, 3).Value = "รายได้ค่าเช่า";
                    worksheet.Cell(3, 4).Value = "รายได้ค่าปรับ";
                    worksheet.Cell(3, 5).Value = "รายได้รวม";
                    worksheet.Cell(3, 6).Value = "กำไร (ประมาณ 70%)";

                    // Style headers
                    var headerRange = worksheet.Range(3, 1, 3, 6);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.DarkGreen;
                    headerRange.Style.Font.FontColor = XLColor.White;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Data
                    int row = 4;
                    decimal totalRental = 0, totalFine = 0, totalRevenue = 0, totalProfit = 0;
                    
                    foreach (var item in monthlyData)
                    {
                        worksheet.Cell(row, 1).Value = $"{item.Month}/{item.Year}";
                        worksheet.Cell(row, 2).Value = item.TotalRentals;
                        worksheet.Cell(row, 3).Value = item.RentalRevenue;
                        worksheet.Cell(row, 4).Value = item.FineRevenue;
                        worksheet.Cell(row, 5).Value = item.TotalRevenue;
                        worksheet.Cell(row, 6).Value = item.EstimatedProfit;
                        
                        totalRental += item.RentalRevenue;
                        totalFine += item.FineRevenue;
                        totalRevenue += item.TotalRevenue;
                        totalProfit += item.EstimatedProfit;
                        row++;
                    }

                    // Total row
                    worksheet.Cell(row, 1).Value = "รวมทั้งหมด";
                    worksheet.Cell(row, 3).Value = totalRental;
                    worksheet.Cell(row, 4).Value = totalFine;
                    worksheet.Cell(row, 5).Value = totalRevenue;
                    worksheet.Cell(row, 6).Value = totalProfit;
                    
                    var totalRange = worksheet.Range(row, 1, row, 6);
                    totalRange.Style.Font.Bold = true;
                    totalRange.Style.Fill.BackgroundColor = XLColor.LightGreen;

                    // Format currency columns
                    worksheet.Columns(3, 6).Style.NumberFormat.Format = "#,##0.00";
                    
                    // Auto-fit columns
                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;
                        
                        return File(stream.ToArray(), 
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                            $"Profit_Report_{start:yyyyMMdd}_{end:yyyyMMdd}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "เกิดข้อผิดพลาดในการสร้างรายงาน", error = ex.Message });
            }
        }

        [HttpGet("export/customers")]
        public async Task<IActionResult> ExportCustomersReport()
        {
            try
            {
                var customers = await _context.Customers
                    .OrderByDescending(c => c.TotalBorrowed)
                    .ToListAsync();
                
                // Get active rentals count for each customer
                var customerRentals = await _context.Rentals
                    .Where(r => r.Status == "Active")
                    .GroupBy(r => r.CustomerId)
                    .Select(g => new { CustomerId = g.Key, ActiveCount = g.Count() })
                    .ToDictionaryAsync(x => x.CustomerId, x => x.ActiveCount);

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("รายงานลูกค้า");

                    // Headers
                    worksheet.Cell(1, 1).Value = "รหัสลูกค้า";
                    worksheet.Cell(1, 2).Value = "ชื่อ-นามสกุล";
                    worksheet.Cell(1, 3).Value = "เบอร์โทร";
                    worksheet.Cell(1, 4).Value = "อีเมล";
                    worksheet.Cell(1, 5).Value = "ที่อยู่";
                    worksheet.Cell(1, 6).Value = "ยืมทั้งหมด";
                    worksheet.Cell(1, 7).Value = "กำลังยืม";
                    worksheet.Cell(1, 8).Value = "ค่าปรับสะสม";
                    worksheet.Cell(1, 9).Value = "วันที่สมัคร";

                    // Style headers
                    var headerRange = worksheet.Range(1, 1, 1, 9);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Data
                    int row = 2;
                    foreach (var customer in customers)
                    {
                        var activeRentals = customerRentals.GetValueOrDefault(customer.CustomerId, 0);
                        
                        worksheet.Cell(row, 1).Value = customer.CustomerId;
                        worksheet.Cell(row, 2).Value = customer.FullName;
                        worksheet.Cell(row, 3).Value = customer.Phone;
                        worksheet.Cell(row, 4).Value = customer.Email ?? "-";
                        worksheet.Cell(row, 5).Value = customer.Address ?? "-";
                        worksheet.Cell(row, 6).Value = customer.TotalBorrowed;
                        worksheet.Cell(row, 7).Value = activeRentals;
                        worksheet.Cell(row, 8).Value = customer.TotalFines;
                        worksheet.Cell(row, 9).Value = customer.CreatedDate.ToString("yyyy-MM-dd");
                        row++;
                    }

                    // Format columns
                    worksheet.Column(8).Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;
                        
                        return File(stream.ToArray(), 
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                            $"Customers_Report_{DateTime.Now:yyyyMMdd}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "เกิดข้อผิดพลาดในการสร้างรายงาน", error = ex.Message });
            }
        }
    }
}