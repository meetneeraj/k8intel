using System;
using System.ComponentModel.DataAnnotations;

namespace K8Intel.Dtos
{
    public record CreateAlertDto([Required] int ClusterId, [Required] string Severity, [Required] string Message);
    public record AlertDto(int Id, int ClusterId, string Severity, string Message, DateTime Timestamp, bool IsResolved, DateTime? ResolvedAt);
}