using SweetTailor.Core.ViewModels;

namespace SweetTailor.App.Controls;

public partial class ToastBanner : ContentView
{
    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(ToastBanner), string.Empty, propertyChanged: (b, o, n) => ((ToastBanner)b).ToastText.Text = (string)n);

    public static readonly BindableProperty TypeProperty =
        BindableProperty.Create(nameof(Type), typeof(ToastType), typeof(ToastBanner), ToastType.Info, propertyChanged: (b, o, n) => ((ToastBanner)b).UpdateType((ToastType)n));

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public ToastType Type
    {
        get => (ToastType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public ToastBanner()
    {
        InitializeComponent();
        UpdateType(Type);
    }

    private void UpdateType(ToastType type)
    {
        var iconPathConv = new Microsoft.Maui.Controls.Shapes.PathGeometryConverter();

        switch (type)
        {
            case ToastType.Success:
                ToastBorder.BackgroundColor = Color.FromArgb("#DDF3E8");
                ToastText.TextColor = Color.FromArgb("#1E7A50");
                ToastIcon.Fill = Color.FromArgb("#1E7A50");
                ToastIcon.Data = (Microsoft.Maui.Controls.Shapes.Geometry)iconPathConv.ConvertFromInvariantString("m5 12.5 4.5 4.5L19 7.5")!;
                break;
            case ToastType.Error:
                ToastBorder.BackgroundColor = Color.FromArgb("#FDE8EA");
                ToastText.TextColor = Color.FromArgb("#B02A3A");
                ToastIcon.Fill = Color.FromArgb("#B02A3A");
                ToastIcon.Data = (Microsoft.Maui.Controls.Shapes.Geometry)iconPathConv.ConvertFromInvariantString("M12 3a9 9 0 1 0 0 18 9 9 0 0 0 0-18zm0 4.5v5.5m0 3.3v.1")!;
                break;
            case ToastType.Info:
            default:
                ToastBorder.BackgroundColor = Color.FromArgb("#E4E5F2");
                ToastText.TextColor = Color.FromArgb("#26306B");
                ToastIcon.Fill = Color.FromArgb("#26306B");
                ToastIcon.Data = (Microsoft.Maui.Controls.Shapes.Geometry)iconPathConv.ConvertFromInvariantString("M12 3a9 9 0 1 0 0 18 9 9 0 0 0 0-18zm0 8v5.5m0-8.9v.1")!;
                break;
        }
    }
}
