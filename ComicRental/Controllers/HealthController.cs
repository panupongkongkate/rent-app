using Microsoft.AspNetCore.Mvc;

namespace ComicRental.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetHealth()
        {
            return Ok(new 
            { 
                status = "OK", 
                message = "Comic Rental API is running",
                timestamp = DateTime.Now
            });
        }
    }
}