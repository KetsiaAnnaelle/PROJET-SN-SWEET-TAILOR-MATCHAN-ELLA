namespace SweetTailor.Core.Models;

public sealed class WorkshopProfile
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string WorkshopName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

public sealed class WorkshopStats
{
    public int ClientCount { get; set; }
    public int OrdersThisMonth { get; set; }
    public int OnTimeRate { get; set; }
}
