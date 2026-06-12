using AWQP.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace AWQP.Web.Controllers;

public sealed class PpeController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ApiTokenProvider _tokenProvider;

    public PpeController(IConfiguration configuration, ApiTokenProvider tokenProvider)
    {
        _configuration = configuration;
        _tokenProvider = tokenProvider;
    }

    private void PopulateApiContext()
    {
        ViewBag.ApiBaseUrl = _configuration["ApiBaseUrl"] ?? "http://localhost:5080";
        ViewBag.ApiToken = _tokenProvider.CreateToken();
    }

    public IActionResult Master() { PopulateApiContext(); return View(); }
    public IActionResult Request() { PopulateApiContext(); return View(); }
    public IActionResult Issue() { PopulateApiContext(); return View(); }
    public IActionResult Inventory() { PopulateApiContext(); return View(); }
}
