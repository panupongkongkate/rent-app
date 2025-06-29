using System;
using System.ComponentModel.DataAnnotations;

namespace ComicRental.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Address { get; set; }
        
        [StringLength(20)]
        public string? IdCard { get; set; }
        
        [StringLength(100)]
        public string? Email { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public int TotalBorrowed { get; set; } = 0;
        
        public decimal TotalFines { get; set; } = 0;
        
        [StringLength(500)]
        public string? ProfileImageUrl { get; set; }
    }
}