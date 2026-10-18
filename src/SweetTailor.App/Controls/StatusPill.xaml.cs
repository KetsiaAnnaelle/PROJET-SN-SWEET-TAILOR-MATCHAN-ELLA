using SweetTailor.Core.Models;

namespace SweetTailor.App.Controls;

public partial class StatusPill : ContentView
{
    public static readonly BindableProperty StatusProperty =
        BindableProperty.Create(nameof(Status), typeof(OrderStatus), typeof(StatusPill), OrderStatus.Recue, propertyChanged: (b, o, n) => ((StatusPill)b).UpdateStatus((OrderStatus)n));

    public OrderStatus Status
    {
        get => (OrderStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public StatusPill()
    {
        InitializeComponent();
        UpdateStatus(Status);
    }

    private void UpdateStatus(OrderStatus status)
    {
        var (text, bg, fg) = status switch
        {
            OrderStatus.Recue => ("Reçue", "#E4E5F2", "#26306B"),
            OrderStatus.Couture => ("Couture", "#FDF3D6", "#8A6A10"),
            OrderStatus.Essayage => ("Essayage", "#FCE4EA", "#B8325A"),
            OrderStatus.Prete => ("Prête", "#DDF3E8", "#1E7A50"),
            OrderStatus.Livree => ("Livrée", "#ECECF2", "#6B7094"),
            _ => ("Reçue", "#E4E5F2", "#26306B")
        };

        PillBorder.BackgroundColor = Color.FromArgb(bg);
        PillLabel.Text = text;
        PillLabel.TextColor = Color.FromArgb(fg);
    }
}
