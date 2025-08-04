using K8Intel.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace K8Intel.Interfaces
{
    public interface IClusterService
    {
        Task<IEnumerable<ClusterDto>> GetAllClustersAsync();
        Task<ClusterDto?> GetClusterByIdAsync(int id);
        Task<ClusterDto> CreateClusterAsync(CreateClusterDto createDto);
    }
}