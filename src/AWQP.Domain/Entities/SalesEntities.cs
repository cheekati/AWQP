using AWQP.Domain.Common;
using AWQP.Domain.Enums;

namespace AWQP.Domain.Entities;

public sealed class RequestForQuotation : AuditableEntity
{
    public string RfqNumber { get; set; } = default!;
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;
    public DateTime RequestedDateUtc { get; set; } = DateTime.UtcNow;
    public SalesStatus Status { get; set; } = SalesStatus.Submitted;
    public ICollection<RequestForQuotationLine> Lines { get; set; } = new List<RequestForQuotationLine>();
}

public sealed class RequestForQuotationLine : AuditableEntity
{
    public Guid RequestForQuotationId { get; set; }
    public RequestForQuotation RequestForQuotation { get; set; } = default!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public int Quantity { get; set; }
    public DateTime RequiredDateUtc { get; set; }
    public string? Notes { get; set; }
}

public sealed class Quotation : AuditableEntity
{
    public string QuotationNumber { get; set; } = default!;
    public Guid RequestForQuotationId { get; set; }
    public RequestForQuotation RequestForQuotation { get; set; } = default!;
    public SalesStatus ApprovalStatus { get; set; } = SalesStatus.Draft;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public ICollection<QuotationLine> Lines { get; set; } = new List<QuotationLine>();
}

public sealed class QuotationLine : AuditableEntity
{
    public Guid QuotationId { get; set; }
    public Quotation Quotation { get; set; } = default!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public sealed class SalesOrder : AuditableEntity
{
    public string SalesOrderNumber { get; set; } = default!;
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;
    public Guid? QuotationId { get; set; }
    public Quotation? Quotation { get; set; }
    public SalesStatus Status { get; set; } = SalesStatus.Approved;
    public DateTime OrderDateUtc { get; set; } = DateTime.UtcNow;
    public ICollection<SalesOrderLine> Lines { get; set; } = new List<SalesOrderLine>();
}

public sealed class SalesOrderLine : AuditableEntity
{
    public Guid SalesOrderId { get; set; }
    public SalesOrder SalesOrder { get; set; } = default!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public int Quantity { get; set; }
    public DateTime PromiseDateUtc { get; set; }
}
