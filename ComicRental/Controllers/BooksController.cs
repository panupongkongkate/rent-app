using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ComicRental.Data;
using ComicRental.Models;

namespace ComicRental.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BooksController : ControllerBase
    {
        private readonly ComicRentalContext _context;

        public BooksController(ComicRentalContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetBooks(int page = 1, int pageSize = 10, string? status = null)
        {
            var query = _context.Books.Include(b => b.Category).AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(b => b.Status == status);
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var books = await query
                .OrderBy(b => b.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new
                {
                    b.BookId,
                    b.Title,
                    b.Author,
                    b.CategoryId,
                    CategoryName = b.Category!.CategoryName,
                    b.Isbn,
                    b.Publisher,
                    b.Volume,
                    b.ShelfLocation,
                    b.RentalPrice,
                    b.Condition,
                    b.Status,
                    CoverImageUrl = b.CoverImageData != null ? $"http://localhost:5081/api/books/{b.BookId}/image" : (string.IsNullOrEmpty(b.CoverImageUrl) ? null : (b.CoverImageUrl.StartsWith("http") ? b.CoverImageUrl : $"http://localhost:5081{b.CoverImageUrl}"))
                })
                .ToListAsync();

            return Ok(new
            {
                data = books,
                page,
                pageSize,
                totalItems,
                totalPages
            });
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchBooks(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { message = "กรุณาระบุคำค้นหา" });
            }

            var books = await _context.Books
                .Include(b => b.Category)
                .Where(b => b.Title.Contains(query) || 
                           b.Author.Contains(query) || 
                           (b.Isbn != null && b.Isbn.Contains(query)))
                .Select(b => new
                {
                    b.BookId,
                    b.Title,
                    b.Author,
                    b.CategoryId,
                    CategoryName = b.Category!.CategoryName,
                    b.Isbn,
                    b.Publisher,
                    b.Volume,
                    b.ShelfLocation,
                    b.RentalPrice,
                    b.Condition,
                    b.Status,
                    CoverImageUrl = b.CoverImageData != null ? $"http://localhost:5081/api/books/{b.BookId}/image" : (string.IsNullOrEmpty(b.CoverImageUrl) ? null : (b.CoverImageUrl.StartsWith("http") ? b.CoverImageUrl : $"http://localhost:5081{b.CoverImageUrl}"))
                })
                .ToListAsync();

            return Ok(books);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateBook([FromBody] Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            book.CreatedDate = DateTime.Now;
            book.Status = "Available";

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBooks), new { id = book.BookId }, book);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] Book book)
        {
            if (id != book.BookId)
            {
                return BadRequest(new { message = "Book ID ไม่ตรงกัน" });
            }

            _context.Entry(book).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await BookExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPost("{id}/upload-image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadBookImage(int id, IFormFile image)
        {
            try
            {
                var book = await _context.Books.FindAsync(id);
                if (book == null)
                {
                    return NotFound(new { message = "ไม่พบหนังสือ" });
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

                // แปลงรูปเป็น byte array
                using (var memoryStream = new MemoryStream())
                {
                    await image.CopyToAsync(memoryStream);
                    book.CoverImageData = memoryStream.ToArray();
                    book.CoverImageMimeType = image.ContentType;
                    // ลบ URL เดิม (ไม่ใช้แล้ว)
                    book.CoverImageUrl = null;
                }

                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "อัพโหลดรูปภาพสำเร็จ", 
                    imageUrl = $"http://localhost:5081/api/books/{id}/image" // URL สำหรับดึงรูปจาก BLOB
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "เกิดข้อผิดพลาดในการอัพโหลดรูปภาพ", error = ex.Message });
            }
        }

        [HttpGet("{id}/image")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBookImage(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null || book.CoverImageData == null)
            {
                return NotFound();
            }

            return File(book.CoverImageData, book.CoverImageMimeType ?? "image/jpeg");
        }

        [HttpDelete("{id}/image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBookImage(int id)
        {
            try
            {
                var book = await _context.Books.FindAsync(id);
                if (book == null)
                {
                    return NotFound(new { message = "ไม่พบหนังสือ" });
                }

                if (book.CoverImageData == null)
                {
                    return BadRequest(new { message = "หนังสือนี้ไม่มีรูปภาพ" });
                }

                // ลบข้อมูลรูปภาพ
                book.CoverImageData = null;
                book.CoverImageMimeType = null;
                book.CoverImageUrl = null;
                await _context.SaveChangesAsync();

                return Ok(new { message = "ลบรูปภาพสำเร็จ" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "เกิดข้อผิดพลาดในการลบรูปภาพ", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            var hasRentals = await _context.Rentals.AnyAsync(r => r.BookId == id);
            if (hasRentals)
            {
                book.Status = "Inactive";
                await _context.SaveChangesAsync();
                return Ok(new { message = "หนังสือถูกเปลี่ยนสถานะเป็น Inactive เนื่องจากมีประวัติการยืม" });
            }

            // ลบรูปภาพก่อนลบหนังสือ
            if (!string.IsNullOrEmpty(book.CoverImageUrl))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", book.CoverImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> BookExists(int id)
        {
            return await _context.Books.AnyAsync(e => e.BookId == id);
        }
    }
}