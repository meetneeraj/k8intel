using System.ComponentModel.DataAnnotations;

namespace K8Intel.Dtos
{
    public record CreateClusterDto([Required] string Name, [Required] string ApiEndpoint);

    public class ClusterDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ApiEndpoint { get; set; } = string.Empty;
        public string HealthStatus { get; set; } = string.Empty;
    }
}