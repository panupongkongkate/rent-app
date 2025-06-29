using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ComicRental.Data;

namespace ComicRental.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ComicRentalContext _context;

        public DashboardController(ComicRentalContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // Today's stats
            var todayRentals = await _context.Rentals
                .CountAsync(r => r.RentalDate >= today && r.RentalDate < tomorrow);

            var todayReturns = await _context.Rentals
                .CountAsync(r => r.ReturnDate >= today && r.ReturnDate < tomorrow);

            var todayRentalRecords = await _context.Rentals
                .Where(r => r.RentalDate >= today && r.RentalDate < tomorrow)
                .Select(r => r.RentalFee)
                .ToListAsync();
            var todayRevenue = todayRentalRecords.Sum();

            var todayReturnRecords = await _context.Rentals
                .Where(r => r.ReturnDate >= today && r.ReturnDate < tomorrow)
                .Select(r => r.FineAmount)
                .ToListAsync();
            var todayFines = todayReturnRecords.Sum();

            // Active rentals
            var activeRentals = await _context.Rentals
                .CountAsync(r => r.Status == "Active");

            // Overdue rentals
            var overdueRentals = await _context.Rentals
                .CountAsync(r => r.Status == "Active" && r.DueDate < today);

            // Total counts
            var totalBooks = await _context.Books.CountAsync();
            var availableBooks = await _context.Books.CountAsync(b => b.Status == "Available");
            var totalCustomers = await _context.Customers.CountAsync(c => c.Status == "Active");
            var newCustomersThisMonth = await _context.Customers
                .CountAsync(c => c.CreatedDate.Month == today.Month && c.CreatedDate.Year == today.Year);

            // Top books this month
            var topBooks = await _context.Rentals
                .Include(r => r.Book)
                .Where(r => r.RentalDate.Month == today.Month && r.RentalDate.Year == today.Year && r.Book != null)
                .GroupBy(r => new { r.BookId, r.Book!.Title })
                .Select(g => new
                {
                    BookId = g.Key.BookId,
                    Title = g.Key.Title ?? "Unknown",
                    Count = g.Count()
                })
                .OrderByDescending(b => b.Count)
                .Take(5)
                .ToListAsync();

            // Revenue by category this month
            var monthlyRentals = await _context.Rentals
                .Include(r => r.Book)
                .ThenInclude(b => b!.Category)
                .Where(r => r.RentalDate.Month == today.Month && r.RentalDate.Year == today.Year && r.Book != null && r.Book.Category != null)
                .Select(r => new { r.Book!.Category!.CategoryId, r.Book.Category.CategoryName, r.RentalFee })
                .ToListAsync();

            var revenueByCategory = monthlyRentals
                .GroupBy(r => new { r.CategoryId, r.CategoryName })
                .Select(g => new
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName ?? "Unknown",
                    Revenue = g.Sum(r => r.RentalFee)
                })
                .OrderByDescending(c => c.Revenue)
                .ToList();

            return Ok(new
            {
                today = new
                {
                    rentals = todayRentals,
                    returns = todayReturns,
                    revenue = todayRevenue,
                    fines = todayFines
                },
                active = new
                {
                    rentals = activeRentals,
                    overdue = overdueRentals
                },
                inventory = new
                {
                    totalBooks,
                    availableBooks,
                    rentedBooks = totalBooks - availableBooks
                },
                customers = new
                {
                    total = totalCustomers,
                    newThisMonth = newCustomersThisMonth
                },
                topBooks,
                revenueByCategory
            });
        }

        [HttpGet("reports")]
        public async Task<IActionResult> GetReportStats([FromQuery] string period = "month")
        {
            var today = DateTime.Today;
            DateTime startDate, endDate;

            // Calculate date range based on period
            switch (period.ToLower())
            {
                case "today":
                    startDate = today;
                    endDate = today.AddDays(1);
                    break;
                case "week":
                    var startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1);
                    startDate = startOfWeek;
                    endDate = startOfWeek.AddDays(7);
                    break;
                case "month":
                    startDate = new DateTime(today.Year, today.Month, 1);
                    endDate = startDate.AddMonths(1);
                    break;
                case "quarter":
                    var quarter = (today.Month - 1) / 3 + 1;
                    startDate = new DateTime(today.Year, (quarter - 1) * 3 + 1, 1);
                    endDate = startDate.AddMonths(3);
                    break;
                case "year":
                    startDate = new DateTime(today.Year, 1, 1);
                    endDate = startDate.AddYears(1);
                    break;
                default:
                    startDate = new DateTime(today.Year, today.Month, 1);
                    endDate = startDate.AddMonths(1);
                    break;
            }

            // Get rentals in period
            var periodRentals = await _context.Rentals
                .Include(r => r.Book)
                .ThenInclude(b => b!.Category)
                .Include(r => r.Customer)
                .Where(r => r.RentalDate >= startDate && r.RentalDate < endDate)
                .ToListAsync();

            // Calculate key metrics
            var totalRentals = periodRentals.Count;
            var totalRevenue = periodRentals.Sum(r => r.RentalFee + r.FineAmount);
            var rentalIncome = periodRentals.Sum(r => r.RentalFee);
            var fineIncome = periodRentals.Sum(r => r.FineAmount);

            // Active customers in period (customers who rented)
            var activeCustomers = periodRentals.Select(r => r.CustomerId).Distinct().Count();
            var avgRentalValue = totalRentals > 0 ? rentalIncome / totalRentals : 0;

            // Status breakdown
            var completedRentals = periodRentals.Count(r => r.Status == "Completed");
            var pendingRentals = await _context.Rentals.CountAsync(r => r.Status == "Active");
            var onTimeReturns = periodRentals.Count(r => r.Status == "Completed" && r.FineAmount == 0);
            var onTimeRate = completedRentals > 0 ? (decimal)onTimeReturns / completedRentals * 100 : 0;

            // Book statistics
            var totalBooks = await _context.Books.CountAsync();
            var rentedBooks = await _context.Books.CountAsync(b => b.Status == "Rented");
            var utilizationRate = totalBooks > 0 ? (decimal)rentedBooks / totalBooks * 100 : 0;

            // Top books in period
            var topBooks = periodRentals
                .Where(r => r.Book != null)
                .GroupBy(r => new { r.BookId, r.Book!.Title })
                .Select(g => new
                {
                    Title = g.Key.Title ?? "Unknown",
                    Rentals = g.Count(),
                    Revenue = g.Sum(r => r.RentalFee)
                })
                .OrderByDescending(b => b.Rentals)
                .Take(5)
                .ToList();

            // Top customers in period
            var topCustomers = periodRentals
                .Where(r => r.Customer != null)
                .GroupBy(r => new { r.CustomerId, r.Customer!.FullName })
                .Select(g => new
                {
                    Name = g.Key.FullName ?? "Unknown",
                    Rentals = g.Count(),
                    Revenue = g.Sum(r => r.RentalFee + r.FineAmount)
                })
                .OrderByDescending(c => c.Revenue)
                .Take(5)
                .ToList();

            // Revenue by category
            var categoryRevenue = periodRentals
                .Where(r => r.Book?.Category != null)
                .GroupBy(r => new { r.Book!.Category!.CategoryId, r.Book.Category.CategoryName })
                .Select(g => new
                {
                    CategoryName = g.Key.CategoryName ?? "Unknown",
                    Revenue = g.Sum(r => r.RentalFee),
                    Count = g.Count()
                })
                .OrderByDescending(c => c.Revenue)
                .ToList();

            // Daily revenue trend (last 7 days for any period)
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => today.AddDays(-6 + i))
                .ToList();

            var dailyRevenue = last7Days.Select(date =>
            {
                var dayStart = date;
                var dayEnd = date.AddDays(1);
                var dayRentals = periodRentals.Where(r => r.RentalDate >= dayStart && r.RentalDate < dayEnd);
                return new
                {
                    Date = date.ToString("dd/MM"),
                    Revenue = dayRentals.Sum(r => r.RentalFee + r.FineAmount)
                };
            }).ToList();

            return Ok(new
            {
                period,
                startDate = startDate.ToString("yyyy-MM-dd"),
                endDate = endDate.ToString("yyyy-MM-dd"),
                summary = new
                {
                    totalRevenue,
                    totalRentals,
                    activeCustomers,
                    avgRentalValue,
                    rentalIncome,
                    fineIncome,
                    completedRentals,
                    pendingRentals,
                    onTimeRate,
                    totalBooks,
                    rentedBooks,
                    utilizationRate
                },
                topBooks,
                topCustomers,
                categoryRevenue,
                dailyRevenue
            });
        }

        [HttpGet("database-stats")]
        public async Task<IActionResult> GetDatabaseStats()
        {
            var totalBooks = await _context.Books.CountAsync();
            var totalCustomers = await _context.Customers.CountAsync();
            var totalRentals = await _context.Rentals.CountAsync();
            var totalFines = await _context.Fines.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();
            var totalEmployees = await _context.Employees.CountAsync();
            var totalSettings = await _context.Settings.CountAsync();
            var totalDailyReports = await _context.DailyReports.CountAsync();

            // Active counts
            var activeRentals = await _context.Rentals.CountAsync(r => r.Status == "Active");
            var unpaidFines = await _context.Fines.CountAsync(f => f.Status == "Unpaid");

            // Book status breakdown
            var availableBooks = await _context.Books.CountAsync(b => b.Status == "Available");
            var rentedBooks = await _context.Books.CountAsync(b => b.Status == "Rented");
            var damagedBooks = await _context.Books.CountAsync(b => b.Status == "Damaged");

            return Ok(new
            {
                tables = new
                {
                    categories = new { total = totalCategories },
                    books = new { total = totalBooks, available = availableBooks, rented = rentedBooks, damaged = damagedBooks },
                    customers = new { total = totalCustomers },
                    employees = new { total = totalEmployees },
                    rentals = new { total = totalRentals, active = activeRentals },
                    fines = new { total = totalFines, unpaid = unpaidFines },
                    settings = new { total = totalSettings },
                    dailyReports = new { total = totalDailyReports }
                },
                summary = new
                {
                    totalBooks,
                    totalCustomers,
                    activeRentals,
                    unpaidFines
                }
            });
        }
    }
}