using ChangeManagement.Domain.Common;
using ChangeManagement.Domain.Enums;

namespace ChangeManagement.Domain.Entities;

/// <summary>
/// Tracks pending verification assignments for each ER stage.
/// </summary>
public class VerificationAssignment : BaseEntity
{
    public Guid EngineeringRequestId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public VerificationStage Stage { get; set; }
    public bool IsCompleted { get; set; }
    public VerificationAction? Result { get; set; }
    public DateTime? CompletedDate { get; set; }

    public EngineeringRequest EngineeringRequest { get; set; } = null!;
    public User? AssignedToUser { get; set; }
}
