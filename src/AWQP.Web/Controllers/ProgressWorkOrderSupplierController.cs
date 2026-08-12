using System.Net.Http.Headers;
using System.Text;
using AWQP.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace AWQP.Web.Controllers;

/// <summary>
/// Supplier-facing Work Permit In Progress screen (route: /progressworkorderSupplier).
/// Front page displays all daily atmospheric readings with acceptable ranges per parameter.
/// </summary>
public sealed class ProgressWorkOrderSupplierController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ApiTokenProvider _tokenProvider;
    private readonly IHttpClientFactory _httpClientFactory;

    public ProgressWorkOrderSupplierController(
        IConfiguration configuration,
        ApiTokenProvider tokenProvider,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _tokenProvider = tokenProvider;
        _httpClientFactory = httpClientFactory;
    }

    private void PopulateApiContext()
    {
        ViewBag.ApiBaseUrl = "/progressworkorderSupplier";
        ViewBag.ApiToken = string.Empty;
        ViewBag.SupplierCode = Request.Query["supplier"].FirstOrDefault() ?? "SUP001";
    }

    [HttpGet("/progressworkorderSupplier")]
    public IActionResult Index()
    {
        PopulateApiContext();
        return View();
    }

    [AcceptVerbs("GET", "POST", "PUT", "DELETE")]
    [Route("progressworkorderSupplier/api/ehs/{*path}")]
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
}
