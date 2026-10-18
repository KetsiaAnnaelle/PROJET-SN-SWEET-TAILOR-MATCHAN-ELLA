namespace SweetTailor.App.Controls;

public partial class TapeControl : ContentView
{
    public static readonly BindableProperty IsSmallProperty =
        BindableProperty.Create(nameof(IsSmall), typeof(bool), typeof(TapeControl), false, propertyChanged: OnIsSmallChanged);

    public bool IsSmall
    {
        get => (bool)GetValue(IsSmallProperty);
        set => SetValue(IsSmallProperty, value);
    }

    public string CornerRadiusShape => IsSmall ? "RoundRectangle 4" : "RoundRectangle 0";

    public TapeControl()
    {
        InitializeComponent();
        HeightRequest = 22;
    }

    private static void OnIsSmallChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TapeControl control)
        {
            control.HeightRequest = (bool)newValue ? 16 : 22;
            control.OnPropertyChanged(nameof(CornerRadiusShape));
        }
    }
}
