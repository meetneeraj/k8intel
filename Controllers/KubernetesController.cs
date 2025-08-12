using K8Intel.Dtos;
using K8Intel.Dtos.Common;
using K8Intel.Interfaces;
using K8Intel.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/k8s")]
[Authorize]
public class KubernetesController : ControllerBase
{
    private readonly IKubernetesService _k8sService;

    public KubernetesController(IKubernetesService k8sService)
    {
        _k8sService = k8sService;
    }
    
    [HttpGet("clusters/{clusterId}/nodes")]
    [Authorize(Roles="Admin,Operator,Viewer")]
    public async Task<ActionResult<List<NodeDto>>> GetNodes(int clusterId) =>
        Ok(await _k8sService.GetNodesByClusterIdAsync(clusterId));

    [HttpGet("nodes/{nodeId}/pods")]
    [Authorize(Roles="Admin,Operator,Viewer")]
    public async Task<ActionResult<List<PodDto>>> GetPods(int nodeId) =>
        Ok(await _k8sService.GetPodsByNodeIdAsync(nodeId));

    [HttpPost("clusters/{clusterId}/inventory")]
    [Authorize(AuthenticationSchemes = ApiKeyAuthenticationOptions.DefaultScheme)]
    public async Task<IActionResult> ReportInventory(int clusterId, InventoryReportDto report)
    {
        await _k8sService.ProcessInventoryReport(clusterId, report);
        return Ok();
    }
    
    [HttpGet("clusters/{clusterId}/metrics/summary")]
    [Authorize(Roles="Admin,Operator,Viewer")]
    public async Task<ActionResult<List<TimeSeriesDataPoint>>> GetMetricSummary(int clusterId, [FromQuery] string metricType, [FromQuery] string timeSpan, [FromQuery] string interval) =>
        Ok(await _k8sService.GetMetricSummaryAsync(clusterId, metricType, timeSpan, interval));
}