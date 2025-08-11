using Bogus;
using K8Intel.Data;
using K8Intel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace K8Intel.Data.Seeding
{
    public static class DataSeeder
    {
        private static string GenerateSafeApiKey()
        {
            var keyBytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(keyBytes)
                          .TrimEnd('=')
                          .Replace('+', '-')
                          .Replace('/', '_');
        }

        public static (int ClustersAdded, int MetricsAdded, int AlertsAdded) Seed(AppDbContext context, bool force = false)
        {
            if (!force && context.Clusters.Count() > 50) return (0, 0, 0);

            if (force)
            {
                context.ClusterMetrics.RemoveRange(context.ClusterMetrics);
                context.Alerts.RemoveRange(context.Alerts);
                context.Clusters.RemoveRange(context.Clusters);
                context.SaveChanges();
            }

            var newClusters = new List<Cluster>();
            var clusterFaker = new Faker<Cluster>()
                .RuleFor(c => c.Name, f => $"{f.Hacker.Adjective()}-{f.Commerce.Department().ToLower()}-{f.Address.CountryCode().ToLower()}")
                .RuleFor(c => c.ApiEndpoint, (f, c) => $"https://{f.Internet.Ip()}:6443")
                .RuleFor(c => c.AgentApiKey, f => GenerateSafeApiKey());

            newClusters.AddRange(clusterFaker.Generate(100));
            context.Clusters.AddRange(newClusters);
            context.SaveChanges();

            var clusterIds = newClusters.Select(c => c.Id).ToList();
            var totalMetrics = 0;
            var totalAlerts = 0;
            var severities = new[] { "Critical", "Warning", "Info" };

            var newAlerts = new Faker<Alert>()
                .CustomInstantiator(f => {
                    var alert = new Alert();
                    alert.IsResolved = f.Random.Bool(0.7f);
                    alert.Timestamp = f.Date.Past(1).ToUniversalTime(); // Generate the creation time

                    if (alert.IsResolved)
                    {
                        // If resolved, set ResolvedAt to a date AFTER the alert was created
                        alert.ResolvedAt = f.Date.Soon(10, alert.Timestamp).ToUniversalTime();
                    } else {
                        alert.ResolvedAt = null;
                    }
                    
                    alert.Severity = f.PickRandom(new[] { "Critical", "Warning", "Info" });
                    alert.Message = f.Hacker.Phrase();
                    
                    return alert;
                });

            var metricFaker = new Faker<ClusterMetric>()
                .RuleFor(m => m.MetricType, f => f.PickRandom(new[] { "CPU", "Memory" }))
                .RuleFor(m => m.Value, f => f.Random.Double(1, 100))
                .RuleFor(m => m.Timestamp, f => f.Date.Past(1).ToUniversalTime());

            foreach (var clusterId in clusterIds)
            {
                var alertsForCluster = newAlerts.Generate(500);
                alertsForCluster.ForEach(a => a.ClusterId = clusterId);
                context.Alerts.AddRange(alertsForCluster);
                totalAlerts += alertsForCluster.Count;

                var metricsForCluster = metricFaker.Generate(2000);
                metricsForCluster.ForEach(m => m.ClusterId = clusterId);
                context.ClusterMetrics.AddRange(metricsForCluster);
                totalMetrics += metricsForCluster.Count;
            }


            context.SaveChanges();

            return (newClusters.Count, totalMetrics, totalAlerts);
        }
    }
}