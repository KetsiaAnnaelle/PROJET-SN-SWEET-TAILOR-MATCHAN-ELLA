using SweetTailor.Core.Models;

namespace SweetTailor.App.Controls;

public partial class StatusBadge : ContentView
{
    public static readonly BindableProperty StatusProperty =
        BindableProperty.Create(nameof(Status), typeof(OrderStatus), typeof(StatusBadge), OrderStatus.Recue, propertyChanged: OnStatusChanged);

    public OrderStatus Status
    {
        get => (OrderStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public StatusBadge()
    {
        InitializeComponent();
        UpdateVisuals(Status);
    }

    private static void OnStatusChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StatusBadge badge && newValue is OrderStatus status)
        {
            badge.UpdateVisuals(status);
        }
    }

    private void UpdateVisuals(OrderStatus status)
    {
        switch (status)
        {
            case OrderStatus.Recue:
                BadgeBorder.BackgroundColor = Color.FromArgb("#E4E5F2");
                BadgeLabel.TextColor = Color.FromArgb("#26306B");
                BadgeLabel.Text = "Reçue";
                break;
            case OrderStatus.Couture:
                BadgeBorder.BackgroundColor = Color.FromArgb("#FDF3D6");
                BadgeLabel.TextColor = Color.FromArgb("#8A6A10");
                BadgeLabel.Text = "Couture";
                break;
            case OrderStatus.Essayage:
                BadgeBorder.BackgroundColor = Color.FromArgb("#FCE4EA");
                BadgeLabel.TextColor = Color.FromArgb("#B8325A");
                BadgeLabel.Text = "Essayage";
                break;
            case OrderStatus.Prete:
                BadgeBorder.BackgroundColor = Color.FromArgb("#DDF3E8");
                BadgeLabel.TextColor = Color.FromArgb("#1E7A50");
                BadgeLabel.Text = "Prête";
                break;
            case OrderStatus.Livree:
                BadgeBorder.BackgroundColor = Color.FromArgb("#ECECF2");
                BadgeLabel.TextColor = Color.FromArgb("#6B7094");
                BadgeLabel.Text = "Livrée";
                break;
        }
    }
}
