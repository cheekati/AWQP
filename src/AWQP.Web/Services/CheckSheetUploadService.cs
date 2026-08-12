namespace AWQP.Web.Services;

public sealed class CheckSheetUploadService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".gif", ".png", ".webp"
    };

    private readonly IWebHostEnvironment _environment;

    public CheckSheetUploadService(IWebHostEnvironment environment) => _environment = environment;

    public async Task<(bool Ok, string? Path, string? Error)> SaveAsync(Stream content, string fileName, CancellationToken ct = default)
    {
        if (content is null || content == Stream.Null)
            return (false, null, "No file uploaded.");

        var ext = Path.GetExtension(fileName);
        if (!AllowedExtensions.Contains(ext))
            return (false, null, "Allowed file types: .jpg, .jpeg, .gif, .png, .webp");

        var now = DateTime.Now;
        var relativeDir = Path.Combine("uploads", "CheckSheet", "Image", now.Year.ToString(), now.Month.ToString(), now.Day.ToString());
        var physicalDir = Path.Combine(_environment.WebRootPath, relativeDir);
        Directory.CreateDirectory(physicalDir);

        var safeName = Path.GetFileName(fileName);
        var uniqueName = $"{Guid.NewGuid():N}_{safeName}";
        var physicalPath = Path.Combine(physicalDir, uniqueName);
        await using (var stream = File.Create(physicalPath))
        {
            await content.CopyToAsync(stream, ct);
        }

        var webPath = "/" + relativeDir.Replace('\\', '/') + "/" + uniqueName;
        return (true, webPath, null);
    }
}
