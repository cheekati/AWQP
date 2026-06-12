using AWQP.Domain.Enums;

namespace AWQP.Application.DTOs;

public sealed record EmployeeDto(Guid Id, string EmployeeNumber, string FullName, string Department, string Designation);

public sealed record PpeItemDto(Guid Id, string ItemCode, string Name, string Category, decimal Price, int LifetimeMonths, string UnitOfMeasure, int ExistingQuantity);
public sealed record CreatePpeItemRequest(string ItemCode, string Name, string Category, decimal Price, int LifetimeMonths, string UnitOfMeasure);
public sealed record UpdatePpeItemRequest(string Name, string Category, decimal Price, int LifetimeMonths, string UnitOfMeasure);

public sealed record PpeRequestDto(
    Guid Id,
    string RequestNumber,
    Guid EmployeeId,
    string EmployeeNumber,
    string EmployeeName,
    string Department,
    Guid PpeItemId,
    string ItemName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string? RequesterSignature,
    PpeRequestStatus Status,
    int BalanceQuantity,
    DateTime RequestedAtUtc,
    bool IsIssued);

public sealed record CreatePpeRequestRequest(string EmployeeNumber, Guid PpeItemId, int Quantity, decimal UnitPrice, string RequesterSignature);
public sealed record IssuePpeRequest(int IssuedQuantity, string ReceiverSignature);

public sealed record PpeIssueDto(
    Guid Id,
    string RequestNumber,
    string EmployeeNumber,
    string EmployeeName,
    string Department,
    string Designation,
    string ItemName,
    int IssuedQuantity,
    string? RequesterSignature,
    string ReceiverSignature,
    string IssuedBy,
    DateTime IssuedAtUtc);

public sealed record PpeInventoryDto(Guid PpeItemId, string ItemCode, string ItemName, int ExistingQuantity);
public sealed record PpeStockEntryDto(Guid Id, Guid PpeItemId, string ItemName, int AddedQuantity, int BalanceAfter, string? Remarks, DateTime EntryDateUtc);
public sealed record AddPpeStockRequest(Guid PpeItemId, int AddedQuantity, string? Remarks);
