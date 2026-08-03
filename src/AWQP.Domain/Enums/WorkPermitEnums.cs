namespace AWQP.Domain.Enums;

public enum WorkPermitStatus
{
    Request = 1,
    Approved,
    InProgress,
    OnHold,
    Completed,
    Verified,
    Rejected
}

public enum WorkPermitType
{
    ColdWork = 1,
    HotWork
}
