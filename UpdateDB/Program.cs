using Microsoft.Data.Sqlite;
using System;

class Program
{
    static void Main()
    {
        string connectionString = "Data Source=../ComicRental/comic_rental.db";
        
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        
        // Update admin01 password
        using var command1 = connection.CreateCommand();
        command1.CommandText = "UPDATE Employees SET PasswordHash = '$2a$11$C2q0s1aImT96hyEexizymuZH/kJVQLrVXc955tsvGT4jqU7jB39MO' WHERE Username = 'admin01';";
        int rows1 = command1.ExecuteNonQuery();
        
        // Update staff01 password
        using var command2 = connection.CreateCommand();
        command2.CommandText = "UPDATE Employees SET PasswordHash = '$2a$11$7vDNXlIq1qV6VzDiIFJJmOE7PpFB0gd3oOimU.9v9TPrP2or94cwy' WHERE Username = 'staff01';";
        int rows2 = command2.ExecuteNonQuery();
        
        Console.WriteLine($"Updated admin01: {rows1} rows");
        Console.WriteLine($"Updated staff01: {rows2} rows");
        
        // Verify updates
        using var command3 = connection.CreateCommand();
        command3.CommandText = "SELECT Username, PasswordHash FROM Employees WHERE Username IN ('admin01', 'staff01');";
        using var reader = command3.ExecuteReader();
        
        Console.WriteLine("\n=== Verification ===");
        while (reader.Read())
        {
            Console.WriteLine($"{reader["Username"]}: {reader["PasswordHash"]}");
        }
    }
}