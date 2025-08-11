using AutoMapper;
using K8Intel.Data;
using K8Intel.Dtos;
using K8Intel.Interfaces;
using K8Intel.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using K8Intel.Dtos.Common;
using K8Intel.Extensions;

namespace K8Intel.Services
{
    public class AlertService : IAlertService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly AutoMapper.IConfigurationProvider _configurationProvider;

        public AlertService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _configurationProvider = mapper.ConfigurationProvider;
        }
        public async Task<PagedResult<AlertDto>> GetAlertsByClusterIdAsync(
            int clusterId, int pageNumber, int pageSize, 
            string? severity, bool? isResolved, 
            string? sortBy, string? sortOrder, 
            DateTime? startDate, DateTime? endDate)
        {
            // 1. Start with the base query
            var query = _context.Alerts
                .Where(a => a.ClusterId == clusterId)
                .AsNoTracking();

            // 2. Apply existing filters
            if (!string.IsNullOrWhiteSpace(severity))
            {
                query = query.Where(a => a.Severity.ToUpper() == severity.ToUpper());
            }

            if (isResolved.HasValue)
            {
                query = query.Where(a => a.IsResolved == isResolved.Value);
            }

            // 3. Apply NEW date range filters
            if (startDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                // Use "less than" the next day to include the entire end day
                query = query.Where(a => a.Timestamp < endDate.Value.AddDays(1));
            }
            
            // 4. Apply NEW dynamic sorting logic
            // Use a switch statement to safely map client-side names to server-side properties.
            var orderedQuery = sortBy?.ToLower() switch
            {
                "severity" => (sortOrder?.ToLower() == "asc")
                    ? query.OrderBy(a => a.Severity)
                    : query.OrderByDescending(a => a.Severity),
                
                "resolvedat" => (sortOrder?.ToLower() == "asc")
                    ? query.OrderBy(a => a.ResolvedAt)
                    : query.OrderByDescending(a => a.ResolvedAt),

                // Default case: sort by timestamp (most recent first)
                _ => query.OrderByDescending(a => a.Timestamp)
            };
            
            // 5. Project to DTO and Paginate
            var finalQuery = orderedQuery.ProjectTo<AlertDto>(_configurationProvider);
            return await finalQuery.ToPagedResultAsync(pageNumber, pageSize);
        }
        public async Task<AlertDto> CreateAlertAsync(CreateAlertDto createDto)
        {
            var alert = _mapper.Map<Alert>(createDto);
            alert.Timestamp = DateTime.UtcNow;

            _context.Alerts.Add(alert);
            await _context.SaveChangesAsync();
            return _mapper.Map<AlertDto>(alert);
        }

        public async Task<AlertDto?> ResolveAlertAsync(int alertId)
        {
            var alert = await _context.Alerts.FindAsync(alertId);
            if (alert == null)
            {
                return null;
            }

            alert.IsResolved = true;
            alert.ResolvedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return _mapper.Map<AlertDto>(alert);
        }
    }
}