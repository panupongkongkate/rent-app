using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ComicRental.Data;
using ComicRental.Models;
using ComicRental.Models.DTOs;

namespace ComicRental.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RentalsController : ControllerBase
    {
        private readonly ComicRentalContext _context;

        public RentalsController(ComicRentalContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetRentals(int page = 1, int pageSize = 10, string? status = null, DateTime? date = null)
        {
            var query = _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Book)
                .ThenInclude(b => b!.Category)
                .Include(r => r.Staff)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status == status);
            }

            if (date.HasValue)
            {
                var startDate = date.Value.Date;
                var endDate = startDate.AddDays(1);
                query = query.Where(r => r.RentalDate >= startDate && r.RentalDate < endDate);
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var rentals = await query
                .OrderByDescending(r => r.RentalDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    r.RentalId,
                    r.CustomerId,
                    CustomerName = r.Customer!.FullName,
                    CustomerPhone = r.Customer.Phone,
                    r.BookId,
                    BookTitle = r.Book!.Title,
                    BookAuthor = r.Book.Author,
                    CategoryName = r.Book.Category!.CategoryName,
                    r.RentalDate,
                    r.DueDate,
                    r.ReturnDate,
                    r.RentalDays,
                    r.RentalFee,
                    r.FineAmount,
                    r.TotalAmount,
                    r.Status,
                    StaffName = r.Staff!.FullName,
                    r.Notes
                })
                .ToListAsync();

            return Ok(new
            {
                data = rentals,
                page,
                pageSize,
                totalItems,
                totalPages
            });
        }

        [HttpPost("borrow")]
        public async Task<IActionResult> BorrowBook([FromBody] BorrowBookDto borrowDto)
        {
            var staffId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Check if book is available
            var book = await _context.Books.FindAsync(borrowDto.BookId);
            if (book == null)
            {
                return NotFound(new { message = "ไม่พบหนังสือ" });
            }

            if (book.Status != "Available")
            {
                return BadRequest(new { message = "หนังสือไม่พร้อมให้ยืม" });
            }

            // Check customer
            var customer = await _context.Customers.FindAsync(borrowDto.CustomerId);
            if (customer == null)
            {
                return NotFound(new { message = "ไม่พบข้อมูลลูกค้า" });
            }

            // Check max books limit
            var maxBooksPerCustomer = await _context.Settings
                .Where(s => s.SettingKey == "MaxBooksPerCustomer")
                .Select(s => s.SettingValue)
                .FirstOrDefaultAsync() ?? "5";

            var currentBorrowedCount = await _context.Rentals
                .CountAsync(r => r.CustomerId == borrowDto.CustomerId && r.Status == "Active");

            if (currentBorrowedCount >= int.Parse(maxBooksPerCustomer))
            {
                return BadRequest(new { message = $"ลูกค้ายืมหนังสือครบจำนวนสูงสุดแล้ว ({maxBooksPerCustomer} เล่ม)" });
            }

            // Get rental days setting
            var rentalDaysSetting = await _context.Settings
                .Where(s => s.SettingKey == "RentalDays")
                .Select(s => s.SettingValue)
                .FirstOrDefaultAsync() ?? "7";

            var rentalDays = borrowDto.RentalDays > 0 ? borrowDto.RentalDays : int.Parse(rentalDaysSetting);

            // Calculate rental fee with discount (only for admin)
            var isAdmin = User.IsInRole("Admin");
            var rentalFee = book.RentalPrice;
            var discountAmount = 0m;
            
            if (isAdmin && borrowDto.DiscountPercent > 0 && borrowDto.DiscountPercent <= 100)
            {
                discountAmount = rentalFee * (borrowDto.DiscountPercent / 100);
                rentalFee = rentalFee - discountAmount;
            }

            // Create rental
            var rental = new Rental
            {
                CustomerId = borrowDto.CustomerId,
                BookId = borrowDto.BookId,
                RentalDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(rentalDays),
                RentalDays = rentalDays,
                RentalFee = rentalFee,
                TotalAmount = rentalFee,
                Status = "Active",
                StaffId = staffId,
                Notes = borrowDto.Notes + (discountAmount > 0 ? $" (ส่วนลด {borrowDto.DiscountPercent}%)" : "")
            };

            _context.Rentals.Add(rental);

            // Update book status
            book.Status = "Rented";

            // Update customer stats
            customer.TotalBorrowed++;

            await _context.SaveChangesAsync();

            return Ok(new { message = "ยืมหนังสือสำเร็จ", rentalId = rental.RentalId });
        }

        [HttpPost("return")]
        public async Task<IActionResult> ReturnBook([FromBody] ReturnBookDto returnDto)
        {
            var staffId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var rental = await _context.Rentals
                .Include(r => r.Book)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.RentalId == returnDto.RentalId);

            if (rental == null)
            {
                return NotFound(new { message = "ไม่พบข้อมูลการยืม" });
            }

            if (rental.Status != "Active")
            {
                return BadRequest(new { message = "รายการนี้คืนหนังสือแล้ว" });
            }

            rental.ReturnDate = DateTime.Now;
            rental.Status = "Returned";

            // Calculate fine if late
            if (rental.ReturnDate > rental.DueDate)
            {
                var finePerDaySetting = await _context.Settings
                    .Where(s => s.SettingKey == "FinePerDay")
                    .Select(s => s.SettingValue)
                    .FirstOrDefaultAsync() ?? "10";

                var finePerDay = decimal.Parse(finePerDaySetting);
                var daysLate = Math.Max(0, (rental.ReturnDate.Value - rental.DueDate).Days);
                var fineAmount = daysLate * finePerDay;

                rental.FineAmount = fineAmount;
                rental.TotalAmount = rental.RentalFee + fineAmount;

                // Create fine record
                var fine = new Fine
                {
                    RentalId = rental.RentalId,
                    CustomerId = rental.CustomerId,
                    FineReason = $"คืนหนังสือช้า {daysLate} วัน",
                    DaysLate = daysLate,
                    FineRate = finePerDay,
                    FineAmount = fineAmount,
                    Remaining = fineAmount,
                    Status = "Unpaid",
                    StaffId = staffId
                };

                _context.Fines.Add(fine);

                // Update customer fine total
                rental.Customer!.TotalFines += fineAmount;
            }

            // Update book status
            rental.Book!.Status = "Available";

            if (!string.IsNullOrEmpty(returnDto.Notes))
            {
                rental.Notes = (rental.Notes ?? "") + $" | คืนหนังสือ: {returnDto.Notes}";
            }

            await _context.SaveChangesAsync();

            return Ok(new 
            { 
                message = "คืนหนังสือสำเร็จ", 
                fineAmount = rental.FineAmount,
                totalAmount = rental.TotalAmount
            });
        }

        [HttpPost("return-with-fine")]
        public async Task<IActionResult> ReturnWithFine([FromBody] ReturnWithFineDto returnDto)
        {
            try
            {
                var rental = await _context.Rentals
                    .Include(r => r.Book)
                    .Include(r => r.Customer)
                    .FirstOrDefaultAsync(r => r.RentalId == returnDto.RentalId && r.Status == "Rented");

                if (rental == null)
                {
                    return NotFound(new { message = "ไม่พบรายการยืมหนังสือ" });
                }

                // Update rental status
                rental.Status = "Returned";
                rental.ReturnDate = DateTime.Now;
                rental.FineAmount = returnDto.FineAmount;
                rental.TotalAmount = rental.RentalFee + rental.FineAmount;

                // Create fine record
                var daysLate = Math.Max(0, (int)(DateTime.Now - rental.DueDate).TotalDays);
                var fine = new Fine
                {
                    RentalId = rental.RentalId,
                    CustomerId = rental.CustomerId,
                    FineReason = daysLate > 0 ? $"ค่าปรับการคืนช้า {daysLate} วัน" : "ค่าปรับอื่นๆ",
                    DaysLate = daysLate,
                    FineRate = daysLate > 0 ? returnDto.FineAmount / daysLate : returnDto.FineAmount,
                    FineAmount = returnDto.FineAmount,
                    PaidAmount = returnDto.FineAmount,
                    Remaining = 0,
                    Status = "Paid",
                    CreatedDate = DateTime.Now,
                    PaidDate = DateTime.Now
                };

                _context.Fines.Add(fine);

                // Update book status
                rental.Book!.Status = "Available";

                // Update customer fine total
                rental.Customer!.TotalFines += returnDto.FineAmount;

                await _context.SaveChangesAsync();

                return Ok(new { message = "คืนหนังสือพร้อมค่าปรับสำเร็จ", fineAmount = returnDto.FineAmount });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "เกิดข้อผิดพลาดในการคืนหนังสือ", error = ex.Message });
            }
        }

        [HttpPost("force-return")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ForceReturn([FromBody] ForceReturnDto returnDto)
        {
            try
            {
                var rental = await _context.Rentals
                    .Include(r => r.Book)
                    .Include(r => r.Customer)
                    .FirstOrDefaultAsync(r => r.RentalId == returnDto.RentalId);

                if (rental == null)
                {
                    return NotFound(new { message = "ไม่พบรายการยืมหนังสือ" });
                }

                // Force return regardless of status
                rental.Status = "Returned";
                rental.ReturnDate = DateTime.Now;
                rental.Notes = (rental.Notes ?? "") + " | บังคับคืนโดยผู้ดูแลระบบ";

                // Update book status
                rental.Book!.Status = "Available";

                await _context.SaveChangesAsync();

                return Ok(new { message = "บังคับคืนหนังสือสำเร็จ" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "เกิดข้อผิดพลาดในการบังคับคืนหนังสือ", error = ex.Message });
            }
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetCustomerRentals(int customerId)
        {
            try
            {
                var rentals = await _context.Rentals
                    .Include(r => r.Book)
                    .Include(r => r.Customer)
                    .Where(r => r.CustomerId == customerId)
                    .OrderByDescending(r => r.RentalDate)
                    .Select(r => new
                    {
                        r.RentalId,
                        r.BookId,
                        BookTitle = r.Book!.Title,
                        BookAuthor = r.Book.Author,
                        r.RentalDate,
                        r.DueDate,
                        r.ReturnDate,
                        r.Status,
                        r.RentalFee,
                        r.FineAmount,
                        r.TotalAmount,
                        r.Notes
                    })
                    .ToListAsync();

                return Ok(rentals);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "เกิดข้อผิดพลาดในการดึงประวัติการยืม", error = ex.Message });
            }
        }

        [HttpGet("recent-activities")]
        public async Task<IActionResult> GetRecentActivities()
        {
            try
            {
                var recentRentals = await _context.Rentals
                    .Include(r => r.Book)
                    .Include(r => r.Customer)
                    .Where(r => r.RentalDate >= DateTime.Today.AddDays(-7))
                    .OrderByDescending(r => r.RentalDate)
                    .Take(20)
                    .Select(r => new
                    {
                        Type = "Rental",
                        r.RentalId,
                        BookTitle = r.Book!.Title,
                        CustomerName = r.Customer!.FullName,
                        Date = r.RentalDate,
                        Status = "Active"  // Always show Active for rental activities
                    })
                    .ToListAsync();

                var recentReturns = await _context.Rentals
                    .Include(r => r.Book)
                    .Include(r => r.Customer)
                    .Where(r => r.ReturnDate != null && r.ReturnDate >= DateTime.Today.AddDays(-7))
                    .OrderByDescending(r => r.ReturnDate)
                    .Take(20)
                    .Select(r => new
                    {
                        Type = "Return",
                        r.RentalId,
                        BookTitle = r.Book!.Title,
                        CustomerName = r.Customer!.FullName,
                        Date = r.ReturnDate!.Value,
                        Status = "Returned"  // Always show Returned for return activities
                    })
                    .ToListAsync();

                var activities = recentRentals.Concat(recentReturns)
                    .OrderByDescending(a => a.Date)
                    .Take(20)
                    .ToList();

                return Ok(activities);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "เกิดข้อผิดพลาดในการดึงกิจกรรมล่าสุด", error = ex.Message });
            }
        }
    }
}