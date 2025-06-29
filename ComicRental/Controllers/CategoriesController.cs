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
    public class CategoriesController : ControllerBase
    {
        private readonly ComicRentalContext _context;

        public CategoriesController(ComicRentalContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .Where(c => c.Status == "Active")
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return Ok(categories);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory([FromBody] Category category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            category.Status = "Active";
            
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(category);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
        {
            if (id != category.CategoryId)
            {
                return BadRequest(new { message = "ID ไม่ตรงกัน" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
            {
                return NotFound(new { message = "ไม่พบหมวดหมู่" });
            }

            existingCategory.CategoryName = category.CategoryName;
            existingCategory.Description = category.Description;
            existingCategory.ColorCode = category.ColorCode;

            await _context.SaveChangesAsync();

            return Ok(existingCategory);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new { message = "ไม่พบหมวดหมู่" });
            }

            // Check if there are books in this category
            var hasBooks = await _context.Books.AnyAsync(b => b.CategoryId == id);
            if (hasBooks)
            {
                // Set status to inactive instead of deleting
                category.Status = "Inactive";
                await _context.SaveChangesAsync();
                return Ok(new { message = "หมวดหมู่ถูกเปลี่ยนสถานะเป็น Inactive เนื่องจากมีหนังสือในหมวดนี้" });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}