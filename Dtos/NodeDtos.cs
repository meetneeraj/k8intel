using System.Collections.Generic;
namespace K8Intel.Dtos
{
    // NodeDto does not need a list of Pods
    public record NodeDto(int Id, int ClusterId, string Name, string Status, string KubeletVersion, string OsImage);
    
    // PodDto needs the name of the node it's running on
    public record PodDto(int Id, int NodeId, string Name, string NodeName, string Namespace, string Status, string Image, double CpuRequest, double MemoryRequest);
    
    // Inventory Report just contains two flat lists
    public record InventoryReportDto(List<NodeDto> Nodes, List<PodDto> Pods);
}