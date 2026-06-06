using AWQP.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AWQP.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IJwtTokenService jwtTokenService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public Task<AuthResponse> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        return jwtTokenService.AuthenticateAsync(request, cancellationToken);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public Task<AuthResponse> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        return jwtTokenService.RefreshAsync(request, cancellationToken);
    }
}
