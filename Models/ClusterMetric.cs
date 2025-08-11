using System;
using System.ComponentModel.DataAnnotations;

namespace K8Intel.Models
{
    public class ClusterMetric
    {
        private DateTime _timestamp; // private backing field

        public int Id { get; set; }
        public int ClusterId { get; set; }
        [Required]
        public string MetricType { get; set; } = string.Empty;
        public double Value { get; set; }

        // Public Timestamp property with UTC enforcement
        public DateTime Timestamp
        {
            get => _timestamp;
            set => _timestamp = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
        
        public Cluster? Cluster { get; set; }
        
        public ClusterMetric()
        {
            // Set default in constructor to ensure it's always UTC
            Timestamp = DateTime.UtcNow;
        }
    }
}