using System;

namespace HashGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            string adminPassword = "admin123";
            string staffPassword = "staff123";
            
            string adminHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);
            string staffHash = BCrypt.Net.BCrypt.HashPassword(staffPassword);
            
            Console.WriteLine($"Admin password 'admin123' hash: {adminHash}");
            Console.WriteLine($"Staff password 'staff123' hash: {staffHash}");
            
            // Verify
            Console.WriteLine($"Admin hash verification: {BCrypt.Net.BCrypt.Verify(adminPassword, adminHash)}");
            Console.WriteLine($"Staff hash verification: {BCrypt.Net.BCrypt.Verify(staffPassword, staffHash)}");
        }
    }
}