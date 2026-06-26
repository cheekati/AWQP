namespace AWQP.Application.DTOs;

public sealed record LoginRequest(string UserName, string Password);
public sealed record RefreshTokenRequest(string RefreshToken);
public sealed record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc, string Role, string UserName);
public sealed record RegisterUserRequest(string UserName, string Email, string Password, string Role);
