using AWQP.Application.DTOs;
using AWQP.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AWQP.Api.Controllers;

[ApiController]
[Route("api/ehs")]
[Authorize]
public sealed class EhsController : ControllerBase
{
    private readonly IPpeService _ppeService;

    public EhsController(IPpeService ppeService) => _ppeService = ppeService;

    // --- Employees (PPE Request employee-number binding) ---

    [HttpGet("employees")]
    public async Task<IActionResult> Employees(CancellationToken cancellationToken) =>
        Ok(await _ppeService.ListEmployeesAsync(cancellationToken));

    [HttpGet("employees/{employeeNumber}")]
    public async Task<IActionResult> Employee(string employeeNumber, CancellationToken cancellationToken)
    {
        var result = await _ppeService.GetEmployeeByNumberAsync(employeeNumber, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    // --- PPE Master ---

    [HttpGet("ppe-items")]
    public async Task<IActionResult> Items(CancellationToken cancellationToken) =>
        Ok(await _ppeService.ListItemsAsync(cancellationToken));

    [HttpPost("ppe-items")]
    public async Task<IActionResult> CreateItem(CreatePpeItemRequest request, CancellationToken cancellationToken)
    {
        var result = await _ppeService.CreateItemAsync(request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpPut("ppe-items/{id:guid}")]
    public async Task<IActionResult> UpdateItem(Guid id, UpdatePpeItemRequest request, CancellationToken cancellationToken)
    {
        var result = await _ppeService.UpdateItemAsync(id, request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    // --- PPE Request ---

    [HttpGet("ppe-requests")]
    public async Task<IActionResult> Requests(CancellationToken cancellationToken) =>
        Ok(await _ppeService.ListRequestsAsync(cancellationToken));

    [HttpPost("ppe-requests")]
    public async Task<IActionResult> CreateRequest(CreatePpeRequestRequest request, CancellationToken cancellationToken)
    {
        var result = await _ppeService.CreateRequestAsync(request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpPost("ppe-requests/{id:guid}/issue")]
    public async Task<IActionResult> Issue(Guid id, IssuePpeRequest request, CancellationToken cancellationToken)
    {
        var result = await _ppeService.IssueAsync(id, request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    // --- PPE Issue ---

    [HttpGet("ppe-issues")]
    public async Task<IActionResult> Issues(CancellationToken cancellationToken) =>
        Ok(await _ppeService.ListIssuesAsync(cancellationToken));

    // --- PPE Inventory ---

    [HttpGet("inventory")]
    public async Task<IActionResult> Inventory(CancellationToken cancellationToken) =>
        Ok(await _ppeService.ListInventoryAsync(cancellationToken));

    [HttpGet("inventory/entries")]
    public async Task<IActionResult> StockEntries(CancellationToken cancellationToken) =>
        Ok(await _ppeService.ListStockEntriesAsync(cancellationToken));

    [HttpGet("inventory/{ppeItemId:guid}/balance")]
    public async Task<IActionResult> Balance(Guid ppeItemId, CancellationToken cancellationToken) =>
        Ok(new { ppeItemId, balance = await _ppeService.GetBalanceAsync(ppeItemId, cancellationToken) });

    [HttpPost("inventory")]
    public async Task<IActionResult> AddStock(AddPpeStockRequest request, CancellationToken cancellationToken)
    {
        var result = await _ppeService.AddStockAsync(request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}
