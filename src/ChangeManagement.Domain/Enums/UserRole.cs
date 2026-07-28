namespace ChangeManagement.Domain.Enums;

public static class AppRoles
{
    public const string Requester = "Requester";
    public const string Safety = "Safety";
    public const string DepartmentHead = "DepartmentHead";
    public const string QA = "QA";
    public const string COO = "COO";
    public const string Administrator = "Administrator";

    public static readonly string[] All =
    [
        Requester,
        Safety,
        DepartmentHead,
        QA,
        COO,
        Administrator
    ];
}
