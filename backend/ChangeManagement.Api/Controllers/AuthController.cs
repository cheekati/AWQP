using System.Security.Claims;
using ChangeManagement.Api.DTOs;
using ChangeManagement.Api.Models;
using ChangeManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChangeManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await _auth.LoginAsync(request);
        if (result is null) return Unauthorized(new { message = "Invalid email or password." });
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Me()
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Unauthorized();
        return Ok(AuthService.ToDto(user));
    }

    private async Task<User?> GetCurrentUserAsync()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var id)) return null;
        return await _auth.GetUserAsync(id);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly Data.AppDbContext _db;

    public UsersController(IAuthService auth, Data.AppDbContext db)
    {
        _auth = auth;
        _db = db;
    }

    [HttpGet]
    public ActionResult<IEnumerable<UserDto>> List([FromQuery] string? role)
    {
        var query = _db.Users.Where(u => u.IsActive);
        if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<UserRole>(role, true, out var parsed))
            query = query.Where(u => u.Role == parsed);

        return Ok(query.OrderBy(u => u.FullName).Select(u => AuthService.ToDto(u)).ToList());
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LookupsController : ControllerBase
{
    private readonly Data.AppDbContext _db;

    public LookupsController(Data.AppDbContext db) => _db = db;

    [HttpGet]
    public ActionResult<IEnumerable<LookupDto>> GetAll()
    {
        var groups = _db.LookupItems
            .Where(l => l.IsActive)
            .OrderBy(l => l.SortOrder)
            .AsEnumerable()
            .GroupBy(l => l.Category)
            .Select(g => new LookupDto(g.Key, g.Select(x => x.Value).ToList()))
            .ToList();
        return Ok(groups);
    }
}
