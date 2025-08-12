namespace K8Intel.Dtos.Common;
public record TimeSeriesDataPoint(DateTime Timestamp, double AverageValue, double MinValue, double MaxValue);