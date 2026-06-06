using AWQP.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace AWQP.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public bool MustChangePassword { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}

public sealed class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public bool IsActive => RevokedAt is null && DateTimeOffset.UtcNow <= ExpiresAt;
}
