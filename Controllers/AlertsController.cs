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
    public class AlertsController(IAlertService alertService) : ControllerBase
    {
        private readonly IAlertService _alertService = alertService;

        [HttpGet("cluster/{clusterId}")]
        [Authorize(Roles = "Admin,Operator,Viewer")]
        public async Task<ActionResult<IEnumerable<AlertDto>>> GetAlertsForCluster(int clusterId)
        {
            var alerts = await _alertService.GetAlertsByClusterIdAsync(clusterId);
            return Ok(alerts);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Operator")] // Or perhaps an automated agent role
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