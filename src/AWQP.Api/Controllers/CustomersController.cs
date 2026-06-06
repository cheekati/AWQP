using AWQP.Application.DTOs;
using AWQP.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AWQP.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public sealed class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    public CustomersController(ICustomerService customerService) => _customerService = customerService;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken) => Ok(await _customerService.ListAsync(cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Admin,SalesTeam")]
    public async Task<IActionResult> Create(CreateCustomerRequest request, CancellationToken cancellationToken) => Ok(await _customerService.CreateAsync(request, cancellationToken));
}
