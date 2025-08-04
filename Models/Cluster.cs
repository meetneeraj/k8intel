using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace K8Intel.Models
{
    public class Cluster
    {
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        [Required]
        public string ?ApiEndpoint { get; set; }
        public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
        public ICollection<ClusterMetric> Metrics { get; set; } = new List<ClusterMetric>();
    }
}