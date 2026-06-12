using AWQP.Application.Common;
using AWQP.Application.DTOs;
using AWQP.Application.Interfaces;
using AWQP.Domain.Entities;
using AWQP.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AWQP.Infrastructure.Services;

public sealed class PpeService : IPpeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public PpeService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyCollection<EmployeeDto>> ListEmployeesAsync(CancellationToken cancellationToken = default) =>
        await _unitOfWork.Repository<Employee>().Query()
            .Where(x => x.IsActive)
            .OrderBy(x => x.EmployeeNumber)
            .Select(x => new EmployeeDto(x.Id, x.EmployeeNumber, x.FullName, x.Department, x.Designation))
            .ToListAsync(cancellationToken);

    public async Task<Result<EmployeeDto>> GetEmployeeByNumberAsync(string employeeNumber, CancellationToken cancellationToken = default)
    {
        var normalized = (employeeNumber ?? string.Empty).Trim();
        var employee = await _unitOfWork.Repository<Employee>().Query()
            .Where(x => x.EmployeeNumber == normalized && x.IsActive)
            .Select(x => new EmployeeDto(x.Id, x.EmployeeNumber, x.FullName, x.Department, x.Designation))
            .FirstOrDefaultAsync(cancellationToken);
        return employee is null
            ? Result<EmployeeDto>.Failure($"No active employee found for employee number '{normalized}'.")
            : Result<EmployeeDto>.Success(employee);
    }

    public async Task<IReadOnlyCollection<PpeItemDto>> ListItemsAsync(CancellationToken cancellationToken = default)
    {
        var balances = await GetBalanceMapAsync(cancellationToken);
        var items = await _unitOfWork.Repository<PpeItem>().Query()
            .OrderBy(x => x.ItemCode)
            .Select(x => new { x.Id, x.ItemCode, x.Name, x.Category, x.Price, x.LifetimeMonths, x.UnitOfMeasure })
            .ToListAsync(cancellationToken);
        return items
            .Select(x => new PpeItemDto(x.Id, x.ItemCode, x.Name, x.Category, x.Price, x.LifetimeMonths, x.UnitOfMeasure, balances.GetValueOrDefault(x.Id)))
            .ToList();
    }

    public async Task<Result<PpeItemDto>> CreateItemAsync(CreatePpeItemRequest request, CancellationToken cancellationToken = default)
    {
        var code = request.ItemCode.Trim();
        var exists = await _unitOfWork.Repository<PpeItem>().Query().AnyAsync(x => x.ItemCode == code, cancellationToken);
        if (exists)
        {
            return Result<PpeItemDto>.Failure($"PPE item code '{code}' already exists.");
        }
        var item = new PpeItem
        {
            ItemCode = code,
            Name = request.Name.Trim(),
            Category = request.Category.Trim(),
            Price = request.Price,
            LifetimeMonths = request.LifetimeMonths,
            UnitOfMeasure = request.UnitOfMeasure.Trim()
        };
        await _unitOfWork.Repository<PpeItem>().AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<PpeItemDto>.Success(new PpeItemDto(item.Id, item.ItemCode, item.Name, item.Category, item.Price, item.LifetimeMonths, item.UnitOfMeasure, 0));
    }

    public async Task<Result<PpeItemDto>> UpdateItemAsync(Guid id, UpdatePpeItemRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.Repository<PpeItem>().GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return Result<PpeItemDto>.Failure("PPE item not found.");
        }
        item.Name = request.Name.Trim();
        item.Category = request.Category.Trim();
        item.Price = request.Price;
        item.LifetimeMonths = request.LifetimeMonths;
        item.UnitOfMeasure = request.UnitOfMeasure.Trim();
        _unitOfWork.Repository<PpeItem>().Update(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var balance = await GetBalanceAsync(id, cancellationToken);
        return Result<PpeItemDto>.Success(new PpeItemDto(item.Id, item.ItemCode, item.Name, item.Category, item.Price, item.LifetimeMonths, item.UnitOfMeasure, balance));
    }

    public async Task<IReadOnlyCollection<PpeRequestDto>> ListRequestsAsync(CancellationToken cancellationToken = default)
    {
        var balances = await GetBalanceMapAsync(cancellationToken);
        var requests = await _unitOfWork.Repository<PpeRequest>().Query()
            .Include(x => x.Employee)
            .Include(x => x.PpeItem)
            .Include(x => x.Issue)
            .OrderByDescending(x => x.RequestedAtUtc)
            .ToListAsync(cancellationToken);
        return requests.Select(x => ToRequestDto(x, balances.GetValueOrDefault(x.PpeItemId))).ToList();
    }

    public async Task<Result<PpeRequestDto>> CreateRequestAsync(CreatePpeRequestRequest request, CancellationToken cancellationToken = default)
    {
        var employeeNumber = request.EmployeeNumber.Trim();
        var employee = await _unitOfWork.Repository<Employee>().Query()
            .FirstOrDefaultAsync(x => x.EmployeeNumber == employeeNumber && x.IsActive, cancellationToken);
        if (employee is null)
        {
            return Result<PpeRequestDto>.Failure($"No active employee found for employee number '{employeeNumber}'.");
        }
        var item = await _unitOfWork.Repository<PpeItem>().GetByIdAsync(request.PpeItemId, cancellationToken);
        if (item is null)
        {
            return Result<PpeRequestDto>.Failure("PPE item not found.");
        }

        var count = await _unitOfWork.Repository<PpeRequest>().Query().CountAsync(cancellationToken);
        var entity = new PpeRequest
        {
            RequestNumber = $"PPE-REQ-{count + 1:D5}",
            EmployeeId = employee.Id,
            PpeItemId = item.Id,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            RequesterSignature = request.RequesterSignature,
            Status = PpeRequestStatus.Requested,
            RequestedAtUtc = DateTime.UtcNow
        };
        await _unitOfWork.Repository<PpeRequest>().AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var balance = await GetBalanceAsync(item.Id, cancellationToken);
        var dto = new PpeRequestDto(
            entity.Id, entity.RequestNumber, employee.Id, employee.EmployeeNumber, employee.FullName, employee.Department,
            item.Id, item.Name, entity.Quantity, entity.UnitPrice, entity.Quantity * entity.UnitPrice,
            entity.RequesterSignature, entity.Status, balance, entity.RequestedAtUtc, false);
        return Result<PpeRequestDto>.Success(dto);
    }

    public async Task<Result<PpeIssueDto>> IssueAsync(Guid requestId, IssuePpeRequest request, CancellationToken cancellationToken = default)
    {
        var ppeRequest = await _unitOfWork.Repository<PpeRequest>().Query()
            .Include(x => x.Employee)
            .Include(x => x.PpeItem)
            .Include(x => x.Issue)
            .FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken);
        if (ppeRequest is null)
        {
            return Result<PpeIssueDto>.Failure("PPE request not found.");
        }
        if (ppeRequest.Issue is not null || ppeRequest.Status == PpeRequestStatus.Issued)
        {
            return Result<PpeIssueDto>.Failure("PPE request has already been issued.");
        }

        var balance = await GetBalanceAsync(ppeRequest.PpeItemId, cancellationToken);
        if (request.IssuedQuantity > balance)
        {
            return Result<PpeIssueDto>.Failure($"Insufficient stock. Available balance is {balance}.");
        }

        var issue = new PpeIssue
        {
            PpeRequestId = ppeRequest.Id,
            IssuedQuantity = request.IssuedQuantity,
            ReceiverSignature = request.ReceiverSignature,
            IssuedBy = _currentUserService.UserName,
            IssuedAtUtc = DateTime.UtcNow
        };
        await _unitOfWork.Repository<PpeIssue>().AddAsync(issue, cancellationToken);

        ppeRequest.Status = PpeRequestStatus.Issued;
        _unitOfWork.Repository<PpeRequest>().Update(ppeRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new PpeIssueDto(
            issue.Id, ppeRequest.RequestNumber, ppeRequest.Employee.EmployeeNumber, ppeRequest.Employee.FullName,
            ppeRequest.Employee.Department, ppeRequest.Employee.Designation, ppeRequest.PpeItem.Name,
            issue.IssuedQuantity, ppeRequest.RequesterSignature, issue.ReceiverSignature, issue.IssuedBy, issue.IssuedAtUtc);
        return Result<PpeIssueDto>.Success(dto);
    }

    public async Task<IReadOnlyCollection<PpeIssueDto>> ListIssuesAsync(CancellationToken cancellationToken = default) =>
        await _unitOfWork.Repository<PpeIssue>().Query()
            .Include(x => x.PpeRequest).ThenInclude(r => r.Employee)
            .Include(x => x.PpeRequest).ThenInclude(r => r.PpeItem)
            .OrderByDescending(x => x.IssuedAtUtc)
            .Select(x => new PpeIssueDto(
                x.Id,
                x.PpeRequest.RequestNumber,
                x.PpeRequest.Employee.EmployeeNumber,
                x.PpeRequest.Employee.FullName,
                x.PpeRequest.Employee.Department,
                x.PpeRequest.Employee.Designation,
                x.PpeRequest.PpeItem.Name,
                x.IssuedQuantity,
                x.PpeRequest.RequesterSignature,
                x.ReceiverSignature,
                x.IssuedBy,
                x.IssuedAtUtc))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<PpeInventoryDto>> ListInventoryAsync(CancellationToken cancellationToken = default)
    {
        var balances = await GetBalanceMapAsync(cancellationToken);
        var items = await _unitOfWork.Repository<PpeItem>().Query()
            .OrderBy(x => x.ItemCode)
            .Select(x => new { x.Id, x.ItemCode, x.Name })
            .ToListAsync(cancellationToken);
        return items.Select(x => new PpeInventoryDto(x.Id, x.ItemCode, x.Name, balances.GetValueOrDefault(x.Id))).ToList();
    }

    public async Task<int> GetBalanceAsync(Guid ppeItemId, CancellationToken cancellationToken = default)
    {
        var added = await _unitOfWork.Repository<PpeStockEntry>().Query()
            .Where(x => x.PpeItemId == ppeItemId)
            .SumAsync(x => (int?)x.AddedQuantity, cancellationToken) ?? 0;
        var issued = await _unitOfWork.Repository<PpeIssue>().Query()
            .Where(x => x.PpeRequest.PpeItemId == ppeItemId)
            .SumAsync(x => (int?)x.IssuedQuantity, cancellationToken) ?? 0;
        return added - issued;
    }

    public async Task<IReadOnlyCollection<PpeStockEntryDto>> ListStockEntriesAsync(CancellationToken cancellationToken = default) =>
        await _unitOfWork.Repository<PpeStockEntry>().Query()
            .Include(x => x.PpeItem)
            .OrderByDescending(x => x.EntryDateUtc)
            .Select(x => new PpeStockEntryDto(x.Id, x.PpeItemId, x.PpeItem.Name, x.AddedQuantity, 0, x.Remarks, x.EntryDateUtc))
            .ToListAsync(cancellationToken);

    public async Task<Result<PpeStockEntryDto>> AddStockAsync(AddPpeStockRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.Repository<PpeItem>().GetByIdAsync(request.PpeItemId, cancellationToken);
        if (item is null)
        {
            return Result<PpeStockEntryDto>.Failure("PPE item not found.");
        }
        var entry = new PpeStockEntry
        {
            PpeItemId = item.Id,
            AddedQuantity = request.AddedQuantity,
            Remarks = string.IsNullOrWhiteSpace(request.Remarks) ? null : request.Remarks.Trim(),
            EntryDateUtc = DateTime.UtcNow
        };
        await _unitOfWork.Repository<PpeStockEntry>().AddAsync(entry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var balance = await GetBalanceAsync(item.Id, cancellationToken);
        return Result<PpeStockEntryDto>.Success(new PpeStockEntryDto(entry.Id, item.Id, item.Name, entry.AddedQuantity, balance, entry.Remarks, entry.EntryDateUtc));
    }

    private async Task<Dictionary<Guid, int>> GetBalanceMapAsync(CancellationToken cancellationToken)
    {
        var added = await _unitOfWork.Repository<PpeStockEntry>().Query()
            .GroupBy(x => x.PpeItemId)
            .Select(g => new { PpeItemId = g.Key, Qty = g.Sum(e => e.AddedQuantity) })
            .ToListAsync(cancellationToken);
        var issued = await _unitOfWork.Repository<PpeIssue>().Query()
            .GroupBy(x => x.PpeRequest.PpeItemId)
            .Select(g => new { PpeItemId = g.Key, Qty = g.Sum(e => e.IssuedQuantity) })
            .ToListAsync(cancellationToken);
        var map = added.ToDictionary(x => x.PpeItemId, x => x.Qty);
        foreach (var row in issued)
        {
            map[row.PpeItemId] = map.GetValueOrDefault(row.PpeItemId) - row.Qty;
        }
        return map;
    }

    private static PpeRequestDto ToRequestDto(PpeRequest x, int balance) => new(
        x.Id,
        x.RequestNumber,
        x.EmployeeId,
        x.Employee.EmployeeNumber,
        x.Employee.FullName,
        x.Employee.Department,
        x.PpeItemId,
        x.PpeItem.Name,
        x.Quantity,
        x.UnitPrice,
        x.Quantity * x.UnitPrice,
        x.RequesterSignature,
        x.Status,
        balance,
        x.RequestedAtUtc,
        x.Issue != null || x.Status == PpeRequestStatus.Issued);
}
