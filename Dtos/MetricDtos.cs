using System;
using System.ComponentModel.DataAnnotations;

namespace K8Intel.Dtos
{
    public record CreateMetricDto([Required] int ClusterId, [Required] string MetricType, double Value);
    public record MetricDto(int Id, int ClusterId, string MetricType, double Value, DateTime Timestamp);
}