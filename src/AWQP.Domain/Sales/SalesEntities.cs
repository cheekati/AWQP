using AWQP.Domain.Common;
using AWQP.Domain.Customers;
using AWQP.Domain.Products;

namespace AWQP.Domain.Sales;

public sealed class RequestForQuote : BaseEntity
{
    public string RfqNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public decimal Quantity { get; set; }
    public DateOnly RequiredDate { get; set; }
    public SalesDocumentStatus Status { get; set; } = SalesDocumentStatus.Draft;
    public string TechnicalRequirements { get; set; } = string.Empty;
}

public sealed class Quotation : BaseEntity
{
    public string QuotationNumber { get; set; } = string.Empty;
    public Guid RequestForQuoteId { get; set; }
    public RequestForQuote? RequestForQuote { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal ToolingCharge { get; set; }
    public decimal LeadTimeDays { get; set; }
    public DateOnly ValidUntil { get; set; }
    public SalesDocumentStatus Status { get; set; } = SalesDocumentStatus.Submitted;
}

public sealed class SalesOrder : BaseEntity
{
    public string SalesOrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public Guid? QuotationId { get; set; }
    public Quotation? Quotation { get; set; }
    public DateOnly OrderDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public DateOnly RequestedShipDate { get; set; }
    public SalesDocumentStatus Status { get; set; } = SalesDocumentStatus.Approved;
    public ICollection<SalesOrderLine> Lines { get; set; } = [];
    public ICollection<SalesApproval> Approvals { get; set; } = [];
}

public sealed class SalesOrderLine : BaseEntity
{
    public Guid SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string ProductRevision { get; set; } = "A";
}

public sealed class SalesApproval : BaseEntity
{
    public Guid SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }
    public string ApprovalLevel { get; set; } = string.Empty;
    public string ApproverUserId { get; set; } = string.Empty;
    public SalesDocumentStatus Status { get; set; } = SalesDocumentStatus.Submitted;
    public DateTimeOffset? ApprovedAt { get; set; }
    public string? Remarks { get; set; }
}
