using ChangeManagement.Domain.Enums;

namespace ChangeManagement.Application.DTOs.Dashboard;

public class DashboardStatsDto
{
    public int TotalRequests { get; set; }
    public int DraftCount { get; set; }
    public int InProgressCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public int AwaitingMyAction { get; set; }
    public int ClosedCount { get; set; }
    public List<StatusCountDto> StatusBreakdown { get; set; } = new();
    public List<MonthlyCountDto> MonthlyTrend { get; set; } = new();
    public List<RecentRequestDto> RecentRequests { get; set; } = new();
}

public class StatusCountDto
{
    public ErStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class MonthlyCountDto
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class RecentRequestDto
{
    public Guid Id { get; set; }
    public string ErNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public ErStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}
