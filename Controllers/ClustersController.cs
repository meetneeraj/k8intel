using K8Intel.Dtos;
using K8Intel.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace K8Intel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClustersController(IClusterService clusterService) : ControllerBase
    {
        private readonly IClusterService _clusterService = clusterService;

        [HttpGet]
        [Authorize(Roles = "Admin,Operator,Viewer")]
        public async Task<ActionResult<IEnumerable<ClusterDto>>> GetClusters()
        {
            var clusters = await _clusterService.GetAllClustersAsync();
            return Ok(clusters);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Operator,Viewer")]
        public async Task<ActionResult<ClusterDto>> GetCluster(int id)
        {
            var cluster = await _clusterService.GetClusterByIdAsync(id);
            if (cluster == null)
            {
                return NotFound();
            }
            return Ok(cluster);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<ActionResult<ClusterDto>> CreateCluster(CreateClusterDto createDto)
        {
            var newCluster = await _clusterService.CreateClusterAsync(createDto);
            return CreatedAtAction(nameof(GetCluster), new { id = newCluster.Id }, newCluster);
        }
    }
}