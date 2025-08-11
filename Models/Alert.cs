using System;
using System.ComponentModel.DataAnnotations;

namespace K8Intel.Models
{
    public class Alert
    {
        private DateTime _timestamp; // private backing field
        private DateTime? _resolvedAt; // private nullable backing field

        public int Id { get; set; }
        public int ClusterId { get; set; }
        [Required]
        public string Severity { get; set; } = string.Empty;
        [Required]
        public string Message { get; set; } = string.Empty;

        // Public Timestamp property with UTC enforcement
        public DateTime Timestamp
        {
            get => _timestamp;
            set => _timestamp = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
        
        public bool IsResolved { get; set; } = false;

        // Public ResolvedAt property with UTC enforcement
        public DateTime? ResolvedAt
        {
            get => _resolvedAt;
            set
            {
                if (value.HasValue)
                {
                    _resolvedAt = DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
                }
                else
                {
                    _resolvedAt = null;
                }
            }
        }
        
        public Cluster? Cluster { get; set; }

        public Alert()
        {
            // Set default in constructor to ensure it's always UTC
            Timestamp = DateTime.UtcNow;
        }
    }
}