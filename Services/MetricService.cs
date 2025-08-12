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
    public class MetricService : IMetricService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly AutoMapper.IConfigurationProvider _configurationProvider;

        public MetricService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _configurationProvider = mapper.ConfigurationProvider;
        }

        public async Task<MetricDto> CreateMetricAsync(CreateMetricDto createDto)
        {
            var cluster = await _context.Clusters.FindAsync(createDto.ClusterId);
            if (cluster != null)
            {
                cluster.LastAgentContactAt = DateTime.UtcNow;
            }

            var metric = _mapper.Map<ClusterMetric>(createDto);
            metric.Timestamp = DateTime.UtcNow;

            _context.ClusterMetrics.Add(metric);
            await _context.SaveChangesAsync();
            return _mapper.Map<MetricDto>(metric);
        }
        public async Task<PagedResult<MetricDto>> GetMetricsByClusterIdAsync(
        int clusterId, int pageNumber, int pageSize, string? metricType,
        DateTime? startDate, DateTime? endDate) // --->>> ADD date range
        {
            var query = _context.ClusterMetrics
                .Where(m => m.ClusterId == clusterId)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(metricType)) {
                query = query.Where(m => m.MetricType.ToUpper() == metricType.ToUpper());
            }

            // --->>> ADD date range filter logic <<<---
            if (startDate.HasValue) {
                query = query.Where(m => m.Timestamp >= startDate.Value);
            }
            if (endDate.HasValue) {
                query = query.Where(m => m.Timestamp < endDate.Value.AddDays(1));
            }

            var finalQuery = query
                .OrderByDescending(m => m.Timestamp)
                .ProjectTo<MetricDto>(_configurationProvider);

            return await finalQuery.ToPagedResultAsync(pageNumber, pageSize);
        }
        
    }
}