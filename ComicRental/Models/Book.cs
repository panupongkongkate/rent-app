using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComicRental.Models
{
    public class Book
    {
        [Key]
        public int BookId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Author { get; set; } = string.Empty;
        
        public int CategoryId { get; set; }
        
        [StringLength(20)]
        public string? Isbn { get; set; }
        
        [StringLength(100)]
        public string? Publisher { get; set; }
        
        public int? Volume { get; set; }
        
        [StringLength(50)]
        public string? ShelfLocation { get; set; }
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal RentalPrice { get; set; }
        
        [StringLength(50)]
        public string Condition { get; set; } = "Good";
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Available";
        
        [StringLength(100)]
        public string? QrCode { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        [StringLength(500)]
        public string? CoverImageUrl { get; set; }
        
        // เก็บรูปเป็น Binary Data
        public byte[]? CoverImageData { get; set; }
        
        // เก็บ MIME type ของรูป
        [StringLength(50)]
        public string? CoverImageMimeType { get; set; }
        
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }
    }
}