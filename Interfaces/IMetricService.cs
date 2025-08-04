using K8Intel.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace K8Intel.Interfaces
{
    public interface IMetricService
    {
        Task<IEnumerable<MetricDto>> GetMetricsByClusterIdAsync(int clusterId);
        Task<MetricDto> CreateMetricAsync(CreateMetricDto createDto);
    }
}