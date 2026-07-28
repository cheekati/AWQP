using ChangeManagement.Domain.Common;

namespace ChangeManagement.Domain.Entities;

public class Product : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<EngineeringRequest> EngineeringRequests { get; set; } = new List<EngineeringRequest>();
}
