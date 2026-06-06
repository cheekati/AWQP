using AWQP.Domain.Common;

namespace AWQP.Domain.Customers;

public sealed class Customer : NamedEntity
{
    public string LegalName { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string? Website { get; set; }
    public string PaymentTerms { get; set; } = "Net 30";
    public string CreditStatus { get; set; } = "Approved";
    public ICollection<CustomerContact> Contacts { get; set; } = [];
    public ICollection<CustomerAddress> Addresses { get; set; } = [];
    public ICollection<CustomerIndustry> Industries { get; set; } = [];
    public ICollection<CustomerDocument> Documents { get; set; } = [];
}

public sealed class CustomerContact : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string Name { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

public sealed class CustomerAddress : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string AddressType { get; set; } = "Billing";
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string StateProvince { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public sealed class CustomerIndustry : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public IndustrySegment Segment { get; set; }
    public string Application { get; set; } = string.Empty;
}

public sealed class CustomerDocument : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StorageUri { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; } = DocumentType.Contract;
    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
}
