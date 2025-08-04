using AutoMapper;
using K8Intel.Data;
using K8Intel.Dtos;
using K8Intel.Interfaces;
using K8Intel.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace K8Intel.Services
{
    public class ClusterService : IClusterService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ClusterService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ClusterDto> CreateClusterAsync(CreateClusterDto createDto)
        {
            var cluster = _mapper.Map<Cluster>(createDto);
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
    }
}