using ChangeManagement.Domain.Common;

namespace ChangeManagement.Domain.Entities;

public class EngineeringRequestAttachment : BaseEntity
{
    public Guid EngineeringRequestId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public bool IsImage { get; set; }

    public EngineeringRequest EngineeringRequest { get; set; } = null!;
}
