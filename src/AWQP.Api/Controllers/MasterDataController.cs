using AWQP.Application.Common;
using AWQP.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AWQP.Api.Controllers;

[ApiController]
[Authorize]
public sealed class CustomersController(CustomerService customerService) : ControllerBase
{
    [HttpGet("api/customers")]
    [Authorize(Roles = "Admin,Sales Team,Production Manager")]
    public Task<IReadOnlyList<CustomerDto>> Get(CancellationToken cancellationToken)
    {
        return customerService.GetCustomersAsync(cancellationToken);
    }

    [HttpPost("api/customers")]
    [Authorize(Roles = "Admin,Sales Team")]
    public Task<CustomerDto> Create(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        return customerService.CreateCustomerAsync(request, cancellationToken);
    }
}

[ApiController]
[Authorize]
public sealed class ProductsController(ProductService productService) : ControllerBase
{
    [HttpGet("api/products")]
    [Authorize(Roles = "Admin,Sales Team,Production Manager,Production Engineer,Quality Engineer")]
    public Task<IReadOnlyList<ProductDto>> Get(CancellationToken cancellationToken)
    {
        return productService.GetProductsAsync(cancellationToken);
    }

    [HttpPost("api/products")]
    [Authorize(Roles = "Admin,Production Engineer")]
    public Task<ProductDto> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        return productService.CreateProductAsync(request, cancellationToken);
    }
}
