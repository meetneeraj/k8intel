using AutoMapper;
using AutoMapper.QueryableExtensions;
using K8Intel.Data;
using K8Intel.Dtos;
using K8Intel.Dtos.Common;
using K8Intel.Interfaces;
using K8Intel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace K8Intel.Services
{
    public class KubernetesService : IKubernetesService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly AutoMapper.IConfigurationProvider _configurationProvider;
        private readonly ILogger<KubernetesService> _logger;

        public KubernetesService(AppDbContext context, IMapper mapper, ILogger<KubernetesService> logger)
        {
            _context = context;
            _mapper = mapper;
            _configurationProvider = mapper.ConfigurationProvider;
            _logger = logger;
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

        // public async Task ProcessInventoryReport(int clusterId, InventoryReportDto report)
        // {
        //     _logger.LogInformation("Processing inventory report for ClusterId: {ClusterId}", clusterId);

        //     // Step 1: Create a dictionary of existing nodes for fast lookup
        //     var existingNodes = await _context.Nodes
        //         .Include(n => n.Pods) // Eagerly load pods
        //         .Where(n => n.ClusterId == clusterId)
        //         .ToDictionaryAsync(n => n.Name, n => n);

        //     // Step 2: Handle incoming nodes
        //     foreach (var nodeDto in report.Nodes)
        //     {
        //         if (existingNodes.TryGetValue(nodeDto.Name, out var existingNode))
        //         {
        //             // Node exists, update it
        //             _mapper.Map(nodeDto, existingNode);
        //         }
        //         else
        //         {
        //             // Node is new, add it
        //             var newNode = _mapper.Map<Node>(nodeDto);
        //             newNode.ClusterId = clusterId;
        //             _context.Nodes.Add(newNode);
        //             existingNodes[newNode.Name] = newNode; // Add to our dictionary for pod processing
        //         }
        //     }

        //     await _context.SaveChangesAsync(); // Save node changes to get DB IDs for new nodes

        //     // Step 3: Handle incoming pods
        //     var allExistingPods = existingNodes.Values.SelectMany(n => n.Pods).ToDictionary(p => $"{p.Namespace}/{p.Name}", p => p);

        //     foreach (var podDto in report.Pods)
        //     {
        //         if (string.IsNullOrEmpty(podDto.NodeName) || !existingNodes.TryGetValue(podDto.NodeName, out var parentNode))
        //         {
        //             _logger.LogWarning("Pod {PodName} is reporting a NodeName {NodeName} that does not exist in the inventory. Skipping.", podDto.Name, podDto.NodeName);
        //             continue; // Skip pods with an unknown parent node
        //         }

        //         var podKey = $"{podDto.Namespace}/{podDto.Name}";

        //         if (allExistingPods.TryGetValue(podKey, out var existingPod))
        //         {
        //             // Pod exists, update it
        //             _mapper.Map(podDto, existingPod);
        //             existingPod.NodeId = parentNode.Id; // Ensure it's linked to the correct parent node
        //         }
        //         else
        //         {
        //             // Pod is new, add it
        //             var newPod = _mapper.Map<Pod>(podDto);
        //             newPod.NodeId = parentNode.Id; // Set the correct Foreign Key
        //             _context.Pods.Add(newPod);
        //         }
        //     }

        //     // Step 4: (Optional but recommended) Handle deletions
        //     var incomingPodKeys = report.Pods.Select(p => $"{p.Namespace}/{p.Name}").ToHashSet();
        //     var podsToRemove = allExistingPods.Values.Where(p => !incomingPodKeys.Contains($"{p.Namespace}/{p.Name}")).ToList();
        //     if (podsToRemove.Any())
        //     {
        //         _logger.LogInformation("Removing {Count} stale pods for ClusterId: {ClusterId}", podsToRemove.Count, clusterId);
        //         _context.Pods.RemoveRange(podsToRemove);
        //     }

        //     await _context.SaveChangesAsync();
        //     _logger.LogInformation("Successfully processed inventory report for ClusterId: {ClusterId}", clusterId);
        // }
        public async Task ProcessInventoryReport(int clusterId, InventoryReportDto report)
        {
            try
            {
                _logger.LogInformation("STEP 1: Starting inventory report for ClusterId: {ClusterId}", clusterId);

                var existingNodes = await _context.Nodes
                    .Include(n => n.Pods)
                    .Where(n => n.ClusterId == clusterId)
                    .ToDictionaryAsync(n => n.Name, n => n);
                _logger.LogInformation("STEP 2: Fetched {Count} existing nodes from DB.", existingNodes.Count);
                
                // === Process Nodes ===
                foreach (var nodeDto in report.Nodes)
                {
                    if (existingNodes.TryGetValue(nodeDto.Name, out var existingNode))
                    {
                        _mapper.Map(nodeDto, existingNode);
                    }
                    else
                    {
                        var newNode = _mapper.Map<Node>(nodeDto);
                        newNode.ClusterId = clusterId;
                        _context.Nodes.Add(newNode);
                        existingNodes[newNode.Name] = newNode;
                    }
                }
                _logger.LogInformation("STEP 3: Processed {Count} incoming nodes. Saving changes.", report.Nodes.Count);
                await _context.SaveChangesAsync();
                _logger.LogInformation("STEP 4: Successfully saved node changes.");

                // === Process Pods ===
                var allExistingPods = existingNodes.Values.SelectMany(n => n.Pods).ToDictionary(p => $"{p.Namespace}/{p.Name}", p => p);
                _logger.LogInformation("STEP 5: Created dictionary of {Count} existing pods for comparison.", allExistingPods.Count);

                foreach (var podDto in report.Pods)
                {
                    if (string.IsNullOrEmpty(podDto.NodeName) || !existingNodes.TryGetValue(podDto.NodeName, out var parentNode))
                    {
                        _logger.LogWarning("Skipping pod '{PodNamespace}/{PodName}' because its parent node '{NodeName}' was not found.", podDto.Namespace, podDto.Name, podDto.NodeName);
                        continue;
                    }

                    var podKey = $"{podDto.Namespace}/{podDto.Name}";
                    if (allExistingPods.TryGetValue(podKey, out var existingPod))
                    {
                        _mapper.Map(podDto, existingPod);
                        existingPod.NodeId = parentNode.Id;
                    }
                    else
                    {
                        var newPod = _mapper.Map<Pod>(podDto);
                        newPod.NodeId = parentNode.Id;
                        _context.Pods.Add(newPod);
                    }
                }
                 _logger.LogInformation("STEP 6: Processed {Count} incoming pods. Saving changes.", report.Pods.Count);
                await _context.SaveChangesAsync();
                 _logger.LogInformation("STEP 7: Successfully saved pod changes.");

                // === Handle Deletions (Final Step) ===
                 _logger.LogInformation("STEP 8: Handling deletions.");
                var incomingPodKeys = report.Pods.Select(p => $"{p.Namespace}/{p.Name}").ToHashSet();
                var podsToRemove = allExistingPods.Values.Where(p => !incomingPodKeys.Contains($"{p.Namespace}/{p.Name}")).ToList();
                if (podsToRemove.Any())
                {
                    _context.Pods.RemoveRange(podsToRemove);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("STEP 9: Inventory report processed successfully for ClusterId: {ClusterId}", clusterId);
            }
            catch (Exception ex)
            {
                // This is the most important part. We will log the *real* exception.
                _logger.LogError(ex, "FATAL ERROR occurred during ProcessInventoryReport for ClusterId: {ClusterId}", clusterId);
                throw; // Re-throw the exception so the global handler still catches it and returns a 500
            }
        }
    }
}