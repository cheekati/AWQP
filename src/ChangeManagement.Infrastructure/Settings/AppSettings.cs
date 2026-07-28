namespace ChangeManagement.Infrastructure.Settings;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    public string Secret { get; set; } = "ChangeManagement_SuperSecret_Key_AtLeast32Chars!";
    public string Issuer { get; set; } = "ChangeManagementAPI";
    public string Audience { get; set; } = "ChangeManagementClient";
    public int ExpiryMinutes { get; set; } = 480;
}

public class EmailSettings
{
    public const string SectionName = "EmailSettings";
    public bool Enabled { get; set; }
    public string SmtpHost { get; set; } = "localhost";
    public int SmtpPort { get; set; } = 25;
    public string FromEmail { get; set; } = "noreply@changemanagement.local";
    public string FromName { get; set; } = "Change Management System";
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public bool UseSsl { get; set; }
}

public class FileStorageSettings
{
    public const string SectionName = "FileStorage";
    public string RootPath { get; set; } = "uploads";
}
