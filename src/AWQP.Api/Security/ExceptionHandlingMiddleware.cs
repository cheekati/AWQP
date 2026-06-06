using System.Net;
using System.Text.Json;
using FluentValidation;

namespace AWQP.Api.Security;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled API exception");
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex switch
            {
                ValidationException => (int)HttpStatusCode.BadRequest,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                InvalidOperationException => (int)HttpStatusCode.NotFound,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var payload = new
            {
                traceId = context.TraceIdentifier,
                status = context.Response.StatusCode,
                error = ex.Message
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
