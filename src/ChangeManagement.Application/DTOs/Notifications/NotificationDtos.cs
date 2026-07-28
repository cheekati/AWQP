using ChangeManagement.Domain.Enums;

namespace ChangeManagement.Application.DTOs.Notifications;

public class NotificationDto
{
    public Guid Id { get; set; }
    public NotificationType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public Guid? EngineeringRequestId { get; set; }
    public string? ErNumber { get; set; }
    public DateTime CreatedDate { get; set; }
}
