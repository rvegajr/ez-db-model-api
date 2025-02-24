using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models;

// Model with no key (used for audit logging)
[NotMapped]
public class SampleAuditLog
{
    public DateTime Timestamp { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Changes { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}
