using ChangeManagement.Domain.Common;
using ChangeManagement.Domain.Enums;

namespace ChangeManagement.Domain.Entities;

public class EngineeringRequestReason : BaseEntity
{
    public Guid EngineeringRequestId { get; set; }
    public ReasonForChange Reason { get; set; }
    public string? OtherDescription { get; set; }

    public EngineeringRequest EngineeringRequest { get; set; } = null!;
}
