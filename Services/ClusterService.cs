using AutoMapper;
using AutoMapper.QueryableExtensions;
using K8Intel.Data;
using K8Intel.Dtos;
using K8Intel.Interfaces;
using K8Intel.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using K8Intel.Dtos.Common;
using K8Intel.Extensions;

namespace K8Intel.Services
{
    public class ClusterService : IClusterService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly AutoMapper.IConfigurationProvider _configurationProvider;

        public ClusterService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _configurationProvider = mapper.ConfigurationProvider; // Get the provider
        }

        public async Task<ClusterDto> CreateClusterAsync(CreateClusterDto createDto)
        {
            var cluster = _mapper.Map<Cluster>(createDto);

            var keyBytes = RandomNumberGenerator.GetBytes(32);
            cluster.AgentApiKey = Convert.ToBase64String(keyBytes)
                                     .TrimEnd('=')
                                     .Replace('+', '-')
                                     .Replace('/', '_');

            _context.Clusters.Add(cluster);
            await _context.SaveChangesAsync();
            return _mapper.Map<ClusterDto>(cluster);
        }

        public async Task<IEnumerable<ClusterDto>> GetAllClustersAsync()
        {
            var clusters = await _context.Clusters.ToListAsync();
            return _mapper.Map<IEnumerable<ClusterDto>>(clusters);
        }

        public async Task<ClusterDto?> GetClusterByIdAsync(int id)
        {
            var cluster = await _context.Clusters.FindAsync(id);
            return cluster == null ? null : _mapper.Map<ClusterDto>(cluster);
        }
        public async Task<PagedResult<ClusterDto>> GetAllClustersAsync(
        int pageNumber, int pageSize, string? name,
        string? sortBy, string? sortOrder) 
        {
            var query = _context.Clusters.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(name))
            {
                // Use EF.Functions.ILike to generate a case-insensitive LIKE query.
                // The '%' wildcards are crucial for the "CONTAINS" behavior.
                // This query form is optimized to use the pg_trgm index.
                query = query.Where(c => EF.Functions.ILike(c.Name, $"%{name}%"));
            }

            // --->>> ADD sorting logic <<<---
            var orderedQuery = sortBy?.ToLower() switch
            {
                "id" => (sortOrder?.ToLower() == "asc")
                    ? query.OrderBy(c => c.Id)
                    : query.OrderByDescending(c => c.Id),

                // Default to sorting by name
                _ => (sortOrder?.ToLower() == "desc")
                    ? query.OrderByDescending(c => c.Name)
                    : query.OrderBy(c => c.Name)
            };

            var finalQuery = orderedQuery.ProjectTo<ClusterDto>(_configurationProvider);
            return await finalQuery.ToPagedResultAsync(pageNumber, pageSize);
        }
    }
}