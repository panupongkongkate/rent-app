using System;
using System.ComponentModel.DataAnnotations;

namespace ComicRental.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string Role { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? Phone { get; set; }
        
        [StringLength(100)]
        public string? Email { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? LastLogin { get; set; }
    }
}