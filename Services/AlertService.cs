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
    public class AlertService : IAlertService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public AlertService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<AlertDto> CreateAlertAsync(CreateAlertDto createDto)
        {
            var alert = _mapper.Map<Alert>(createDto);
            alert.Timestamp = DateTime.UtcNow;

            _context.Alerts.Add(alert);
            await _context.SaveChangesAsync();
            return _mapper.Map<AlertDto>(alert);
        }

        public async Task<IEnumerable<AlertDto>> GetAlertsByClusterIdAsync(int clusterId)
        {
            var alerts = await _context.Alerts
                .Where(a => a.ClusterId == clusterId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
            return _mapper.Map<IEnumerable<AlertDto>>(alerts);
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