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
    public class AlertsController(IAlertService alertService) : ControllerBase
    {
        private readonly IAlertService _alertService = alertService;

        [HttpGet("cluster/{clusterId}")]
    [Authorize(Roles = "Admin,Operator,Viewer")]
    public async Task<ActionResult<IEnumerable<AlertDto>>> GetAlertsForCluster(
        int clusterId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? severity = null,
        [FromQuery] bool? isResolved = null,
        [FromQuery] string? sortBy = null,      // Optional sort field
        [FromQuery] string? sortOrder = null,   // Optional sort direction (asc/desc)
        [FromQuery] DateTime? startDate = null, // Optional start of date range
        [FromQuery] DateTime? endDate = null)   // Optional end of date range
    {
        var pagedResult = await _alertService.GetAlertsByClusterIdAsync(
            clusterId, pageNumber, pageSize, severity, isResolved, 
            sortBy, sortOrder, startDate, endDate);

        // ... Pagination header logic remains the same ...
        var paginationMetadata = new { /* ... */ };
        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
        return Ok(pagedResult.Items);
    }


        [HttpPost]
        // [Authorize(AuthenticationSchemes = ApiKeyAuthenticationOptions.DefaultScheme)]
        [Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme},{ApiKeyAuthenticationOptions.DefaultScheme}")]
        public async Task<ActionResult<AlertDto>> CreateAlert(CreateAlertDto createDto)
        {
            var newAlert = await _alertService.CreateAlertAsync(createDto);
            return CreatedAtAction(nameof(CreateAlert), new { id = newAlert.Id }, newAlert);
        }

        [HttpPatch("{id}/resolve")]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> ResolveAlert(int id)
        {
            var resolvedAlert = await _alertService.ResolveAlertAsync(id);
            if (resolvedAlert == null)
            {
                return NotFound($"Alert with ID {id} not found.");
            }
            return Ok(resolvedAlert);
        }
    }
}