using AWQP.Domain.Documents;
using AWQP.Infrastructure.Persistence;

namespace AWQP.Api.Security;

public sealed class UserActivityAuditMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, AwqpDbContext dbContext)
    {
        await next(context);

        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        dbContext.UserActivityAudits.Add(new UserActivityAudit
        {
            UserId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
            UserName = context.User.Identity.Name ?? string.Empty,
            Activity = $"{context.Request.Method} {context.Request.Path} => {context.Response.StatusCode}",
            IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            UserAgent = context.Request.Headers.UserAgent.ToString(),
            ActivityAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(context.RequestAborted);
    }
}
