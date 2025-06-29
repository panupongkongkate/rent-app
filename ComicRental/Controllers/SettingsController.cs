using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ComicRental.Data;

namespace ComicRental.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        private readonly ComicRentalContext _context;

        public SettingsController(ComicRentalContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            var settings = await _context.Settings.ToListAsync();
            return Ok(settings);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSettings([FromBody] List<SettingUpdateDto> settingsDto)
        {
            try
            {
                foreach (var settingDto in settingsDto)
                {
                    var setting = await _context.Settings.FindAsync(settingDto.SettingKey);
                    if (setting != null)
                    {
                        setting.SettingValue = settingDto.SettingValue;
                        setting.UpdatedDate = DateTime.Now;
                        // Note: UpdatedBy should be set based on authenticated user
                        // setting.UpdatedBy = getCurrentUserId();
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "อัพเดทการตั้งค่าสำเร็จ" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "เกิดข้อผิดพลาดในการอัพเดทการตั้งค่า", error = ex.Message });
            }
        }
    }

    public class SettingUpdateDto
    {
        public string SettingKey { get; set; } = string.Empty;
        public string SettingValue { get; set; } = string.Empty;
    }
}