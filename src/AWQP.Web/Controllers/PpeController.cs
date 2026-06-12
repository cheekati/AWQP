using System.Net.Http.Headers;
using System.Text;
using AWQP.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace AWQP.Web.Controllers;

public sealed class PpeController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ApiTokenProvider _tokenProvider;
    private readonly IHttpClientFactory _httpClientFactory;

    public PpeController(IConfiguration configuration, ApiTokenProvider tokenProvider, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _tokenProvider = tokenProvider;
        _httpClientFactory = httpClientFactory;
    }

    private void PopulateApiContext()
    {
        // The screens call the same-origin proxy below; no token is exposed to the browser.
        ViewBag.ApiBaseUrl = "/Ppe";
        ViewBag.ApiToken = string.Empty;
    }

    public IActionResult Master() { PopulateApiContext(); return View(); }
    public IActionResult Request() { PopulateApiContext(); return View(); }
    public IActionResult Issue() { PopulateApiContext(); return View(); }
    public IActionResult Inventory() { PopulateApiContext(); return View(); }

    // Same-origin proxy: forwards /Ppe/api/ehs/* to the authorized API, adding a service token.
    [AcceptVerbs("GET", "POST", "PUT", "DELETE")]
    [Route("Ppe/api/ehs/{*path}")]
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
