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
    private readonly IWorkPermitService _workPermitService;
    private readonly ICheckSheetService _checkSheetService;

    public EhsController(IPpeService ppeService, IWorkPermitService workPermitService, ICheckSheetService checkSheetService)
    {
        _ppeService = ppeService;
        _workPermitService = workPermitService;
        _checkSheetService = checkSheetService;
    }

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

    // --- Work Permit Supplier Progress (front page with daily atmospheric data + ranges) ---

    [HttpGet("work-permits/acceptable-ranges")]
    public async Task<IActionResult> AcceptableRanges(CancellationToken cancellationToken) =>
        Ok(await _workPermitService.GetAcceptableRangesAsync(cancellationToken));

    [HttpGet("work-permits/progress/{supplierCode}")]
    public async Task<IActionResult> ProgressFrontPage(string supplierCode, CancellationToken cancellationToken) =>
        Ok(await _workPermitService.GetProgressFrontPageAsync(supplierCode, cancellationToken));

    [HttpGet("work-permits/{id:guid}")]
    public async Task<IActionResult> WorkPermit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _workPermitService.GetByIdAsync(id, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpPut("work-permits/{id:guid}/progress")]
    public async Task<IActionResult> UpdateProgress(Guid id, UpdateProgressWorkPermitRequest request, CancellationToken cancellationToken)
    {
        var result = await _workPermitService.UpdateProgressAsync(id, request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpPost("work-permits/{id:guid}/daily-readings")]
    public async Task<IActionResult> AddDailyReading(Guid id, AddDailyAtmosphericReadingRequest request, CancellationToken cancellationToken)
    {
        var result = await _workPermitService.AddDailyReadingAsync(id, request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    // --- Check Point Master ---

    [HttpGet("check-points")]
    public async Task<IActionResult> CheckPoints(CancellationToken cancellationToken) =>
        Ok(await _checkSheetService.ListCheckPointsAsync(cancellationToken));

    [HttpGet("check-points/{id:guid}")]
    public async Task<IActionResult> CheckPoint(Guid id, CancellationToken cancellationToken)
    {
        var result = await _checkSheetService.GetCheckPointAsync(id, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpPost("check-points")]
    public async Task<IActionResult> CreateCheckPoint(CreateCheckPointMasterRequest request, CancellationToken cancellationToken)
    {
        var result = await _checkSheetService.CreateCheckPointAsync(request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpPut("check-points/{id:guid}")]
    public async Task<IActionResult> UpdateCheckPoint(Guid id, UpdateCheckPointMasterRequest request, CancellationToken cancellationToken)
    {
        var result = await _checkSheetService.UpdateCheckPointAsync(id, request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpPut("check-points/{id:guid}/specs")]
    public async Task<IActionResult> UpdateCheckPointSpecs(Guid id, UpdateCheckPointSpecsRequest request, CancellationToken cancellationToken)
    {
        var result = await _checkSheetService.UpdateCheckPointSpecsAsync(id, request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpDelete("check-points/{id:guid}")]
    public async Task<IActionResult> DeleteCheckPoint(Guid id, CancellationToken cancellationToken)
    {
        var result = await _checkSheetService.DeleteCheckPointAsync(id, cancellationToken);
        return result.Succeeded ? Ok(new { deleted = true }) : BadRequest(new { error = result.Error });
    }

    // --- CheckSheet Items Sorting ---

    [HttpGet("checksheets/departments")]
    public async Task<IActionResult> CheckSheetDepartments(CancellationToken cancellationToken) =>
        Ok(await _checkSheetService.ListDepartmentsAsync(cancellationToken));

    [HttpGet("checksheets/sections")]
    public async Task<IActionResult> CheckSheetSections([FromQuery] string departmentCode, CancellationToken cancellationToken) =>
        Ok(await _checkSheetService.ListSectionsAsync(departmentCode, cancellationToken));

    [HttpGet("checksheets/machines")]
    public async Task<IActionResult> CheckSheetMachines([FromQuery] string departmentCode, [FromQuery] string sectionCode, CancellationToken cancellationToken) =>
        Ok(await _checkSheetService.ListMachineNamesAsync(departmentCode, sectionCode, cancellationToken));

    [HttpGet("checksheets/frequencies")]
    public async Task<IActionResult> CheckSheetFrequencies(
        [FromQuery] string departmentCode,
        [FromQuery] string sectionCode,
        [FromQuery] string machineName,
        CancellationToken cancellationToken) =>
        Ok(await _checkSheetService.ListFrequenciesAsync(departmentCode, sectionCode, machineName, cancellationToken));

    [HttpGet("checksheets/definition")]
    public async Task<IActionResult> CheckSheetDefinition(
        [FromQuery] string departmentCode,
        [FromQuery] string sectionCode,
        [FromQuery] string machineName,
        [FromQuery] string frequency,
        CancellationToken cancellationToken)
    {
        var result = await _checkSheetService.GetDefinitionAsync(departmentCode, sectionCode, machineName, frequency, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpGet("checksheets/grid")]
    public async Task<IActionResult> CheckSheetGrid(
        [FromQuery] string departmentCode,
        [FromQuery] string sectionCode,
        [FromQuery] string machineName,
        [FromQuery] string frequency,
        [FromQuery] DateTime checkingDate,
        CancellationToken cancellationToken) =>
        Ok(await _checkSheetService.GetGridAsync(departmentCode, sectionCode, machineName, frequency, checkingDate, cancellationToken));

    [HttpGet("checksheets/dates")]
    public async Task<IActionResult> CheckSheetDates(
        [FromQuery] string departmentCode,
        [FromQuery] string sectionCode,
        [FromQuery] string machineName,
        [FromQuery] string frequency,
        CancellationToken cancellationToken) =>
        Ok(await _checkSheetService.GetCheckedDatesAsync(departmentCode, sectionCode, machineName, frequency, cancellationToken));

    [HttpPost("checksheets/rows")]
    public async Task<IActionResult> SaveCheckSheetRow(SaveCheckSheetRowRequest request, CancellationToken cancellationToken)
    {
        var result = await _checkSheetService.SaveRowAsync(request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}
