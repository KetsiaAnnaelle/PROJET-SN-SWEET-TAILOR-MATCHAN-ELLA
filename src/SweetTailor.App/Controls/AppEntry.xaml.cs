namespace SweetTailor.App.Controls;

public partial class AppEntry : ContentView
{
    private const string CheckPathData = "M5,12 L10,17 L20,7";
    private const string AlertPathData = "M12,2 L1,21 L23,21 Z M12,9 L12,13 M12,17 L12.01,17";

    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(AppEntry), string.Empty, propertyChanged: (b, o, n) => ((AppEntry)b).FieldLabel.Text = (string)n);

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(AppEntry), string.Empty, BindingMode.TwoWay, propertyChanged: (b, o, n) => ((AppEntry)b).InputField.Text = (string)n);

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(AppEntry), string.Empty, propertyChanged: (b, o, n) => ((AppEntry)b).InputField.Placeholder = (string)n);

    public static readonly BindableProperty IsPasswordProperty =
        BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(AppEntry), false, propertyChanged: (b, o, n) => ((AppEntry)b).SetIsPassword((bool)n));

    public static readonly BindableProperty KeyboardProperty =
        BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(AppEntry), Keyboard.Default, propertyChanged: (b, o, n) => ((AppEntry)b).InputField.Keyboard = (Keyboard)n);

    public static readonly BindableProperty LeadingIconProperty =
        BindableProperty.Create(nameof(LeadingIcon), typeof(string), typeof(AppEntry), string.Empty, propertyChanged: (b, o, n) => ((AppEntry)b).SetLeadingIcon((string)n));

    public static readonly BindableProperty HelperTextProperty =
        BindableProperty.Create(nameof(HelperText), typeof(string), typeof(AppEntry), string.Empty, propertyChanged: (b, o, n) => ((AppEntry)b).UpdateState());

    public static readonly BindableProperty ErrorMessageProperty =
        BindableProperty.Create(nameof(ErrorMessage), typeof(string), typeof(AppEntry), string.Empty, propertyChanged: (b, o, n) => ((AppEntry)b).UpdateState());

    public static readonly BindableProperty IsValidProperty =
        BindableProperty.Create(nameof(IsValid), typeof(bool?), typeof(AppEntry), null, propertyChanged: (b, o, n) => ((AppEntry)b).UpdateState());

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public bool IsPassword
    {
        get => (bool)GetValue(IsPasswordProperty);
        set => SetValue(IsPasswordProperty, value);
    }

    public Keyboard Keyboard
    {
        get => (Keyboard)GetValue(KeyboardProperty);
        set => SetValue(KeyboardProperty, value);
    }

    public string LeadingIcon
    {
        get => (string)GetValue(LeadingIconProperty);
        set => SetValue(LeadingIconProperty, value);
    }

    public string HelperText
    {
        get => (string)GetValue(HelperTextProperty);
        set => SetValue(HelperTextProperty, value);
    }

    public string ErrorMessage
    {
        get => (string)GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }

    public bool? IsValid
    {
        get => (bool?)GetValue(IsValidProperty);
        set => SetValue(IsValidProperty, value);
    }

    public AppEntry()
    {
        InitializeComponent();

        InputField.TextChanged += (s, e) => Text = e.NewTextValue;
        InputField.Focused += (s, e) => UpdateState(isFocused: true);
        InputField.Unfocused += (s, e) => UpdateState(isFocused: false);

        var boxTap = new TapGestureRecognizer();
        boxTap.Tapped += (s, e) => InputField.Focus();
        BorderBox.GestureRecognizers.Add(boxTap);

        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(IsEnabled))
            {
                UpdateState();
            }
        };

        UpdateState();
    }

    private void SetIsPassword(bool isPassword)
    {
        InputField.IsPassword = isPassword;
        EyeButton.IsVisible = isPassword;
    }

    private void OnTogglePassword(object? sender, EventArgs e)
    {
        InputField.IsPassword = !InputField.IsPassword;
    }

    private void SetLeadingIcon(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            LeadingIconPath.IsVisible = false;
        }
        else
        {
            var resourceKey = data.ToLowerInvariant() switch
            {
                "user" => "IconPathUser",
                "phone" => "IconPathPhone",
                "mail" => "IconPathMail",
                "lock" => "IconPathLock",
                _ => null
            };

            if (resourceKey is not null &&
                Application.Current?.Resources.TryGetValue(resourceKey, out var resource) == true &&
                resource is string resourcePath)
            {
                data = resourcePath;
            }

            LeadingIconPath.Data = (Microsoft.Maui.Controls.Shapes.Geometry)new Microsoft.Maui.Controls.Shapes.PathGeometryConverter().ConvertFromInvariantString(data)!;
            LeadingIconPath.IsVisible = true;
        }
    }

    public void UpdateState(bool? isFocused = null)
    {
        bool focused = isFocused ?? InputField.IsFocused;

        if (!IsEnabled)
        {
            BorderBox.BackgroundColor = Color.FromArgb("#EEEFF6");
            BorderBox.Stroke = Color.FromArgb("#E2E3EF");
            FieldLabel.TextColor = Color.FromArgb("#A6AAC8");
            InputField.TextColor = Color.FromArgb("#A6AAC8");
            InputField.IsEnabled = false;
            MessageStack.IsVisible = false;
            StatusIcon.IsVisible = false;
            return;
        }

        InputField.IsEnabled = true;
        InputField.TextColor = Color.FromArgb("#14173A");

        if (!string.IsNullOrWhiteSpace(ErrorMessage))
        {
            // ERROR STATE
            BorderBox.BackgroundColor = Color.FromArgb("#FFF8F9");
            BorderBox.Stroke = Color.FromArgb("#D93A4A");
            BorderBox.StrokeThickness = 1.5;
            FieldLabel.TextColor = Color.FromArgb("#D93A4A");

            StatusIcon.Data = (Microsoft.Maui.Controls.Shapes.Geometry)new Microsoft.Maui.Controls.Shapes.PathGeometryConverter().ConvertFromInvariantString(AlertPathData)!;
            StatusIcon.Fill = Color.FromArgb("#D93A4A");
            StatusIcon.IsVisible = true;

            MessageStack.IsVisible = true;
            MessageIcon.Data = StatusIcon.Data;
            MessageIcon.Fill = Color.FromArgb("#D93A4A");
            MessageText.Text = ErrorMessage;
            MessageText.TextColor = Color.FromArgb("#D93A4A");
        }
        else if (IsValid == true)
        {
            // VALID STATE
            BorderBox.BackgroundColor = Colors.White;
            BorderBox.Stroke = Color.FromArgb("#2E9E6B");
            BorderBox.StrokeThickness = 1;
            FieldLabel.TextColor = Color.FromArgb("#6B7094");

            StatusIcon.Data = (Microsoft.Maui.Controls.Shapes.Geometry)new Microsoft.Maui.Controls.Shapes.PathGeometryConverter().ConvertFromInvariantString(CheckPathData)!;
            StatusIcon.Fill = Color.FromArgb("#2E9E6B");
            StatusIcon.IsVisible = true;

            MessageStack.IsVisible = false;
        }
        else if (focused)
        {
            // ACTIVE / FOCUS STATE
            BorderBox.BackgroundColor = Colors.White;
            BorderBox.Stroke = Color.FromArgb("#26306B");
            BorderBox.StrokeThickness = 1.8;
            FieldLabel.TextColor = Color.FromArgb("#6B7094");
            StatusIcon.IsVisible = false;

            if (!string.IsNullOrWhiteSpace(HelperText))
            {
                MessageStack.IsVisible = true;
                MessageIcon.IsVisible = false;
                MessageText.Text = HelperText;
                MessageText.TextColor = Color.FromArgb("#6B7094");
            }
            else
            {
                MessageStack.IsVisible = false;
            }
        }
        else
        {
            // NORMAL STATE
            BorderBox.BackgroundColor = Colors.White;
            BorderBox.Stroke = Color.FromArgb("#E8E9F3");
            BorderBox.StrokeThickness = 1;
            FieldLabel.TextColor = Color.FromArgb("#6B7094");
            StatusIcon.IsVisible = false;

            if (!string.IsNullOrWhiteSpace(HelperText))
            {
                MessageStack.IsVisible = true;
                MessageIcon.IsVisible = false;
                MessageText.Text = HelperText;
                MessageText.TextColor = Color.FromArgb("#6B7094");
            }
            else
            {
                MessageStack.IsVisible = false;
            }
        }
    }
}
