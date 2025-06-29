using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComicRental.Models
{
    public class Fine
    {
        [Key]
        public int FineId { get; set; }
        
        public int RentalId { get; set; }
        
        public int CustomerId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FineReason { get; set; } = string.Empty;
        
        public int DaysLate { get; set; }
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal FineRate { get; set; }
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal FineAmount { get; set; }
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal PaidAmount { get; set; } = 0;
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Remaining { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Unpaid";
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? PaidDate { get; set; }
        
        public int? StaffId { get; set; }
        
        [ForeignKey("RentalId")]
        public virtual Rental? Rental { get; set; }
        
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }
        
        [ForeignKey("StaffId")]
        public virtual Employee? Staff { get; set; }
    }
}