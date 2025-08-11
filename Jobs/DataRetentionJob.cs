using K8Intel.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace K8Intel.Jobs
{
    // This interface allows us to register the class with Dependency Injection if needed, which is good practice.
    public interface IDataRetentionJob
    {
        Task PurgeOldDataAsync();
    }

    public class DataRetentionJob : IDataRetentionJob
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DataRetentionJob> _logger;

        public DataRetentionJob(AppDbContext context, ILogger<DataRetentionJob> logger)
        {
            _context = context;
            _logger = logger;
        }

        // This is the public method Hangfire will execute.
        public async Task PurgeOldDataAsync()
        {
            _logger.LogInformation("Starting daily data retention job at: {time}", DateTime.UtcNow);

            // 1. Define the retention period (e.g., 30 days ago).
            var retentionDate = DateTime.UtcNow.AddDays(-30);

            // 2. Purge old metrics.
            try
            {
                // Use ExecuteDeleteAsync for maximum performance. It generates a single DELETE SQL statement.
                var metricsDeleted = await _context.ClusterMetrics
                    .Where(m => m.Timestamp < retentionDate)
                    .ExecuteDeleteAsync();

                if (metricsDeleted > 0)
                {
                    _logger.LogInformation("Successfully purged {count} old metric records.", metricsDeleted);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while purging old metrics.");
            }

            // 3. Purge old RESOLVED alerts.
            // We only delete alerts that are both old AND marked as resolved.
            try
            {
                var alertsDeleted = await _context.Alerts
                    .Where(a => a.IsResolved && a.ResolvedAt < retentionDate)
                    .ExecuteDeleteAsync();

                if (alertsDeleted > 0)
                {
                    _logger.LogInformation("Successfully purged {count} old resolved alerts.", alertsDeleted);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while purging old resolved alerts.");
            }

            _logger.LogInformation("Data retention job finished at: {time}", DateTime.UtcNow);
        }
    }
}