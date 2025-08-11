using System.Text.Json;
using K8Intel.Dtos;
using K8Intel.Dtos.Common;
using K8Intel.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace K8Intel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClustersController(IClusterService clusterService, ILogger<ClustersController> logger) : ControllerBase
    {
        private readonly IClusterService _clusterService = clusterService;
        private readonly ILogger<ClustersController> _logger = logger;

        [HttpGet]
        [Authorize(Roles = "Admin,Operator,Viewer")]
        public async Task<ActionResult<IEnumerable<ClusterDto>>> GetClusters(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? name = null,
            [FromQuery] string? sortBy = null,   
            [FromQuery] string? sortOrder = null)
        {
            //throw new InvalidOperationException("This is a test exception!");
            var pagedResult = await _clusterService.GetAllClustersAsync(pageNumber, pageSize, name, sortBy, sortOrder);

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

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Operator,Viewer")]
        public async Task<ActionResult<ClusterDto>> GetCluster(int id)
        {
            _logger.LogInformation("GetCluster endpoint called with id: {Id}", id);
            var cluster = await _clusterService.GetClusterByIdAsync(id);
            if (cluster == null)
            {
                _logger.LogWarning("Cluster with id {Id} not found", id);
                return NotFound();
            }
            _logger.LogInformation("Returning cluster with id: {Id}", id);
            return Ok(cluster);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<ActionResult<ClusterDto>> CreateCluster(CreateClusterDto createDto)
        {
            _logger.LogInformation("CreateCluster endpoint called with data: {@CreateDto}", createDto);
            var newCluster = await _clusterService.CreateClusterAsync(createDto);
            _logger.LogInformation("Created new cluster with id: {Id}", newCluster.Id);
            return CreatedAtAction(nameof(GetCluster), new { id = newCluster.Id }, newCluster);
        }
    }
}