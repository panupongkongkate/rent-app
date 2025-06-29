using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ComicRental.Data;
using ComicRental.Models;
using ComicRental.Models.DTOs;

namespace ComicRental.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ComicRentalContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ComicRentalContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Username == loginDto.Username && e.Status == "Active");

            if (employee == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, employee.PasswordHash))
            {
                return Unauthorized(new { message = "Username หรือ Password ไม่ถูกต้อง" });
            }

            employee.LastLogin = DateTime.Now;
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(employee);

            return Ok(new LoginResponseDto
            {
                Token = token,
                Username = employee.Username,
                FullName = employee.FullName,
                Role = employee.Role,
                EmployeeId = employee.EmployeeId
            });
        }

        private string GenerateJwtToken(Employee employee)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "ComicRentalSecretKey2024"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, employee.EmployeeId.ToString()),
                new Claim(ClaimTypes.Name, employee.Username),
                new Claim(ClaimTypes.Role, employee.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "ComicRentalAPI",
                audience: _configuration["Jwt:Audience"] ?? "ComicRentalClient",
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}