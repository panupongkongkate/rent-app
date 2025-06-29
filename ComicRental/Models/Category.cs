using System.ComponentModel.DataAnnotations;

namespace ComicRental.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string CategoryName { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        [StringLength(7)]
        public string? ColorCode { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";
    }
}