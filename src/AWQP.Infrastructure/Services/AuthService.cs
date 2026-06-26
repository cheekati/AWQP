using AWQP.Application.Common;
using AWQP.Application.DTOs;
using AWQP.Application.Interfaces;
using AWQP.Domain.Entities;
using AWQP.Domain.Enums;
using AWQP.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AWQP.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly JwtOptions _jwtOptions;

    public AuthService(IUnitOfWork unitOfWork, ITokenService tokenService, IOptions<JwtOptions> jwtOptions)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Repository<ApplicationUser>().Query()
            .Include(x => x.RefreshTokens)
            .FirstOrDefaultAsync(x => x.UserName == request.UserName && x.IsActive, cancellationToken);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Result<AuthResponse>.Failure("Invalid credentials.");
        }

        var refreshToken = _tokenService.CreateRefreshToken();
        user.RefreshTokens.Add(new RefreshToken
        {
            ApplicationUserId = user.Id,
            TokenHash = _tokenService.HashToken(refreshToken),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays)
        });
        user.LastLoginAtUtc = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<AuthResponse>.Success(_tokenService.CreateToken(user, refreshToken));
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role))
        {
            return Result<AuthResponse>.Failure("Unknown role.");
        }
        var exists = await _unitOfWork.Repository<ApplicationUser>().Query().AnyAsync(x => x.UserName == request.UserName || x.Email == request.Email, cancellationToken);
        if (exists)
        {
            return Result<AuthResponse>.Failure("User already exists.");
        }
        var user = new ApplicationUser { UserName = request.UserName, Email = request.Email, Role = role, PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 13) };
        var refreshToken = _tokenService.CreateRefreshToken();
        user.RefreshTokens.Add(new RefreshToken { TokenHash = _tokenService.HashToken(refreshToken), ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays) });
        await _unitOfWork.Repository<ApplicationUser>().AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<AuthResponse>.Success(_tokenService.CreateToken(user, refreshToken));
    }
}
