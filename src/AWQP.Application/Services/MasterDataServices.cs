using AutoMapper;
using AutoMapper.QueryableExtensions;
using AWQP.Application.Common;
using AWQP.Domain.Customers;
using AWQP.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace AWQP.Application.Services;

public sealed class CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
{
    public async Task<IReadOnlyList<CustomerDto>> GetCustomersAsync(CancellationToken cancellationToken)
    {
        return await unitOfWork.Repository<Customer>()
            .Query()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ProjectTo<CustomerDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<Customer>(request);
        await unitOfWork.Repository<Customer>().AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return mapper.Map<CustomerDto>(entity);
    }
}

public sealed class ProductService(IUnitOfWork unitOfWork, IMapper mapper)
{
    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(CancellationToken cancellationToken)
    {
        return await unitOfWork.Repository<Product>()
            .Query()
            .Include(x => x.ProductCategory)
            .Include(x => x.MaterialGrade)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Code)
            .ProjectTo<ProductDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<Product>(request);
        await unitOfWork.Repository<Product>().AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return mapper.Map<ProductDto>(entity);
    }
}
