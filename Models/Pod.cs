using System.ComponentModel.DataAnnotations;

namespace K8Intel.Models
{
    public class Pod
    {
        public int Id { get; set; }
        public int NodeId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty; // For simplicity, we'll store the primary image here
        public double CpuRequest { get; set; } // In millicores
        public double MemoryRequest { get; set; } // In MiB

        // Navigation property
        public Node Node { get; set; }
    }
}