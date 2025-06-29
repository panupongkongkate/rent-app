using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ComicRental.Data;
using ComicRental.Models;
using ComicRental.Models.DTOs;
using System.Text;

namespace ComicRental.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly ComicRentalContext _context;

        public CustomersController(ComicRentalContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomers(int page = 1, int pageSize = 10)
        {
            var totalItems = await _context.Customers.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var customers = await _context.Customers
                .Where(c => c.Status == "Active")
                .OrderBy(c => c.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                data = customers,
                page,
                pageSize,
                totalItems,
                totalPages
            });
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchCustomers(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { message = "กรุณาระบุคำค้นหา" });
            }

            var customers = await _context.Customers
                .Where(c => c.Status == "Active" && 
                           (c.FullName.Contains(query) || 
                            c.Phone.Contains(query) || 
                            (c.IdCard != null && c.IdCard.Contains(query))))
                .ToListAsync();

            return Ok(customers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer == null)
            {
                return NotFound();
            }

            var rentals = await _context.Rentals
                .Include(r => r.Book)
                .Where(r => r.CustomerId == id)
                .OrderByDescending(r => r.RentalDate)
                .Take(10)
                .Select(r => new
                {
                    r.RentalId,
                    r.BookId,
                    BookTitle = r.Book!.Title,
                    r.RentalDate,
                    r.DueDate,
                    r.ReturnDate,
                    r.Status,
                    r.TotalAmount
                })
                .ToListAsync();

            return Ok(new
            {
                customer,
                recentRentals = rentals
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            customer.CreatedDate = DateTime.Now;
            customer.Status = "Active";

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCustomer), new { id = customer.CustomerId }, customer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] Customer customer)
        {
            if (id != customer.CustomerId)
            {
                return BadRequest(new { message = "Customer ID ไม่ตรงกัน" });
            }

            _context.Entry(customer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CustomerExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPost("{id}/upload-image")]
        public async Task<IActionResult> UploadCustomerImage(int id, IFormFile image)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    return NotFound(new { message = "ไม่พบลูกค้า" });
                }

                if (image == null || image.Length == 0)
                {
                    return BadRequest(new { message = "กรุณาเลือกไฟล์รูปภาพ" });
                }

                // ตรวจสอบชนิดไฟล์
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(image.ContentType.ToLower()))
                {
                    return BadRequest(new { message = "รองรับเฉพาะไฟล์ JPG, PNG, GIF เท่านั้น" });
                }

                // ตรวจสอบขนาดไฟล์ (5MB)
                if (image.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { message = "ขนาดไฟล์ต้องไม่เกิน 5MB" });
                }

                // สร้างโฟลเดอร์สำหรับเก็บรูปภาพ
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "customers");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // สร้างชื่อไฟล์ใหม่
                var fileExtension = Path.GetExtension(image.FileName);
                var fileName = $"{id}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // ลบรูปเดิมถ้ามี
                if (!string.IsNullOrEmpty(customer.ProfileImageUrl))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", customer.ProfileImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // บันทึกไฟล์ใหม่
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // อัพเดท URL ในฐานข้อมูล
                customer.ProfileImageUrl = $"/uploads/customers/{fileName}";
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "อัพโหลดรูปภาพสำเร็จ", 
                    imageUrl = customer.ProfileImageUrl 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "เกิดข้อผิดพลาดในการอัพโหลดรูปภาพ", error = ex.Message });
            }
        }

        [HttpDelete("{id}/image")]
        public async Task<IActionResult> DeleteCustomerImage(int id)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    return NotFound(new { message = "ไม่พบลูกค้า" });
                }

                if (string.IsNullOrEmpty(customer.ProfileImageUrl))
                {
                    return BadRequest(new { message = "ลูกค้านี้ไม่มีรูปภาพ" });
                }

                // ลบไฟล์รูปภาพ
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", customer.ProfileImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // อัพเดทฐานข้อมูล
                customer.ProfileImageUrl = null;
                await _context.SaveChangesAsync();

                return Ok(new { message = "ลบรูปภาพสำเร็จ" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "เกิดข้อผิดพลาดในการลบรูปภาพ", error = ex.Message });
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCustomerStatus(int id, [FromBody] CustomerStatusDto statusDto)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    return NotFound(new { message = "ไม่พบลูกค้า" });
                }

                customer.Status = statusDto.Status;
                await _context.SaveChangesAsync();

                return Ok(new { message = "อัพเดทสถานะสำเร็จ", customer });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "เกิดข้อผิดพลาดในการอัพเดทสถานะ", error = ex.Message });
            }
        }

        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportCustomersCSV()
        {
            try
            {
                var customers = await _context.Customers
                    .Where(c => c.Status == "Active")
                    .OrderBy(c => c.FullName)
                    .ToListAsync();

                var csvContent = new StringBuilder();
                csvContent.AppendLine("ชื่อ-นามสกุล,เบอร์โทร,อีเมล,ที่อยู่,เลขบัตรประชาชน,วันที่สมัคร");

                foreach (var customer in customers)
                {
                    csvContent.AppendLine($"\"{customer.FullName}\"," +
                                        $"\"{customer.Phone}\"," +
                                        $"\"{customer.Email ?? ""}\"," +
                                        $"\"{customer.Address ?? ""}\"," +
                                        $"\"{customer.IdCard ?? ""}\"," +
                                        $"\"{customer.CreatedDate:yyyy-MM-dd}\"");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent.ToString());
                return File(bytes, "text/csv", $"customers_{DateTime.Now:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "เกิดข้อผิดพลาดในการส่งออกข้อมูล", error = ex.Message });
            }
        }

        private async Task<bool> CustomerExists(int id)
        {
            return await _context.Customers.AnyAsync(e => e.CustomerId == id);
        }
    }
}