using AutoMapper;
using AutoMapper.QueryableExtensions;
using K8Intel.Data;
using K8Intel.Dtos;
using K8Intel.Dtos.Common;
using K8Intel.Extensions;
using K8Intel.Interfaces;
using K8Intel.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Threading.Tasks;

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
            _configurationProvider = mapper.ConfigurationProvider;
        }

        public async Task<ClusterDto> CreateClusterAsync(CreateClusterDto createDto)
        {
            var cluster = _mapper.Map<Cluster>(createDto);

            var keyBytes = RandomNumberGenerator.GetBytes(32);
            cluster.AgentApiKey = System.Convert.ToBase64String(keyBytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

            _context.Clusters.Add(cluster);
            await _context.SaveChangesAsync();
            return _mapper.Map<ClusterDto>(cluster); // This will now work
        }

        public async Task<PagedResult<ClusterDto>> GetAllClustersAsync(int pageNumber, int pageSize, string? name, string? sortBy, string? sortOrder)
        {
            var query = _context.Clusters.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(c => EF.Functions.ILike(c.Name, $"%{name}%"));
            }
            var orderedQuery = sortBy?.ToLower() switch {
                "id" => (sortOrder?.ToLower() == "asc") ? query.OrderBy(c => c.Id) : query.OrderByDescending(c => c.Id),
                _ => (sortOrder?.ToLower() == "desc") ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name)
            };
            return await orderedQuery.ProjectTo<ClusterDto>(_configurationProvider).ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<ClusterDto?> GetClusterByIdAsync(int id)
        {
            return await _context.Clusters
                .AsNoTracking()
                .Where(c => c.Id == id)
                .ProjectTo<ClusterDto>(_configurationProvider)
                .FirstOrDefaultAsync();
        }
    }
}