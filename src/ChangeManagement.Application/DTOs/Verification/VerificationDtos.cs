using ChangeManagement.Domain.Enums;

namespace ChangeManagement.Application.DTOs.Verification;

public class VerificationActionRequest
{
    public VerificationAction Action { get; set; }
    public string? Comments { get; set; }
}

public class CooActionRequest
{
    public VerificationAction Action { get; set; }
    public string? Comments { get; set; }
}

public class AddCommentRequest
{
    public string Content { get; set; } = string.Empty;
}
