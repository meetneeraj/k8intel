using K8Intel.Data;
using K8Intel.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace K8Intel.Jobs
{
    public class InsightsGeneratorJob : IInsightsGeneratorJob
    {
        private readonly AppDbContext _context;

        public InsightsGeneratorJob(AppDbContext context)
        {
            _context = context;
        }

        public async Task GenerateStabilityRecommendationsAsync()
        {
            // --- AI Rule: Detect frequently recurring alerts ("Alert Flapping") ---
            var timeWindow = DateTime.UtcNow.AddHours(-1); // Analyze alerts from the last hour
            var frequencyThreshold = 5; // An alert is "flapping" if it occurs 5+ times

            // Find alert groups that are flapping
            var flappingAlerts = await _context.Alerts
                .Where(a => a.Timestamp >= timeWindow && !a.IsResolved)
                .GroupBy(a => new { a.ClusterId, a.Message }) // Group by what and where
                .Where(g => g.Count() >= frequencyThreshold)
                .Select(g => new { g.Key.ClusterId, g.Key.Message, Count = g.Count() })
                .ToListAsync();

            if (!flappingAlerts.Any()) return;
            
            // Get existing recommendations to avoid duplicates
            var existingRecs = await _context.Recommendations
                .Where(r => r.Type == RecommendationType.Stability)
                .ToListAsync();

            foreach (var flappingAlert in flappingAlerts)
            {
                // Simple fingerprint to check for duplicates
                var fingerprint = $"FlappingAlert-{flappingAlert.ClusterId}-{flappingAlert.Message}";

                if (!existingRecs.Any(r => r.TargetResource == fingerprint))
                {
                    var newRecommendation = new Recommendation
                    {
                        ClusterId = flappingAlert.ClusterId,
                        Type = RecommendationType.Stability,
                        Severity = "High",
                        TargetResource = fingerprint, // Use the fingerprint to track this specific recommendation
                        Message = $"The alert '{flappingAlert.Message}' is flapping. It has fired {flappingAlert.Count} times in the last hour. Consider investigating the root cause or adjusting alert thresholds.",
                        GeneratedAt = DateTime.UtcNow
                    };
                    _context.Recommendations.Add(newRecommendation);
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}