using K8Intel.Dtos;
using K8Intel.Dtos.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace K8Intel.Interfaces
{
    public interface IMetricService
    {
        Task<PagedResult<MetricDto>> GetMetricsByClusterIdAsync(
        int clusterId, int pageNumber, int pageSize, string? metricType,
        DateTime? startDate, DateTime? endDate);
        Task<MetricDto> CreateMetricAsync(CreateMetricDto createDto);
    }
}