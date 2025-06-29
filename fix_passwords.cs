using System;
using BCrypt.Net;

class Program 
{
    static void Main() 
    {
        // สร้าง hash สำหรับ admin123 และ staff123
        string adminPassword = "admin123";
        string staffPassword = "staff123";
        
        string adminHash = BCrypt.HashPassword(adminPassword);
        string staffHash = BCrypt.HashPassword(staffPassword);
        
        Console.WriteLine("=== Password Hashes ===");
        Console.WriteLine($"admin123 hash: {adminHash}");
        Console.WriteLine($"staff123 hash: {staffHash}");
        
        // ทดสอบการ verify
        Console.WriteLine("\n=== Verification Test ===");
        Console.WriteLine($"admin123 verify: {BCrypt.Verify(adminPassword, adminHash)}");
        Console.WriteLine($"staff123 verify: {BCrypt.Verify(staffPassword, staffHash)}");
        
        Console.WriteLine("\n=== SQL Commands ===");
        Console.WriteLine($"UPDATE Employees SET PasswordHash = '{adminHash}' WHERE Username = 'admin01';");
        Console.WriteLine($"UPDATE Employees SET PasswordHash = '{staffHash}' WHERE Username = 'staff01';");
    }
}