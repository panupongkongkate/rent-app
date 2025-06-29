using System;
using System.ComponentModel.DataAnnotations;

namespace ComicRental.Models
{
    public class Setting
    {
        [Key]
        [StringLength(50)]
        public string SettingKey { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string SettingValue { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        public int? UpdatedBy { get; set; }
        
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}