namespace ChangeManagement.Api.Models;

public class ErAttachment
{
    public int Id { get; set; }
    public int EngineeringRequestId { get; set; }
    public EngineeringRequest? EngineeringRequest { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public bool IsImage { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public int UploadedByUserId { get; set; }
}

public class ErComment
{
    public int Id { get; set; }
    public int EngineeringRequestId { get; set; }
    public EngineeringRequest? EngineeringRequest { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class EmailLog
{
    public int Id { get; set; }
    public int EngineeringRequestId { get; set; }
    public EngineeringRequest? EngineeringRequest { get; set; }

    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool Success { get; set; } = true;
}

public class ErSequence
{
    public int Id { get; set; }
    public string YearMonth { get; set; } = string.Empty;
    public int LastSequence { get; set; }
}
