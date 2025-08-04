using K8Intel.Dtos;
using K8Intel.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace K8Intel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MetricsController(IMetricService metricService) : ControllerBase
    {
        private readonly IMetricService _metricService = metricService;

        [HttpGet("cluster/{clusterId}")]
        [Authorize(Roles = "Admin,Operator,Viewer")]
        public async Task<ActionResult<IEnumerable<MetricDto>>> GetMetricsForCluster(int clusterId)
        {
            var metrics = await _metricService.GetMetricsByClusterIdAsync(clusterId);
            return Ok(metrics);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Operator")] // Or an automated agent role
        public async Task<ActionResult<MetricDto>> CreateMetric(CreateMetricDto createDto)
        {
            var newMetric = await _metricService.CreateMetricAsync(createDto);
            return CreatedAtAction(nameof(CreateMetric), new { id = newMetric.Id }, newMetric);
        }
    }
}