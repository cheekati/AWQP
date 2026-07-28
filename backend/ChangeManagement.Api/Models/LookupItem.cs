namespace ChangeManagement.Api.Models;

public class LookupItem
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
