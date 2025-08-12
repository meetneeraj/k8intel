namespace K8Intel.Dtos;
public record IncidentDto(int Id, string Type, string Status, DateTime FirstSeenAt, DateTime LastSeenAt, int AlertCount);
public record RecommendationDto(int Id, string Type, string Message, string Severity, string TargetResource, DateTime GeneratedAt);