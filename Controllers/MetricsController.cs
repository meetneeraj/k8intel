using K8Intel.Dtos;
using K8Intel.Interfaces;
using K8Intel.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
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
        public async Task<ActionResult<IEnumerable<MetricDto>>> GetMetricsForCluster(
            int clusterId, 
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? metricType = null,
            [FromQuery] DateTime? startDate = null, 
            [FromQuery] DateTime? endDate = null)
        {
            var pagedResult = await _metricService.GetMetricsByClusterIdAsync(
                clusterId, pageNumber, pageSize, metricType, startDate, endDate);

            var paginationMetadata = new
            {
                pagedResult.TotalCount,
                pagedResult.PageSize,
                pagedResult.PageNumber,
                pagedResult.TotalPages,
                pagedResult.HasNextPage,
                pagedResult.HasPreviousPage
            };

            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
            
            return Ok(pagedResult.Items);
        }

        [HttpPost]
        //[Authorize(AuthenticationSchemes = ApiKeyAuthenticationOptions.DefaultScheme)]
        [Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme},{ApiKeyAuthenticationOptions.DefaultScheme}")]
        public async Task<ActionResult<MetricDto>> CreateMetric(CreateMetricDto createDto)
        {
            var newMetric = await _metricService.CreateMetricAsync(createDto);
            return CreatedAtAction(nameof(CreateMetric), new { id = newMetric.Id }, newMetric);
        }
    }
}