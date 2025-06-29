using Microsoft.EntityFrameworkCore;
using ComicRental.Models;

namespace ComicRental.Data
{
    public class ComicRentalContext : DbContext
    {
        public ComicRentalContext(DbContextOptions<ComicRentalContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<Fine> Fines { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<DailyReport> DailyReports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed data for Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, CategoryName = "Action", Description = "แอ็กชั่น", ColorCode = "#FF5722", Status = "Active" },
                new Category { CategoryId = 2, CategoryName = "Romance", Description = "โรแมนติก", ColorCode = "#E91E63", Status = "Active" },
                new Category { CategoryId = 3, CategoryName = "Comedy", Description = "ตลก", ColorCode = "#FFC107", Status = "Active" },
                new Category { CategoryId = 4, CategoryName = "Drama", Description = "ดราม่า", ColorCode = "#9C27B0", Status = "Active" },
                new Category { CategoryId = 5, CategoryName = "Fantasy", Description = "แฟนตาซี", ColorCode = "#3F51B5", Status = "Active" },
                new Category { CategoryId = 6, CategoryName = "Horror", Description = "สยองขวัญ", ColorCode = "#607D8B", Status = "Active" },
                new Category { CategoryId = 7, CategoryName = "Sports", Description = "กีฬา", ColorCode = "#4CAF50", Status = "Active" },
                new Category { CategoryId = 8, CategoryName = "Sci-Fi", Description = "ไซไฟ", ColorCode = "#00BCD4", Status = "Active" }
            );

            // Seed data for Employees
            modelBuilder.Entity<Employee>().HasData(
                new Employee 
                { 
                    EmployeeId = 1, 
                    Username = "admin01", 
                    PasswordHash = "$2a$11$K5R8CvB6gCKwrqQGsxNfduEXKjXPqLgfJKKiJQiKsrfJhkPQkqYx2", // admin123
                    Role = "Admin",
                    FullName = "ผู้ดูแลระบบ",
                    Phone = "0812345678",
                    Email = "admin@comicrent.com",
                    Status = "Active",
                    CreatedDate = new DateTime(2024, 1, 1)
                },
                new Employee 
                { 
                    EmployeeId = 2, 
                    Username = "staff01", 
                    PasswordHash = "$2a$11$K5R8CvB6gCKwrqQGsxNfduEXKjXPqLgfJKKiJQiKsrfJhkPQkqYx2", // staff123
                    Role = "Staff",
                    FullName = "พนักงานคนที่ 1",
                    Phone = "0823456789",
                    Email = "staff01@comicrent.com",
                    Status = "Active",
                    CreatedDate = new DateTime(2024, 1, 1)
                }
            );

            // Seed data for Settings
            modelBuilder.Entity<Setting>().HasData(
                new Setting { SettingKey = "RentalDays", SettingValue = "7", Description = "จำนวนวันที่ให้ยืม", UpdatedBy = 1, UpdatedDate = new DateTime(2024, 1, 1) },
                new Setting { SettingKey = "FinePerDay", SettingValue = "10", Description = "ค่าปรับต่อวัน (บาท)", UpdatedBy = 1, UpdatedDate = new DateTime(2024, 1, 1) },
                new Setting { SettingKey = "MaxBooksPerCustomer", SettingValue = "5", Description = "จำนวนหนังสือสูงสุดที่ยืมได้ต่อคน", UpdatedBy = 1, UpdatedDate = new DateTime(2024, 1, 1) }
            );

            // Seed data for Sample Books
            modelBuilder.Entity<Book>().HasData(
                new Book { BookId = 1, Title = "One Piece เล่ม 1", Author = "Eiichiro Oda", CategoryId = 1, Publisher = "Shueisha", Volume = 1, ShelfLocation = "A1", RentalPrice = 10, Status = "Available", CreatedDate = new DateTime(2024, 1, 1) },
                new Book { BookId = 2, Title = "Naruto เล่ม 1", Author = "Masashi Kishimoto", CategoryId = 1, Publisher = "Shueisha", Volume = 1, ShelfLocation = "A2", RentalPrice = 10, Status = "Available", CreatedDate = new DateTime(2024, 1, 1) },
                new Book { BookId = 3, Title = "Attack on Titan เล่ม 1", Author = "Hajime Isayama", CategoryId = 1, Publisher = "Kodansha", Volume = 1, ShelfLocation = "A3", RentalPrice = 15, Status = "Available", CreatedDate = new DateTime(2024, 1, 1) },
                new Book { BookId = 4, Title = "Kimi ni Todoke เล่ม 1", Author = "Karuho Shiina", CategoryId = 2, Publisher = "Shueisha", Volume = 1, ShelfLocation = "B1", RentalPrice = 10, Status = "Available", CreatedDate = new DateTime(2024, 1, 1) },
                new Book { BookId = 5, Title = "Slam Dunk เล่ม 1", Author = "Takehiko Inoue", CategoryId = 7, Publisher = "Shueisha", Volume = 1, ShelfLocation = "C1", RentalPrice = 10, Status = "Available", CreatedDate = new DateTime(2024, 1, 1) }
            );

            // Seed data for Sample Customers
            modelBuilder.Entity<Customer>().HasData(
                new Customer { CustomerId = 1, FullName = "สมชาย ใจดี", Phone = "0891234567", Address = "123 ถนนสุขุมวิท", IdCard = "1234567890123", Status = "Active", CreatedDate = new DateTime(2024, 1, 1) },
                new Customer { CustomerId = 2, FullName = "สมหญิง รักอ่าน", Phone = "0892345678", Address = "456 ถนนพระราม 4", IdCard = "2345678901234", Status = "Active", CreatedDate = new DateTime(2024, 1, 1) }
            );
        }
    }
}