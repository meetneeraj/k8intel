using System;
using System.ComponentModel.DataAnnotations;

namespace K8Intel.Models
{
    public class ClusterMetric
    {
        public int Id { get; set; }
        public int ClusterId { get; set; }
        [Required]
        public string? MetricType { get; set; } // "CPU", "Memory", "Disk"
        public double Value { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public Cluster? Cluster { get; set; }
    }
}