using Microsoft.AspNetCore.Mvc;

namespace AWQP.Web.Controllers;

public sealed class DashboardController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Production() => View();
    public IActionResult Quality() => View();
    public IActionResult Inventory() => View();
    public IActionResult CleanRoom() => View();
    public IActionResult Shipping() => View();
}
