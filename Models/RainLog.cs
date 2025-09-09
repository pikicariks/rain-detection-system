using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RainDetectionApp.Models
{
    public class RainLog
    {
        public int Id { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string EventType { get; set; } = string.Empty; // "rain_start", "rain_stop", "manual_servo", etc.

        public int AnalogValue { get; set; }
        public int DigitalValue { get; set; }
        public bool IsRaining { get; set; }
        public int ServoPosition { get; set; }

        [StringLength(200)]
        public string? Notes { get; set; }
        
        public long? Distance { get; set; }
    }
}