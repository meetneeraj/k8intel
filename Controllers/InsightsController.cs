using AutoMapper;
using AutoMapper.QueryableExtensions;
using K8Intel.Data;
using K8Intel.Dtos;
using K8Intel.Interfaces; // Assuming a new service interface for insights
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace K8Intel.Controllers;
[ApiController]
[Route("api/insights")]
[Authorize(Roles = "Admin,Operator,Viewer")]
public class InsightsController : ControllerBase
{
    // For simplicity, directly using AppDbContext here. Can be moved to a service.
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public InsightsController(AppDbContext context, IMapper mapper) {
        _context = context; _mapper = mapper;
    }

    [HttpGet("recommendations")]
    public async Task<ActionResult<List<RecommendationDto>>> GetRecommendations() {
        var recs = await _context.Recommendations
            .OrderByDescending(r => r.GeneratedAt)
            .ProjectTo<RecommendationDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
        return Ok(recs);
    }
}