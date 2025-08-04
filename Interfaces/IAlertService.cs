using K8Intel.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace K8Intel.Interfaces
{
    public interface IAlertService
    {
        Task<IEnumerable<AlertDto>> GetAlertsByClusterIdAsync(int clusterId);
        Task<AlertDto> CreateAlertAsync(CreateAlertDto createDto);
        Task<AlertDto?> ResolveAlertAsync(int alertId);
    }
}