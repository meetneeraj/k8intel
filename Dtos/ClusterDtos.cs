using System.ComponentModel.DataAnnotations;

namespace K8Intel.Dtos
{
    public record CreateClusterDto([Required] string Name, [Required] string ApiEndpoint);
    public record ClusterDto(int Id, string Name, string ApiEndpoint);
}