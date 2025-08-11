using K8Intel.Dtos;
using K8Intel.Dtos.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace K8Intel.Interfaces
{
    public interface IClusterService
    {
        Task<PagedResult<ClusterDto>> GetAllClustersAsync(
        int pageNumber, int pageSize, string? name,
        string? sortBy, string? sortOrder);

        Task<ClusterDto?> GetClusterByIdAsync(int id);
        Task<ClusterDto> CreateClusterAsync(CreateClusterDto createDto);
    }
}