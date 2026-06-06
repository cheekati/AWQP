using Microsoft.AspNetCore.Mvc;

namespace AWQP.Web.Controllers;

public sealed class DashboardController : Controller
{
    public IActionResult Index() => View();
}

public sealed class CustomersController : Controller
{
    public IActionResult Index() => View();
}

public sealed class WorkOrdersController : Controller
{
    public IActionResult Index() => View();
}

public sealed class QualityController : Controller
{
    public IActionResult Index() => View();
}

public sealed class InventoryController : Controller
{
    public IActionResult Index() => View();
}

public sealed class ShippingController : Controller
{
    public IActionResult Index() => View();
}
