using AWQP.Domain.Common;
using AWQP.Domain.Enums;

namespace AWQP.Domain.Entities;

public sealed class Customer : AuditableEntity
{
    public string CustomerCode { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? TaxNumber { get; set; }
    public string? Website { get; set; }
    public ICollection<CustomerContact> Contacts { get; set; } = new List<CustomerContact>();
    public ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();
    public ICollection<CustomerIndustry> Industries { get; set; } = new List<CustomerIndustry>();
    public ICollection<CustomerDocument> Documents { get; set; } = new List<CustomerDocument>();
}

public sealed class CustomerContact : AuditableEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? Phone { get; set; }
    public string? JobTitle { get; set; }
    public bool IsPrimary { get; set; }
}

public sealed class CustomerAddress : AuditableEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;
    public string AddressType { get; set; } = "Billing";
    public string Line1 { get; set; } = default!;
    public string? Line2 { get; set; }
    public string City { get; set; } = default!;
    public string State { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string PostalCode { get; set; } = default!;
}

public sealed class CustomerIndustry : AuditableEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;
    public IndustrySegment IndustrySegment { get; set; }
}

public sealed class CustomerDocument : AuditableEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;
    public string DocumentType { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public string StorageUri { get; set; } = default!;
}
