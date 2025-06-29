using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComicRental.Models
{
    public class Rental
    {
        [Key]
        public int RentalId { get; set; }
        
        public int CustomerId { get; set; }
        
        public int BookId { get; set; }
        
        public DateTime RentalDate { get; set; } = DateTime.Now;
        
        public DateTime DueDate { get; set; }
        
        public DateTime? ReturnDate { get; set; }
        
        public int RentalDays { get; set; }
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal RentalFee { get; set; }
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal FineAmount { get; set; } = 0;
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalAmount { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";
        
        public int StaffId { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }
        
        [ForeignKey("BookId")]
        public virtual Book? Book { get; set; }
        
        [ForeignKey("StaffId")]
        public virtual Employee? Staff { get; set; }
    }
}