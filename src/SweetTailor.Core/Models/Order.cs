using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace SweetTailor.Core.Models;

public partial class Order : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    private OrderStatus _status = OrderStatus.Recue;

    public int Id { get; set; }
    public string OrderNumber { get; set; } = "#0001";
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientPhone { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string Fabric { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public decimal AdvancePayment { get; set; }
    public decimal RemainingPayment => TotalPrice - AdvancePayment;
    public decimal BalancePayment => RemainingPayment;
    public double PaymentProgressRatio => TotalPrice > 0 ? (double)(AdvancePayment / TotalPrice) : 0;
    public double AdvancePaymentRatio => PaymentProgressRatio;

    public DateTime DueDate { get; set; }
    public string DueDateLabel { get; set; } = string.Empty;
    public bool IsHotDueDate { get; set; }
    public DateTime? DeliveredAt { get; set; }

    public string SvgAsset { get; set; } = "dress";
    public string ImagePath { get; set; } = string.Empty;
    [JsonIgnore]
    public string? RemoteImagePath { get; set; }
    public string GradientClass { get; set; } = "g-rose";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<OrderStep> Steps { get; set; } = new();

    public string StatusText => Status switch
    {
        OrderStatus.Recue => "Reçue",
        OrderStatus.Couture => "Couture",
        OrderStatus.Essayage => "Essayage",
        OrderStatus.Prete => "Prête",
        OrderStatus.Livree => "Livrée",
        _ => Status.ToString()
    };

    public string FormattedPrice => $"{TotalPrice:N0} FCFA".Replace(",", " ");
    public string FormattedAdvance => $"{AdvancePayment:N0} FCFA".Replace(",", " ");
    public string FormattedRemaining => $"{RemainingPayment:N0} FCFA".Replace(",", " ");
    public string FormattedPaymentRatio => $"{AdvancePayment:N0} / {TotalPrice:N0} FCFA".Replace(",", " ");
}
