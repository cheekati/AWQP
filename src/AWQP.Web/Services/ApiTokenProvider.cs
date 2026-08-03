using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AWQP.Web.Services;

// Mints a short-lived service token so the EHS/PPE screens can call the
// (authorized) API. Uses the shared Jwt settings configured for the API.
public sealed class ApiTokenProvider
{
    private readonly IConfiguration _configuration;

    public ApiTokenProvider(IConfiguration configuration) => _configuration = configuration;

    public string CreateToken()
    {
        var section = _configuration.GetSection("Jwt");
        var signingKey = section["SigningKey"] ?? "replace-this-development-key-with-key-vault-secret-32chars-min";
        var issuer = section["Issuer"] ?? "AWQP";
        var audience = section["Audience"] ?? "AWQP.Clients";

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "ehs-portal"),
            new Claim(ClaimTypes.Name, "ehs-portal"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer, audience, claims, expires: DateTime.UtcNow.AddHours(1), signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
