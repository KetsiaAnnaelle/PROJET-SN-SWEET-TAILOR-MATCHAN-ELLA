using System.Windows.Input;
using SweetTailor.Core.Models;

namespace SweetTailor.App.Controls;

public partial class OrderRow : ContentView
{
    public static readonly BindableProperty OrderProperty =
        BindableProperty.Create(nameof(Order), typeof(Order), typeof(OrderRow), null, propertyChanged: (b, o, n) => ((OrderRow)b).UpdateOrder((Order?)n));

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(OrderRow), null);

    public event EventHandler? Tapped;

    public Order? Order
    {
        get => (Order?)GetValue(OrderProperty);
        set => SetValue(OrderProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public OrderRow()
    {
        InitializeComponent();

        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) =>
        {
            Tapped?.Invoke(this, EventArgs.Empty);
            if (Command?.CanExecute(Order) == true)
            {
                Command.Execute(Order);
            }
        };
        GestureRecognizers.Add(tap);
    }

    private void UpdateOrder(Order? order)
    {
        if (order == null) return;

        DayLabel.Text = order.DueDate.ToString("dd");
        MonthLabel.Text = order.DueDate.ToString("MMM").ToUpper();

        // Highlight hot dates (within 5 days)
        bool isHot = (order.DueDate - DateTime.Today).TotalDays <= 5;
        if (isHot)
        {
            DateBox.BackgroundColor = Color.FromArgb("#E8607F");
            DayLabel.TextColor = Colors.White;
            MonthLabel.TextColor = Colors.White;
        }
        else
        {
            DateBox.BackgroundColor = Color.FromArgb("#F6F5FA");
            DayLabel.TextColor = Color.FromArgb("#14173A");
            MonthLabel.TextColor = Color.FromArgb("#6B7094");
        }

        TitleLabel.Text = order.ModelName;
        ClientLabel.Text = order.ClientName;
        StatusBadge.Status = order.Status;
    }
}
