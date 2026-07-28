using ChangeManagement.Domain.Common;

namespace ChangeManagement.Domain.Entities;

public class Department : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<EngineeringRequest> EngineeringRequests { get; set; } = new List<EngineeringRequest>();
}
