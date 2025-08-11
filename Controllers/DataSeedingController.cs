using K8Intel.Data;
using K8Intel.Data.Seeding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace K8Intel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)] // Hide from Swagger UI
    public class DataSeedingController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DataSeedingController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("seed-test-data")]
        [Authorize(Roles = "Admin")] // Only Admins can execute
        public IActionResult SeedData([FromQuery] bool force = false) // Accept 'force' as an optional query param
        {
            try
            {
                var result = DataSeeder.Seed(_context, force);

                // Check if the seeder did nothing because the DB was already full
                if (!force && result.ClustersAdded == 0 && result.MetricsAdded == 0)
                {
                    return Ok(new { Message = "Database already contains significant data. No new data added. To clear and re-seed, use the '?force=true' query parameter." });
                }

                // Return a detailed success message
                return Ok(new
                {
                    Message = force ? "Forced re-seeding completed successfully." : "Data seeding completed successfully.",
                    ClustersAdded = result.ClustersAdded,
                    MetricsAdded = result.MetricsAdded,
                    AlertsAdded = result.AlertsAdded
                });
            }
            catch (Exception ex)
            {
                // Return a more detailed error if something goes wrong during the process
                return StatusCode(500, new 
                { 
                    Message = "An error occurred during data seeding.", 
                    Error = ex.Message,
                    InnerException = ex.InnerException?.Message
                });
            }
        }
    }
}