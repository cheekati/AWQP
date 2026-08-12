using System.Net.Http.Headers;
using System.Text;
using AWQP.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace AWQP.Web.Controllers;

public sealed class CheckSheetController : Controller
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".gif", ".png", ".webp"
    };

    private readonly IConfiguration _configuration;
    private readonly ApiTokenProvider _tokenProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWebHostEnvironment _environment;

    public CheckSheetController(
        IConfiguration configuration,
        ApiTokenProvider tokenProvider,
        IHttpClientFactory httpClientFactory,
        IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _tokenProvider = tokenProvider;
        _httpClientFactory = httpClientFactory;
        _environment = environment;
    }

    private void PopulateApiContext()
    {
        ViewBag.ApiBaseUrl = "/CheckSheet";
        ViewBag.ApiToken = string.Empty;
    }

    /// <summary>Check Point Master (also available as Blazor route /checksheet).</summary>
    public IActionResult Index()
    {
        PopulateApiContext();
        return View();
    }

    /// <summary>Items Sorting / CheckSheet Data (also available as Blazor route /ckpage).</summary>
    public IActionResult ItemsSorting()
    {
        PopulateApiContext();
        return View();
    }

    [AcceptVerbs("GET", "POST", "PUT", "DELETE")]
    [Route("CheckSheet/api/ehs/{*path}")]
    public async Task<IActionResult> Proxy(string path, CancellationToken cancellationToken)
    {
        var apiBase = (_configuration["ApiBaseUrl"] ?? "http://localhost:5080").TrimEnd('/');
        var target = $"{apiBase}/api/ehs/{path}{HttpContext.Request.QueryString}";

        using var message = new HttpRequestMessage(new HttpMethod(HttpContext.Request.Method), target);
        if (HttpContext.Request.ContentLength is > 0)
        {
            using var reader = new StreamReader(HttpContext.Request.Body);
            var body = await reader.ReadToEndAsync(cancellationToken);
            message.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.CreateToken());

        var client = _httpClientFactory.CreateClient();
        using var response = await client.SendAsync(message, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return new ContentResult
        {
            StatusCode = (int)response.StatusCode,
            Content = responseBody,
            ContentType = "application/json"
        };
    }

    /// <summary>Upload up to one image file; returns relative path for Image1–Image4 slots.</summary>
    [HttpPost]
    [Route("CheckSheet/api/upload")]
    [RequestSizeLimit(10_485_760)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded." });

        var ext = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(ext))
            return BadRequest(new { error = "Allowed file types: .jpg, .jpeg, .gif, .png, .webp" });

        if (file.Length > 10_485_760)
            return BadRequest(new { error = "Max file size is 10 MB." });

        var now = DateTime.Now;
        var relativeDir = Path.Combine("uploads", "CheckSheet", "Image", now.Year.ToString(), now.Month.ToString(), now.Day.ToString());
        var physicalDir = Path.Combine(_environment.WebRootPath, relativeDir);
        Directory.CreateDirectory(physicalDir);

        var safeName = Path.GetFileName(file.FileName);
        var uniqueName = $"{Guid.NewGuid():N}_{safeName}";
        var physicalPath = Path.Combine(physicalDir, uniqueName);
        await using (var stream = System.IO.File.Create(physicalPath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var webPath = "/" + relativeDir.Replace('\\', '/') + "/" + uniqueName;
        return Ok(new { path = webPath });
    }
}
