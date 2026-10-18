using System.Windows.Input;

namespace SweetTailor.App.Controls;

public partial class ErrorStateView : ContentView
{
    public static readonly BindableProperty ErrorCodeProperty =
        BindableProperty.Create(nameof(ErrorCode), typeof(string), typeof(ErrorStateView), "NET-01", propertyChanged: (b, o, n) => ((ErrorStateView)b).ErrorCodeLabel.Text = $"Code : {n}");

    public static readonly BindableProperty BannerMessageProperty =
        BindableProperty.Create(nameof(BannerMessage), typeof(string), typeof(ErrorStateView), "Impossible de charger les données.", propertyChanged: (b, o, n) => ((ErrorStateView)b).BannerMessageLabel.Text = (string)n);

    public static readonly BindableProperty RetryCommandProperty =
        BindableProperty.Create(nameof(RetryCommand), typeof(ICommand), typeof(ErrorStateView), null, propertyChanged: (b, o, n) => ((ErrorStateView)b).RetryButton.Command = (ICommand)n);

    public static readonly BindableProperty HomeCommandProperty =
        BindableProperty.Create(nameof(HomeCommand), typeof(ICommand), typeof(ErrorStateView), null, propertyChanged: (b, o, n) => ((ErrorStateView)b).HomeButton.Command = (ICommand)n);

    public string BannerMessage
    {
        get => (string)GetValue(BannerMessageProperty);
        set => SetValue(BannerMessageProperty, value);
    }

    public string ErrorCode
    {
        get => (string)GetValue(ErrorCodeProperty);
        set => SetValue(ErrorCodeProperty, value);
    }

    public ICommand? RetryCommand
    {
        get => (ICommand?)GetValue(RetryCommandProperty);
        set => SetValue(RetryCommandProperty, value);
    }

    public ICommand? HomeCommand
    {
        get => (ICommand?)GetValue(HomeCommandProperty);
        set => SetValue(HomeCommandProperty, value);
    }

    public ErrorStateView()
    {
        InitializeComponent();
        HomeButton.Clicked += async (s, e) =>
        {
            if (HomeCommand != null && HomeCommand.CanExecute(null))
            {
                HomeCommand.Execute(null);
            }
            else
            {
                await Shell.Current.GoToAsync("//ClientsPage");
            }
        };
    }
}
