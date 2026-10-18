using System.Windows.Input;
using SweetTailor.Core.Models;

namespace SweetTailor.App.Controls;

public partial class ClientRow : ContentView
{
    public static readonly BindableProperty ClientProperty =
        BindableProperty.Create(nameof(Client), typeof(Client), typeof(ClientRow), null, propertyChanged: (b, o, n) => ((ClientRow)b).UpdateClient((Client?)n));

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ClientRow), null);

    public event EventHandler? Tapped;

    public Client? Client
    {
        get => (Client?)GetValue(ClientProperty);
        set => SetValue(ClientProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public ClientRow()
    {
        InitializeComponent();

        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) =>
        {
            Tapped?.Invoke(this, EventArgs.Empty);
            if (Command?.CanExecute(Client) == true)
            {
                Command.Execute(Client);
            }
        };
        GestureRecognizers.Add(tap);
    }

    private void UpdateClient(Client? client)
    {
        if (client == null) return;

        Avatar.Name = client.FullName;
        Avatar.ColorVariant = client.AvatarColorClass;
        Avatar.ImagePath = client.PhotoPath;
        NameLabel.Text = client.FullName;
        OrderDescLabel.Text = client.CurrentOrderSummary;
        StatusBadge.Status = client.CurrentOrderStatus;
    }
}
