namespace SweetTailor.App.Controls;

public partial class AvatarInitials : ContentView
{
    public static readonly BindableProperty NameProperty =
        BindableProperty.Create(nameof(Name), typeof(string), typeof(AvatarInitials), string.Empty, propertyChanged: (b, o, n) => ((AvatarInitials)b).UpdateAvatar());

    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(double), typeof(AvatarInitials), 46.0, propertyChanged: (b, o, n) => ((AvatarInitials)b).UpdateAvatar());

    public static readonly BindableProperty ColorVariantProperty =
        BindableProperty.Create(nameof(ColorVariant), typeof(string), typeof(AvatarInitials), string.Empty, propertyChanged: (b, o, n) => ((AvatarInitials)b).UpdateAvatar());

    public static readonly BindableProperty ImagePathProperty =
        BindableProperty.Create(nameof(ImagePath), typeof(string), typeof(AvatarInitials), string.Empty, propertyChanged: (b, o, n) => ((AvatarInitials)b).UpdateAvatar());

    public string Name
    {
        get => (string)GetValue(NameProperty);
        set => SetValue(NameProperty, value);
    }

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public string ColorVariant
    {
        get => (string)GetValue(ColorVariantProperty);
        set => SetValue(ColorVariantProperty, value);
    }

    public string ImagePath
    {
        get => (string)GetValue(ImagePathProperty);
        set => SetValue(ImagePathProperty, value);
    }

    public AvatarInitials()
    {
        InitializeComponent();
        UpdateAvatar();
    }

    private void UpdateAvatar()
    {
        var size = Size > 0 ? Size : 46;
        AvatarBorder.WidthRequest = size;
        AvatarBorder.HeightRequest = size;
        AvatarBorder.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(size / 2) };

        // Calculate Initials from Name
        string initials = "ST";
        if (!string.IsNullOrWhiteSpace(Name))
        {
            var parts = Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                initials = $"{char.ToUpper(parts[0][0])}{char.ToUpper(parts[1][0])}";
            }
            else if (parts.Length == 1 && parts[0].Length >= 2)
            {
                initials = parts[0].Substring(0, 2).ToUpper();
            }
            else if (parts.Length == 1 && parts[0].Length == 1)
            {
                initials = parts[0].ToUpper();
            }
        }
        InitialsLabel.Text = initials;
        InitialsLabel.FontSize = size >= 70 ? 26 : (size >= 50 ? 18 : 15);
        PhotoImage.Source = string.IsNullOrWhiteSpace(ImagePath) ? null : ImagePath;
        PhotoImage.IsVisible = !string.IsNullOrWhiteSpace(ImagePath);
        InitialsLabel.IsVisible = !PhotoImage.IsVisible;

        // Determine color variant
        string variant = ColorVariant;
        if (string.IsNullOrWhiteSpace(variant) && !string.IsNullOrWhiteSpace(Name))
        {
            int hash = Math.Abs(Name.GetHashCode()) % 4;
            variant = hash switch
            {
                0 => "a1",
                1 => "a2",
                2 => "a3",
                _ => "a4"
            };
        }

        var (bg, fg) = variant switch
        {
            "a1" => ("#FCE4EA", "#B8325A"),
            "a2" => ("#FDF3D6", "#8A6A10"),
            "a3" => ("#E4E5F2", "#26306B"),
            "a4" => ("#DDF3E8", "#1E7A50"),
            _ => ("#FCE4EA", "#B8325A")
        };

        AvatarBorder.BackgroundColor = Color.FromArgb(bg);
        InitialsLabel.TextColor = Color.FromArgb(fg);
    }
}
