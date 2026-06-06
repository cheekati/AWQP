using AWQP.Domain.Common;

namespace AWQP.Domain.Documents;

public sealed class ManagedDocument : BaseEntity
{
    public string DocumentNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public string Revision { get; set; } = "A";
    public string FileName { get; set; } = string.Empty;
    public string StorageUri { get; set; } = string.Empty;
    public string OwnerDepartment { get; set; } = string.Empty;
    public DateTimeOffset EffectiveAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ExpiresAt { get; set; }
    public bool IsReleased { get; set; }
}

public sealed class AuditLog : BaseEntity
{
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
    public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? OldValuesJson { get; set; }
    public string? NewValuesJson { get; set; }
    public string? CorrelationId { get; set; }
}

public sealed class UserActivityAudit : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Activity { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTimeOffset ActivityAt { get; set; } = DateTimeOffset.UtcNow;
}
