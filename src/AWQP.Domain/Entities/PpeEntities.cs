using AWQP.Domain.Common;
using AWQP.Domain.Enums;

namespace AWQP.Domain.Entities;

public sealed class Employee : AuditableEntity
{
    public string EmployeeNumber { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Department { get; set; } = default!;
    public string Designation { get; set; } = default!;
    public bool IsActive { get; set; } = true;
}

public sealed class PpeItem : AuditableEntity
{
    public string ItemCode { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Category { get; set; } = default!;
    public decimal Price { get; set; }
    public int LifetimeMonths { get; set; }
    public string UnitOfMeasure { get; set; } = "EA";
    public ICollection<PpeStockEntry> StockEntries { get; set; } = new List<PpeStockEntry>();
    public ICollection<PpeRequest> Requests { get; set; } = new List<PpeRequest>();
}

public sealed class PpeRequest : AuditableEntity
{
    public string RequestNumber { get; set; } = default!;
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = default!;
    public Guid PpeItemId { get; set; }
    public PpeItem PpeItem { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? RequesterSignature { get; set; }
    public PpeRequestStatus Status { get; set; } = PpeRequestStatus.Requested;
    public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;
    public PpeIssue? Issue { get; set; }
}

public sealed class PpeIssue : AuditableEntity
{
    public Guid PpeRequestId { get; set; }
    public PpeRequest PpeRequest { get; set; } = default!;
    public int IssuedQuantity { get; set; }
    public string ReceiverSignature { get; set; } = default!;
    public string IssuedBy { get; set; } = default!;
    public DateTime IssuedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class PpeStockEntry : AuditableEntity
{
    public Guid PpeItemId { get; set; }
    public PpeItem PpeItem { get; set; } = default!;
    public int AddedQuantity { get; set; }
    public string? Remarks { get; set; }
    public DateTime EntryDateUtc { get; set; } = DateTime.UtcNow;
}
