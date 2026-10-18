using System.Windows.Input;

namespace SweetTailor.App.Controls;

public enum ButtonVariant
{
    Primary,
    Accent,
    Ghost,
    Danger
}

public partial class AppButton : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(AppButton), string.Empty, propertyChanged: (b, o, n) => ((AppButton)b).ButtonLabel.Text = (string)n);

    public static readonly BindableProperty VariantProperty =
        BindableProperty.Create(nameof(Variant), typeof(ButtonVariant), typeof(AppButton), ButtonVariant.Primary, propertyChanged: (b, o, n) => ((AppButton)b).UpdateVisualState());

    public static readonly BindableProperty IsBusyProperty =
        BindableProperty.Create(nameof(IsBusy), typeof(bool), typeof(AppButton), false, propertyChanged: (b, o, n) => ((AppButton)b).UpdateVisualState());

    public static readonly BindableProperty BusyTextProperty =
        BindableProperty.Create(nameof(BusyText), typeof(string), typeof(AppButton), "Enregistrement…", propertyChanged: (b, o, n) => ((AppButton)b).BusyLabel.Text = (string)n);

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(AppButton), null);

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(AppButton), null);

    public event EventHandler? Clicked;

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public ButtonVariant Variant
    {
        get => (ButtonVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    public string BusyText
    {
        get => (string)GetValue(BusyTextProperty);
        set => SetValue(BusyTextProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public AppButton()
    {
        InitializeComponent();
        BusyLabel.Text = BusyText;

        var tap = new TapGestureRecognizer();
        tap.Tapped += OnTapped;
        GestureRecognizers.Add(tap);

        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (s, e) =>
        {
            if (IsEnabled && !IsBusy) ButtonBorder.Opacity = 0.88;
        };
        pointer.PointerExited += (s, e) =>
        {
            if (IsEnabled && !IsBusy) ButtonBorder.Opacity = 1.0;
        };
        GestureRecognizers.Add(pointer);

        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(IsEnabled))
            {
                UpdateVisualState();
            }
        };

        UpdateVisualState();
    }

    private void OnTapped(object? sender, EventArgs e)
    {
        if (!IsEnabled || IsBusy) return;

        Clicked?.Invoke(this, EventArgs.Empty);
        if (Command?.CanExecute(CommandParameter) == true)
        {
            Command.Execute(CommandParameter);
        }
    }

    private void UpdateVisualState()
    {
        if (IsBusy)
        {
            ButtonLabel.IsVisible = false;
            BusyStack.IsVisible = true;
            ButtonBorder.BackgroundColor = Color.FromArgb("#26306B");
            ButtonBorder.Stroke = Color.FromArgb("#26306B");
            ButtonBorder.Opacity = 0.9;
            return;
        }

        ButtonLabel.IsVisible = true;
        BusyStack.IsVisible = false;

        if (!IsEnabled)
        {
            ButtonBorder.BackgroundColor = Color.FromArgb("#C9CCEA");
            ButtonBorder.Stroke = Color.FromArgb("#C9CCEA");
            ButtonLabel.TextColor = Colors.White;
            ButtonBorder.Opacity = 1.0;
            return;
        }

        ButtonBorder.Opacity = 1.0;
        switch (Variant)
        {
            case ButtonVariant.Primary:
                ButtonBorder.BackgroundColor = Color.FromArgb("#26306B");
                ButtonBorder.Stroke = Color.FromArgb("#26306B");
                ButtonLabel.TextColor = Colors.White;
                break;
            case ButtonVariant.Accent:
                ButtonBorder.BackgroundColor = Color.FromArgb("#E8607F");
                ButtonBorder.Stroke = Color.FromArgb("#E8607F");
                ButtonLabel.TextColor = Colors.White;
                break;
            case ButtonVariant.Ghost:
                ButtonBorder.BackgroundColor = Colors.White;
                ButtonBorder.Stroke = Color.FromArgb("#E8E9F3");
                ButtonLabel.TextColor = Color.FromArgb("#26306B");
                break;
            case ButtonVariant.Danger:
                ButtonBorder.BackgroundColor = Colors.White;
                ButtonBorder.Stroke = Color.FromArgb("#D93A4A");
                ButtonLabel.TextColor = Color.FromArgb("#D93A4A");
                break;
        }
    }
}
