using System.Net;
using System.Net.Mail;
using ChangeManagement.Application.Interfaces;
using ChangeManagement.Infrastructure.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChangeManagement.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Email disabled. To={To} Subject={Subject} Body={Body}", toEmail, subject, body);
            return;
        }

        using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            EnableSsl = _settings.UseSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrWhiteSpace(_settings.UserName))
        {
            client.Credentials = new NetworkCredential(_settings.UserName, _settings.Password);
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        await client.SendMailAsync(message, cancellationToken);
        _logger.LogInformation("Email sent to {To} subject {Subject}", toEmail, subject);
    }
}

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tif", ".tiff"
    };

    public LocalFileStorageService(IOptions<FileStorageSettings> settings)
    {
        _rootPath = Path.GetFullPath(settings.Value.RootPath);
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<(string StoredFileName, string StoragePath, bool IsImage)> SaveAsync(IFormFile file, string folder, CancellationToken cancellationToken = default)
    {
        var safeFolder = Path.Combine(_rootPath, folder);
        Directory.CreateDirectory(safeFolder);

        var ext = Path.GetExtension(file.FileName);
        var stored = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(safeFolder, stored);

        await using var stream = File.Create(fullPath);
        await file.CopyToAsync(stream, cancellationToken);

        var relative = Path.Combine(folder, stored).Replace('\\', '/');
        return (stored, relative, ImageExtensions.Contains(ext));
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_rootPath, storagePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public Task<(Stream Stream, string ContentType)?> OpenReadAsync(string storagePath, string contentType, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_rootPath, storagePath);
        if (!File.Exists(fullPath))
            return Task.FromResult<(Stream, string)?>(null);

        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult<(Stream, string)?>((stream, contentType));
    }
}
