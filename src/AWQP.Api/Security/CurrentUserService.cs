using System.Security.Claims;
using AWQP.Application.Common;

namespace AWQP.Api.Security;

public sealed class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    public string UserId => accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
    public string UserName => accessor.HttpContext?.User.Identity?.Name ?? "system";
    public string IpAddress => accessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
