using ChangeManagement.Application.DTOs.MasterData;
using ChangeManagement.Application.Interfaces;
using ChangeManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChangeManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MasterDataController : ControllerBase
{
    private readonly IMasterDataService _masterDataService;

    public MasterDataController(IMasterDataService masterDataService)
    {
        _masterDataService = masterDataService;
    }

    [HttpGet("departments")]
    public async Task<IActionResult> Departments(CancellationToken cancellationToken) =>
        Ok(await _masterDataService.GetDepartmentsAsync(cancellationToken));

    [HttpGet("divisions")]
    public async Task<IActionResult> Divisions(CancellationToken cancellationToken) =>
        Ok(await _masterDataService.GetDivisionsAsync(cancellationToken));

    [HttpGet("products")]
    public async Task<IActionResult> Products(CancellationToken cancellationToken) =>
        Ok(await _masterDataService.GetProductsAsync(cancellationToken));

    [HttpGet("customers")]
    public async Task<IActionResult> Customers(CancellationToken cancellationToken) =>
        Ok(await _masterDataService.GetCustomersAsync(cancellationToken));

    [HttpGet("roles")]
    public async Task<IActionResult> Roles(CancellationToken cancellationToken) =>
        Ok(await _masterDataService.GetRolesAsync(cancellationToken));

    [HttpPost("departments")]
    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateLookupRequest request, CancellationToken cancellationToken) =>
        Ok(await _masterDataService.CreateDepartmentAsync(request, cancellationToken));

    [HttpPost("divisions")]
    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> CreateDivision([FromBody] CreateLookupRequest request, CancellationToken cancellationToken) =>
        Ok(await _masterDataService.CreateDivisionAsync(request, cancellationToken));

    [HttpPost("products")]
    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateLookupRequest request, CancellationToken cancellationToken) =>
        Ok(await _masterDataService.CreateProductAsync(request, cancellationToken));

    [HttpPost("customers")]
    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateLookupRequest request, CancellationToken cancellationToken) =>
        Ok(await _masterDataService.CreateCustomerAsync(request, cancellationToken));
}
