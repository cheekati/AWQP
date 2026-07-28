using ChangeManagement.Domain.Common;
using ChangeManagement.Domain.Enums;

namespace ChangeManagement.Domain.Entities;

public class ApprovalHistory : BaseEntity
{
    public Guid EngineeringRequestId { get; set; }
    public Guid ApproverId { get; set; }
    public VerificationAction Action { get; set; }
    public string? Comments { get; set; }
    public DateTime ActionDate { get; set; } = DateTime.UtcNow;

    public EngineeringRequest EngineeringRequest { get; set; } = null!;
    public User Approver { get; set; } = null!;
}
