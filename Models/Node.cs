using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace K8Intel.Models
{
    public class Node
    {
        public int Id { get; set; }
        public int ClusterId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string KubeletVersion { get; set; } = string.Empty;
        public string OsImage { get; set; } = string.Empty;
        
        // Navigation properties
        public Cluster Cluster { get; set; }
        public ICollection<Pod> Pods { get; set; } = new List<Pod>();
    }
}