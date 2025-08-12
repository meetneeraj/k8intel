using AutoMapper;
using AutoMapper.QueryableExtensions;
using K8Intel.Data;
using K8Intel.Dtos;
using K8Intel.Dtos.Common;
using K8Intel.Interfaces;
using K8Intel.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace K8Intel.Services
{
    public class KubernetesService : IKubernetesService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly AutoMapper.IConfigurationProvider _configurationProvider;

        public KubernetesService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _configurationProvider = mapper.ConfigurationProvider;
        }

        public async Task<List<NodeDto>> GetNodesByClusterIdAsync(int clusterId) =>
            await _context.Nodes
                .Where(n => n.ClusterId == clusterId)
                .ProjectTo<NodeDto>(_configurationProvider)
                .ToListAsync();

        public async Task<List<PodDto>> GetPodsByNodeIdAsync(int nodeId) =>
            await _context.Pods
                .Where(p => p.NodeId == nodeId)
                .ProjectTo<PodDto>(_configurationProvider)
                .ToListAsync();

        public async Task<List<TimeSeriesDataPoint>> GetMetricSummaryAsync(int clusterId, string metricType, string timeSpan, string interval)
        {
            var startDate = DateTime.UtcNow.AddDays(-1);

            var results = await _context.ClusterMetrics
                .Where(m => m.ClusterId == clusterId && m.MetricType.ToUpper() == metricType.ToUpper() && m.Timestamp >= startDate)
                .GroupBy(m => new { m.Timestamp.Date, m.Timestamp.Hour })
                .Select(g => new TimeSeriesDataPoint(
                    new DateTime(g.Key.Date.Year, g.Key.Date.Month, g.Key.Date.Day, g.Key.Hour, 0, 0, DateTimeKind.Utc),
                    g.Average(m => m.Value),
                    g.Min(m => m.Value),
                    g.Max(m => m.Value)
                ))
                .OrderBy(dp => dp.Timestamp)
                .ToListAsync();
            
            return results;
        }

        public async Task ProcessInventoryReport(int clusterId, InventoryReportDto report)
        {
            // === Step 1: Upsert Nodes ===
            var existingNodes = await _context.Nodes
                .Where(n => n.ClusterId == clusterId)
                .ToListAsync();

            var incomingNodeNames = report.Nodes.Select(n => n.Name).ToHashSet();
            var nodesToRemove = existingNodes.Where(n => !incomingNodeNames.Contains(n.Name)).ToList();

            if (nodesToRemove.Any())
            {
                _context.Nodes.RemoveRange(nodesToRemove);
            }

            foreach (var nodeDto in report.Nodes)
            {
                var existingNode = existingNodes.FirstOrDefault(n => n.Name == nodeDto.Name);
                if (existingNode == null)
                {
                    var newNode = _mapper.Map<Node>(nodeDto);
                    newNode.ClusterId = clusterId;
                    _context.Nodes.Add(newNode);
                }
                else
                {
                    _mapper.Map(nodeDto, existingNode);
                }
            }
            await _context.SaveChangesAsync();

            // === Step 2: Upsert Pods ===
            var allClusterNodes = await _context.Nodes
                .Where(n => n.ClusterId == clusterId)
                .Include(n => n.Pods)
                .ToDictionaryAsync(n => n.Name, n => n); // Use ToDictionaryAsync

            var incomingPodKeys = report.Pods.Select(p => $"{p.Namespace}/{p.Name}").ToHashSet();

            // --->>> FIX IS HERE: Removed 'await' from this synchronous, in-memory operation <<<---
            var allExistingPods = allClusterNodes.Values.SelectMany(n => n.Pods).ToList();
            
            var podsToRemove = allExistingPods
                .Where(p => !incomingPodKeys.Contains($"{p.Namespace}/{p.Name}"))
                .ToList();
            
            if (podsToRemove.Any())
            {
                _context.Pods.RemoveRange(podsToRemove);
            }

            foreach (var podDto in report.Pods)
            {
                var existingPod = allExistingPods
                    .FirstOrDefault(p => p.Name == podDto.Name && p.Namespace == podDto.Namespace);
                
                if (string.IsNullOrEmpty(podDto.NodeName) || !allClusterNodes.TryGetValue(podDto.NodeName, out var parentNode))
                {
                    continue; 
                }
                
                if (existingPod == null)
                {
                    var newPod = _mapper.Map<Pod>(podDto);
                    newPod.NodeId = parentNode.Id;
                    _context.Pods.Add(newPod);
                }
                else
                {
                    _mapper.Map(podDto, existingPod);
                    existingPod.NodeId = parentNode.Id;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}