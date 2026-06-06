using AWQP.Domain.Common;

namespace AWQP.Domain.Products;

public sealed class ProductCategory : NamedEntity
{
    public IndustrySegment Segment { get; set; }
    public ICollection<Product> Products { get; set; } = [];
}

public sealed class MaterialGrade : NamedEntity
{
    public decimal MinimumPurityPercent { get; set; }
    public string SupplierSpecification { get; set; } = string.Empty;
}

public sealed class Product : NamedEntity
{
    public Guid ProductCategoryId { get; set; }
    public ProductCategory? ProductCategory { get; set; }
    public Guid MaterialGradeId { get; set; }
    public MaterialGrade? MaterialGrade { get; set; }
    public string ProductFamily { get; set; } = string.Empty;
    public string DefaultUom { get; set; } = "EA";
    public ICollection<ProductSpecification> Specifications { get; set; } = [];
    public ICollection<ProductDimension> Dimensions { get; set; } = [];
    public ICollection<ProductRevision> Revisions { get; set; } = [];
    public ICollection<EngineeringDrawing> Drawings { get; set; } = [];
}

public sealed class ProductSpecification : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public string ParameterName { get; set; } = string.Empty;
    public string NominalValue { get; set; } = string.Empty;
    public string? LowerTolerance { get; set; }
    public string? UpperTolerance { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public bool IsCriticalToQuality { get; set; }
}

public sealed class ProductDimension : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public decimal LengthMm { get; set; }
    public decimal WidthMm { get; set; }
    public decimal HeightMm { get; set; }
    public decimal InnerDiameterMm { get; set; }
    public decimal OuterDiameterMm { get; set; }
    public decimal WallThicknessMm { get; set; }
    public string ToleranceClass { get; set; } = "Standard";
}

public sealed class ProductRevision : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public string RevisionNumber { get; set; } = "A";
    public string ChangeSummary { get; set; } = string.Empty;
    public DateTimeOffset EffectiveDate { get; set; } = DateTimeOffset.UtcNow;
    public bool IsCurrent { get; set; }
}

public sealed class EngineeringDrawing : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public string DrawingNumber { get; set; } = string.Empty;
    public string RevisionNumber { get; set; } = "A";
    public string FileName { get; set; } = string.Empty;
    public string StorageUri { get; set; } = string.Empty;
    public bool IsReleased { get; set; }
}

public sealed class EngineeringChangeRequest : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string ImpactAssessment { get; set; } = string.Empty;
    public SalesDocumentStatus Status { get; set; } = SalesDocumentStatus.Submitted;
}
