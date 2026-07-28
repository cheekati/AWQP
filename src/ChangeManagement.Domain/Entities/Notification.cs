using ChangeManagement.Domain.Common;
using ChangeManagement.Domain.Enums;

namespace ChangeManagement.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? EngineeringRequestId { get; set; }
    public NotificationType Type { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public bool EmailSent { get; set; }
    public DateTime? EmailSentDate { get; set; }

    public User User { get; set; } = null!;
    public EngineeringRequest? EngineeringRequest { get; set; }
}
