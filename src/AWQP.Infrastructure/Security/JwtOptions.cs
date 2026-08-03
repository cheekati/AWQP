namespace AWQP.Infrastructure.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "AWQP";
    public string Audience { get; set; } = "AWQP.Clients";
    public string SigningKey { get; set; } = "replace-with-256-bit-secret-from-key-vault";
    public int AccessTokenMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 14;
}
