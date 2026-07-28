using ChangeManagement.Domain.Common;

namespace ChangeManagement.Domain.Entities;

public class Comment : BaseEntity
{
    public Guid EngineeringRequestId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Stage { get; set; }

    public EngineeringRequest EngineeringRequest { get; set; } = null!;
    public User User { get; set; } = null!;
}
