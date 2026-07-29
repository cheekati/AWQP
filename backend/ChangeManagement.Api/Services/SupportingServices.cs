using ChangeManagement.Api.Data;
using ChangeManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Services;

public interface IEmailService
{
    Task NotifyAsync(EngineeringRequest er, string toEmail, string subject, string body);
}

/// <summary>Logs outbound emails. Swap for SMTP/SendGrid in production.</summary>
public class EmailService : IEmailService
{
    private readonly AppDbContext _db;
    private readonly ILogger<EmailService> _logger;

    public EmailService(AppDbContext db, ILogger<EmailService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task NotifyAsync(EngineeringRequest er, string toEmail, string subject, string body)
    {
        _logger.LogInformation("EMAIL to {To}: {Subject} | ER {Er}", toEmail, subject, er.DisplayErNumber);

        _db.EmailLogs.Add(new EmailLog
        {
            EngineeringRequestId = er.Id,
            ToEmail = toEmail,
            Subject = subject,
            Body = body,
            SentAt = DateTime.UtcNow,
            Success = true
        });
        await _db.SaveChangesAsync();
    }
}

public interface IErNumberService
{
    Task<(string ErNumber, DateTime ValidationDate)> GenerateNextAsync();
}

public class ErNumberService : IErNumberService
{
    private readonly AppDbContext _db;

    public ErNumberService(AppDbContext db) => _db = db;

    public async Task<(string ErNumber, DateTime ValidationDate)> GenerateNextAsync()
    {
        var now = DateTime.UtcNow;
        var yearMonth = now.ToString("yyMM");

        var seq = await _db.ErSequences.FirstOrDefaultAsync(s => s.YearMonth == yearMonth);
        if (seq is null)
        {
            seq = new ErSequence { YearMonth = yearMonth, LastSequence = 0 };
            _db.ErSequences.Add(seq);
        }

        seq.LastSequence += 1;
        await _db.SaveChangesAsync();

        var erNumber = $"{yearMonth}{seq.LastSequence:D3}";
        return (erNumber, now);
    }
}

public interface IFileStorageService
{
    Task<(string StoredFileName, string ContentType, long Size, bool IsImage)> SaveAsync(IFormFile file);
    string GetRelativeUrl(string storedFileName);
    string GetPhysicalPath(string storedFileName);
    void Delete(string storedFileName);
}

public class FileStorageService : IFileStorageService
{
    private readonly string _root;
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tif", ".tiff"
    };

    public FileStorageService(IWebHostEnvironment env, IConfiguration config)
    {
        _root = Path.Combine(env.ContentRootPath, config["Storage:UploadPath"] ?? "wwwroot/uploads");
        Directory.CreateDirectory(_root);
    }

    public async Task<(string StoredFileName, string ContentType, long Size, bool IsImage)> SaveAsync(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName);
        var stored = $"{Guid.NewGuid():N}{ext}";
        var path = Path.Combine(_root, stored);

        await using var stream = File.Create(path);
        await file.CopyToAsync(stream);

        var isImage = ImageExtensions.Contains(ext) || (file.ContentType?.StartsWith("image/") ?? false);
        return (stored, string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType, file.Length, isImage);
    }

    public string GetRelativeUrl(string storedFileName) => $"/uploads/{storedFileName}";
    public string GetPhysicalPath(string storedFileName) => Path.Combine(_root, storedFileName);

    public void Delete(string storedFileName)
    {
        var path = GetPhysicalPath(storedFileName);
        if (File.Exists(path)) File.Delete(path);
    }
}
