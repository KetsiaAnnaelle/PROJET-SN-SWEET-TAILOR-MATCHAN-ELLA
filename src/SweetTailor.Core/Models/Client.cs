namespace SweetTailor.Core.Models;

public class Client
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public ClientCategory Category { get; set; } = ClientCategory.Femme;
    public string NotesSummary { get; set; } = string.Empty;
    public string AvatarInitials { get; set; } = string.Empty;
    public string AvatarColorClass { get; set; } = "a1"; // a1 (rose), a2 (tape), a3 (mist), a4 (green)
    public string PhotoPath { get; set; } = string.Empty;
    public string CurrentOrderSummary { get; set; } = string.Empty;
    public OrderStatus CurrentOrderStatus { get; set; } = OrderStatus.Essayage;
    public int ActiveOrdersCount { get; set; }
    public int TotalOrdersCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<MeasurementItem> Measurements { get; set; } = new();
    public List<ClientNote> Notes { get; set; } = new();
    public List<Order> Orders { get; set; } = new();

    public static string ComputeInitials(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return "ST";
        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return parts[0].Length >= 2 ? parts[0][..2].ToUpperInvariant() : parts[0].ToUpperInvariant();
        return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[^1][0])}";
    }
}
