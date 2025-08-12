using System;
using System.ComponentModel.DataAnnotations;

namespace K8Intel.Models
{
    // Represents a correlated group of alerts
    public class Incident
    {
        public int Id { get; set; }
        public int ClusterId { get; set; }
        [Required]
        public string Type { get; set; } // e.g., "HighCpu", "PodCrashLooping"
        [Required]
        public string Fingerprint { get; set; } // A unique hash to identify this type of incident
        public string Status { get; set; } = "Active"; // Active, Resolved
        public DateTime FirstSeenAt { get; set; }
        public DateTime LastSeenAt { get; set; }
        public int AlertCount { get; set; }
        public Cluster Cluster { get; set; }
    }
}