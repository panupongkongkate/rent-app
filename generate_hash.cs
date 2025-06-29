using System;

class Program 
{
    static void Main() 
    {
        string password1 = "admin123";
        string password2 = "staff123";
        
        string hash1 = BCrypt.Net.BCrypt.HashPassword(password1);
        string hash2 = BCrypt.Net.BCrypt.HashPassword(password2);
        
        Console.WriteLine($"admin123: {hash1}");
        Console.WriteLine($"staff123: {hash2}");
        
        // Test verification
        Console.WriteLine($"Verify admin123: {BCrypt.Net.BCrypt.Verify(password1, hash1)}");
        Console.WriteLine($"Verify staff123: {BCrypt.Net.BCrypt.Verify(password2, hash2)}");
    }
}