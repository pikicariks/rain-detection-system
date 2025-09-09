using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RainDetectionApp.Models
{
    public class SystemSettings
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string SettingName { get; set; } = string.Empty;
        
        [Required]
        public string SettingValue { get; set; } = string.Empty;
        
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        
        [StringLength(200)]
        public string? Description { get; set; }
    }
}