namespace ChangeManagement.Api.Models;

public enum Division
{
    PR = 1,
    ENG = 2,
    QA = 3,
    IND = 4
}

public enum UserRole
{
    Requester = 1,
    Safety = 2,
    DeptHead = 3,
    QA = 4,
    COO = 5,
    Admin = 6
}

public enum ErStatus
{
    Draft = 0,
    InProgress = 1,
    Approved = 2,
    Rejected = 3,
    Closed = 4,
    ResubmitRequested = 5
}

public enum VerificationLevel
{
    Safety = 1,
    DeptHead = 2,
    QA = 3,
    COO = 4
}

public enum VerificationDecision
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Resubmit = 3
}

public enum ErOutcome
{
    None = 0,
    Pass = 1,
    Fail = 2,
    Resubmit = 3
}

public enum TestLotType
{
    RunAsNormal = 1,
    SpecialProcess = 2,
    Others = 3
}

public enum ReasonForChange
{
    CostDown = 1,
    AlternativeSourcing = 2,
    Others = 3
}
