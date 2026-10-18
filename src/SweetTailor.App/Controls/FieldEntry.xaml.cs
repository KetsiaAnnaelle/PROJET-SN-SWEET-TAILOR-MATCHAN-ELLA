namespace SweetTailor.App.Controls;

public enum FieldValidationState
{
    Normal,
    Active,
    Valid,
    Error,
    Disabled
}

public partial class FieldEntry : ContentView
{
    public static readonly BindableProperty LabelTextProperty =
        BindableProperty.Create(nameof(LabelText), typeof(string), typeof(FieldEntry), string.Empty, propertyChanged: (b, o, n) => ((FieldEntry)b).FieldLabel.Text = (string)n);

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(FieldEntry), string.Empty, BindingMode.TwoWay, propertyChanged: (b, o, n) => ((FieldEntry)b).EntryInput.Text = (string)n);

    public static readonly BindableProperty IsPasswordProperty =
        BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(FieldEntry), false, propertyChanged: (b, o, n) => ((FieldEntry)b).EntryInput.IsPassword = (bool)n);

    public static readonly BindableProperty ValidationStateProperty =
        BindableProperty.Create(nameof(ValidationState), typeof(FieldValidationState), typeof(FieldEntry), FieldValidationState.Normal, propertyChanged: (b, o, n) => ((FieldEntry)b).UpdateState((FieldValidationState)n));

    public static readonly BindableProperty HelperMessageProperty =
        BindableProperty.Create(nameof(HelperMessage), typeof(string), typeof(FieldEntry), string.Empty, propertyChanged: (b, o, n) => ((FieldEntry)b).UpdateHelper());

    public static readonly BindableProperty IconDataProperty =
        BindableProperty.Create(nameof(IconData), typeof(string), typeof(FieldEntry), string.Empty, propertyChanged: (b, o, n) => ((FieldEntry)b).UpdateIcon((string)n));

    public string LabelText
    {
        get => (string)GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsPassword
    {
        get => (bool)GetValue(IsPasswordProperty);
        set => SetValue(IsPasswordProperty, value);
    }

    public FieldValidationState ValidationState
    {
        get => (FieldValidationState)GetValue(ValidationStateProperty);
        set => SetValue(ValidationStateProperty, value);
    }

    public string HelperMessage
    {
        get => (string)GetValue(HelperMessageProperty);
        set => SetValue(HelperMessageProperty, value);
    }

    public string IconData
    {
        get => (string)GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }

    public FieldEntry()
    {
        InitializeComponent();
        EntryInput.TextChanged += (s, e) => Text = e.NewTextValue;
        EntryInput.Focused += (s, e) => { if (ValidationState == FieldValidationState.Normal) UpdateState(FieldValidationState.Active); };
        EntryInput.Unfocused += (s, e) => { if (ValidationState == FieldValidationState.Active) UpdateState(FieldValidationState.Normal); };
    }

    private void UpdateIcon(string pathData)
    {
        if (string.IsNullOrWhiteSpace(pathData))
        {
            TrailingIconPath.IsVisible = false;
        }
        else
        {
            TrailingIconPath.Data = (Microsoft.Maui.Controls.Shapes.Geometry)new Microsoft.Maui.Controls.Shapes.PathGeometryConverter().ConvertFromInvariantString(pathData)!;
            TrailingIconPath.IsVisible = true;
        }
    }

    private void UpdateHelper()
    {
        if (string.IsNullOrWhiteSpace(HelperMessage))
        {
            HelperStack.IsVisible = false;
        }
        else
        {
            HelperStack.IsVisible = true;
            HelperText.Text = HelperMessage;
        }
    }

    public void UpdateState(FieldValidationState state)
    {
        switch (state)
        {
            case FieldValidationState.Normal:
                FieldBorder.BackgroundColor = Colors.White;
                FieldBorder.Stroke = Color.FromArgb("#E8E9F3");
                FieldLabel.TextColor = Color.FromArgb("#6B7094");
                EntryInput.IsEnabled = true;
                HelperStack.IsVisible = !string.IsNullOrWhiteSpace(HelperMessage);
                HelperText.TextColor = Color.FromArgb("#6B7094");
                break;
            case FieldValidationState.Active:
                FieldBorder.BackgroundColor = Colors.White;
                FieldBorder.Stroke = Color.FromArgb("#26306B");
                FieldLabel.TextColor = Color.FromArgb("#6B7094");
                EntryInput.IsEnabled = true;
                break;
            case FieldValidationState.Valid:
                FieldBorder.BackgroundColor = Colors.White;
                FieldBorder.Stroke = Color.FromArgb("#2E9E6B");
                FieldLabel.TextColor = Color.FromArgb("#6B7094");
                TrailingIconPath.Fill = Color.FromArgb("#2E9E6B");
                break;
            case FieldValidationState.Error:
                FieldBorder.BackgroundColor = Color.FromArgb("#FFF8F9");
                FieldBorder.Stroke = Color.FromArgb("#D93A4A");
                FieldLabel.TextColor = Color.FromArgb("#D93A4A");
                TrailingIconPath.Fill = Color.FromArgb("#D93A4A");
                HelperStack.IsVisible = true;
                HelperText.TextColor = Color.FromArgb("#D93A4A");
                break;
            case FieldValidationState.Disabled:
                FieldBorder.BackgroundColor = Color.FromArgb("#EEEFF6");
                FieldBorder.Stroke = Color.FromArgb("#E2E3EF");
                FieldLabel.TextColor = Color.FromArgb("#A6AAC8");
                EntryInput.TextColor = Color.FromArgb("#A6AAC8");
                EntryInput.IsEnabled = false;
                break;
        }
    }
}
