using System.Windows.Input;

namespace SweetTailor.App.Controls;

public partial class EmptyStateView : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(EmptyStateView), string.Empty, propertyChanged: (b, o, n) => ((EmptyStateView)b).TitleLabel.Text = (string)n);

    public static readonly BindableProperty DescriptionProperty =
        BindableProperty.Create(nameof(Description), typeof(string), typeof(EmptyStateView), string.Empty, propertyChanged: (b, o, n) => ((EmptyStateView)b).DescriptionLabel.Text = (string)n);

    public static readonly BindableProperty ButtonTextProperty =
        BindableProperty.Create(nameof(ButtonText), typeof(string), typeof(EmptyStateView), string.Empty, propertyChanged: (b, o, n) => ((EmptyStateView)b).ActionButton.Text = (string)n);

    public static readonly BindableProperty ShowActionProperty =
        BindableProperty.Create(nameof(ShowAction), typeof(bool), typeof(EmptyStateView), true,
            propertyChanged: (b, o, n) => ((EmptyStateView)b).ActionButton.IsVisible = (bool)n);

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(EmptyStateView), null, propertyChanged: (b, o, n) => ((EmptyStateView)b).ActionButton.Command = (ICommand)n);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public string ButtonText
    {
        get => (string)GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }

    public bool ShowAction
    {
        get => (bool)GetValue(ShowActionProperty);
        set => SetValue(ShowActionProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public event EventHandler? ActionClicked;

    public EmptyStateView()
    {
        InitializeComponent();
        ActionButton.Clicked += (sender, args) => ActionClicked?.Invoke(this, args);
    }
}
