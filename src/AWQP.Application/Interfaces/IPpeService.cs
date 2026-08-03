using AWQP.Application.Common;
using AWQP.Application.DTOs;

namespace AWQP.Application.Interfaces;

public interface IPpeService
{
    // Employees (PPE Request employee-number binding)
    Task<IReadOnlyCollection<EmployeeDto>> ListEmployeesAsync(CancellationToken cancellationToken = default);
    Task<Result<EmployeeDto>> GetEmployeeByNumberAsync(string employeeNumber, CancellationToken cancellationToken = default);

    // PPE Master
    Task<IReadOnlyCollection<PpeItemDto>> ListItemsAsync(CancellationToken cancellationToken = default);
    Task<Result<PpeItemDto>> CreateItemAsync(CreatePpeItemRequest request, CancellationToken cancellationToken = default);
    Task<Result<PpeItemDto>> UpdateItemAsync(Guid id, UpdatePpeItemRequest request, CancellationToken cancellationToken = default);

    // PPE Request
    Task<IReadOnlyCollection<PpeRequestDto>> ListRequestsAsync(CancellationToken cancellationToken = default);
    Task<Result<PpeRequestDto>> CreateRequestAsync(CreatePpeRequestRequest request, CancellationToken cancellationToken = default);

    // PPE Issue (triggered from request, captures receiver signature)
    Task<Result<PpeIssueDto>> IssueAsync(Guid requestId, IssuePpeRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PpeIssueDto>> ListIssuesAsync(CancellationToken cancellationToken = default);

    // PPE Inventory
    Task<IReadOnlyCollection<PpeInventoryDto>> ListInventoryAsync(CancellationToken cancellationToken = default);
    Task<int> GetBalanceAsync(Guid ppeItemId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PpeStockEntryDto>> ListStockEntriesAsync(CancellationToken cancellationToken = default);
    Task<Result<PpeStockEntryDto>> AddStockAsync(AddPpeStockRequest request, CancellationToken cancellationToken = default);
}
