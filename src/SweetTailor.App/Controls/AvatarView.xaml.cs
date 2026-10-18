namespace SweetTailor.App.Controls;

public partial class AvatarView : ContentView
{
    public static readonly BindableProperty InitialsProperty =
        BindableProperty.Create(nameof(Initials), typeof(string), typeof(AvatarView), "ST", propertyChanged: OnPropsChanged);

    public static readonly BindableProperty ColorClassProperty =
        BindableProperty.Create(nameof(ColorClass), typeof(string), typeof(AvatarView), "a1", propertyChanged: OnPropsChanged);

    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(double), typeof(AvatarView), 46.0, propertyChanged: OnSizeChanged);

    public string Initials
    {
        get => (string)GetValue(InitialsProperty);
        set => SetValue(InitialsProperty, value);
    }

    public string ColorClass
    {
        get => (string)GetValue(ColorClassProperty);
        set => SetValue(ColorClassProperty, value);
    }

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public AvatarView()
    {
        InitializeComponent();
        UpdateView();
    }

    private static void OnPropsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AvatarView view)
        {
            view.UpdateView();
        }
    }

    private static void OnSizeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AvatarView view && newValue is double size)
        {
            view.AvatarBorder.WidthRequest = size;
            view.AvatarBorder.HeightRequest = size;
            view.InitialsLabel.FontSize = size >= 70 ? 28 : (size >= 50 ? 18 : 15);
        }
    }

    private void UpdateView()
    {
        InitialsLabel.Text = Initials;
        InitialsLabel.FontSize = Size >= 70 ? 28 : (Size >= 50 ? 18 : 15);

        switch (ColorClass?.ToLowerInvariant())
        {
            case "a1":
                AvatarBorder.BackgroundColor = Color.FromArgb("#FCE4EA");
                InitialsLabel.TextColor = Color.FromArgb("#B8325A");
                break;
            case "a2":
                AvatarBorder.BackgroundColor = Color.FromArgb("#FDF3D6");
                InitialsLabel.TextColor = Color.FromArgb("#8A6A10");
                break;
            case "a3":
                AvatarBorder.BackgroundColor = Color.FromArgb("#E4E5F2");
                InitialsLabel.TextColor = Color.FromArgb("#26306B");
                break;
            case "a4":
                AvatarBorder.BackgroundColor = Color.FromArgb("#DDF3E8");
                InitialsLabel.TextColor = Color.FromArgb("#1E7A50");
                break;
            default:
                AvatarBorder.BackgroundColor = Color.FromArgb("#FCE4EA");
                InitialsLabel.TextColor = Color.FromArgb("#B8325A");
                break;
        }
    }
}
