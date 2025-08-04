using System;
using System.ComponentModel.DataAnnotations;

namespace K8Intel.Models
{
    public class Alert
    {
        public int Id { get; set; }
        public int ClusterId { get; set; }
        [Required]
        public string? Severity { get; set; } // e.g., "Critical", "Warning", "Info"
        [Required]
        public string? Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsResolved { get; set; } = false;
        public DateTime? ResolvedAt { get; set; }

        public Cluster? Cluster { get; set; }
    }
}