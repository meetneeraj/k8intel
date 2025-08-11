using K8Intel.Dtos;
using K8Intel.Dtos.Common; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace K8Intel.Interfaces
{
    public interface IAlertService
    {
       Task<PagedResult<AlertDto>> GetAlertsByClusterIdAsync(
            int clusterId, int pageNumber, int pageSize, 
            string? severity, bool? isResolved, 
            string? sortBy, string? sortOrder,      
            DateTime? startDate, DateTime? endDate);

        Task<AlertDto> CreateAlertAsync(CreateAlertDto createDto);
        Task<AlertDto?> ResolveAlertAsync(int alertId);
    }
}