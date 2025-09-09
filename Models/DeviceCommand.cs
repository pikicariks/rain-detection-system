using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RainDetectionApp.Models
{
    public class DeviceCommand
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string CommandType { get; set; } = string.Empty; // "servo_move", "toggle_system", "update_threshold"
        
        [Required]
        public string CommandData { get; set; } = string.Empty; // JSON data for the command
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExecutedAt { get; set; }
        public bool IsExecuted { get; set; } = false;
        public bool WasSuccessful { get; set; } = false;
        
        [StringLength(500)]
        public string? Response { get; set; }
    }
}