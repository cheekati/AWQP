using AWQP.Domain.Common;

namespace AWQP.Domain.CleanRoom;

public sealed class CleanRoomArea : NamedEntity
{
    public string IsoClass { get; set; } = "Class 1000";
    public decimal TemperatureLowerC { get; set; } = 20;
    public decimal TemperatureUpperC { get; set; } = 24;
    public decimal HumidityLowerPercent { get; set; } = 40;
    public decimal HumidityUpperPercent { get; set; } = 55;
    public int ParticleLimitPerCubicFoot { get; set; } = 1000;
}

public sealed class CleanRoomEnvironmentalReading : BaseEntity
{
    public Guid CleanRoomAreaId { get; set; }
    public CleanRoomArea? CleanRoomArea { get; set; }
    public DateTimeOffset RecordedAt { get; set; } = DateTimeOffset.UtcNow;
    public decimal TemperatureC { get; set; }
    public decimal HumidityPercent { get; set; }
    public int ParticleCountPerCubicFoot { get; set; }
    public string SensorId { get; set; } = string.Empty;
    public bool IsCompliant { get; set; }
}

public sealed class CleanRoomAccessLog : BaseEntity
{
    public Guid CleanRoomAreaId { get; set; }
    public CleanRoomArea? CleanRoomArea { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTimeOffset EntryAt { get; set; }
    public DateTimeOffset? ExitAt { get; set; }
    public bool GowningChecklistPassed { get; set; }
    public bool AccessApproved { get; set; }
    public string? DenialReason { get; set; }
}

public sealed class EnvironmentalComplianceRecord : BaseEntity
{
    public Guid CleanRoomAreaId { get; set; }
    public CleanRoomArea? CleanRoomArea { get; set; }
    public DateOnly ComplianceDate { get; set; }
    public bool TemperatureCompliant { get; set; }
    public bool HumidityCompliant { get; set; }
    public bool ParticleCompliant { get; set; }
    public string ReviewedByUserId { get; set; } = string.Empty;
    public string CorrectiveAction { get; set; } = string.Empty;
}
