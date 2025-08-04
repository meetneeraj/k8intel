using AutoMapper;
using K8Intel.Data;
using K8Intel.Dtos;
using K8Intel.Interfaces;
using K8Intel.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace K8Intel.Services
{
    public class MetricService : IMetricService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public MetricService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<MetricDto> CreateMetricAsync(CreateMetricDto createDto)
        {
            var metric = _mapper.Map<ClusterMetric>(createDto);
            metric.Timestamp = DateTime.UtcNow;

            _context.ClusterMetrics.Add(metric);
            await _context.SaveChangesAsync();
            return _mapper.Map<MetricDto>(metric);
        }

        public async Task<IEnumerable<MetricDto>> GetMetricsByClusterIdAsync(int clusterId)
        {
            var metrics = await _context.ClusterMetrics
                .Where(m => m.ClusterId == clusterId)
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();
            return _mapper.Map<IEnumerable<MetricDto>>(metrics);
        }
    }
}