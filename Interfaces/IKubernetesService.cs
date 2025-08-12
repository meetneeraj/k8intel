using K8Intel.Dtos;
using K8Intel.Dtos.Common;

namespace K8Intel.Interfaces;
public interface IKubernetesService
{
    Task ProcessInventoryReport(int clusterId, InventoryReportDto report);
    Task<List<NodeDto>> GetNodesByClusterIdAsync(int clusterId);
    Task<List<PodDto>> GetPodsByNodeIdAsync(int nodeId);
    Task<List<TimeSeriesDataPoint>> GetMetricSummaryAsync(int clusterId, string metricType, string timeSpan, string interval);
}