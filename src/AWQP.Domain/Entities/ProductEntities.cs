using AWQP.Domain.Common;
using AWQP.Domain.Enums;

namespace AWQP.Domain.Entities;

public sealed class ProductCategory : AuditableEntity
{
    public string Name { get; set; } = default!;
    public IndustrySegment IndustrySegment { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public sealed class Product : AuditableEntity
{
    public string ProductCode { get; set; } = default!;
    public string Name { get; set; } = default!;
    public Guid ProductCategoryId { get; set; }
    public ProductCategory ProductCategory { get; set; } = default!;
    public string MaterialGrade { get; set; } = "High Purity Quartz";
    public decimal OuterDiameterMm { get; set; }
    public decimal InnerDiameterMm { get; set; }
    public decimal LengthMm { get; set; }
    public decimal WallThicknessMm { get; set; }
    public ICollection<ProductRevision> Revisions { get; set; } = new List<ProductRevision>();
}

public sealed class ProductRevision : AuditableEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public string RevisionNumber { get; set; } = "A";
    public string EngineeringDrawingNumber { get; set; } = default!;
    public string DrawingStorageUri { get; set; } = default!;
    public DateTime EffectiveFromUtc { get; set; } = DateTime.UtcNow;
    public DateTime? EffectiveToUtc { get; set; }
    public string ChangeSummary { get; set; } = default!;
    public ICollection<EngineeringChangeRequest> EngineeringChangeRequests { get; set; } = new List<EngineeringChangeRequest>();
}

public sealed class EngineeringChangeRequest : AuditableEntity
{
    public Guid ProductRevisionId { get; set; }
    public ProductRevision ProductRevision { get; set; } = default!;
    public string RequestNumber { get; set; } = default!;
    public string Reason { get; set; } = default!;
    public SalesStatus ApprovalStatus { get; set; } = SalesStatus.Submitted;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
}
