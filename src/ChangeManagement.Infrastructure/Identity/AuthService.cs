using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using ChangeManagement.Application.DTOs.Auth;
using ChangeManagement.Application.DTOs.Common;
using ChangeManagement.Application.Interfaces;
using ChangeManagement.Domain.Entities;
using ChangeManagement.Domain.Interfaces;
using ChangeManagement.Infrastructure.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ChangeManagement.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly JwtSettings _jwtSettings;
    private readonly ICurrentUserService _currentUser;

    public AuthService(IUnitOfWork unitOfWork, IMapper mapper, IOptions<JwtSettings> jwtSettings, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _jwtSettings = jwtSettings.Value;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Repository<User>().Query()
            .Include(u => u.Department)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserName == request.UserName && !u.IsDeleted, cancellationToken);

        if (user is null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return ApiResponse<LoginResponse>.Fail("Invalid username or password.");

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);
        var token = GenerateToken(user, roles, expires);

        return ApiResponse<LoginResponse>.Ok(new LoginResponse
        {
            Token = token,
            ExpiresAt = expires,
            User = _mapper.Map<UserInfoDto>(user)
        });
    }

    public async Task<ApiResponse<UserInfoDto>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await _unitOfWork.Repository<User>()
            .AnyAsync(u => u.UserName == request.UserName || u.Email == request.Email, cancellationToken);
        if (exists)
            return ApiResponse<UserInfoDto>.Fail("Username or email already exists.");

        var roles = await _unitOfWork.Repository<Role>().Query()
            .Where(r => request.Roles.Contains(r.Name))
            .ToListAsync(cancellationToken);

        if (roles.Count == 0)
            return ApiResponse<UserInfoDto>.Fail("At least one valid role is required.");

        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmployeeId = request.EmployeeId,
            DepartmentId = request.DepartmentId,
            IsActive = true,
            CreatedBy = _currentUser.UserName ?? "system"
        };

        foreach (var role in roles)
        {
            user.UserRoles.Add(new UserRole { RoleId = role.Id });
        }

        await _unitOfWork.Repository<User>().AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _unitOfWork.Repository<User>().Query()
            .Include(u => u.Department)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstAsync(u => u.Id == user.Id, cancellationToken);

        return ApiResponse<UserInfoDto>.Ok(_mapper.Map<UserInfoDto>(created), "User registered successfully.");
    }

    public async Task<ApiResponse<UserInfoDto>> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is null)
            return ApiResponse<UserInfoDto>.Fail("Unauthorized.");

        var user = await _unitOfWork.Repository<User>().Query()
            .Include(u => u.Department)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId && !u.IsDeleted, cancellationToken);

        if (user is null)
            return ApiResponse<UserInfoDto>.Fail("User not found.");

        return ApiResponse<UserInfoDto>.Ok(_mapper.Map<UserInfoDto>(user));
    }

    private string GenerateToken(User user, IEnumerable<string> roles, DateTime expires)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(ClaimTypes.Name, user.UserName),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("fullName", user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var id = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return Guid.TryParse(id, out var guid) ? guid : null;
        }
    }

    public string? UserName => User?.Identity?.Name ?? User?.FindFirstValue(JwtRegisteredClaimNames.UniqueName);

    public IReadOnlyList<string> Roles =>
        User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();

    public bool IsInRole(string role) => User?.IsInRole(role) == true;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
}
