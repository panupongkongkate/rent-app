using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComicRental.Models
{
    public class DailyReport
    {
        [Key]
        public DateTime ReportDate { get; set; }
        
        public int TotalRentals { get; set; } = 0;
        
        public int TotalReturns { get; set; } = 0;
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalRevenue { get; set; } = 0;
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalFines { get; set; } = 0;
        
        public int NewCustomers { get; set; } = 0;
        
        public int StaffId { get; set; }
        
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        
        [ForeignKey("StaffId")]
        public virtual Employee? Staff { get; set; }
    }
}