using SweetTailor.Core.Models;

namespace SweetTailor.App.Controls;

public partial class MeasureRow : ContentView
{
    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(MeasureRow), string.Empty, propertyChanged: OnDisplayPropertyChanged);

    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(MeasureRow), string.Empty, propertyChanged: OnDisplayPropertyChanged);

    public static readonly BindableProperty UnitProperty =
        BindableProperty.Create(nameof(Unit), typeof(string), typeof(MeasureRow), "cm", propertyChanged: OnDisplayPropertyChanged);

    public static readonly BindableProperty IsHighlightedProperty =
        BindableProperty.Create(nameof(IsHighlighted), typeof(bool), typeof(MeasureRow), false, propertyChanged: OnDisplayPropertyChanged);

    public static readonly BindableProperty MeasurementProperty =
        BindableProperty.Create(nameof(Measurement), typeof(MeasurementItem), typeof(MeasureRow), null, propertyChanged: OnDisplayPropertyChanged);

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public bool IsHighlighted
    {
        get => (bool)GetValue(IsHighlightedProperty);
        set => SetValue(IsHighlightedProperty, value);
    }

    public MeasurementItem? Measurement
    {
        get => (MeasurementItem?)GetValue(MeasurementProperty);
        set => SetValue(MeasurementProperty, value);
    }

    public MeasureRow()
    {
        InitializeComponent();
        UpdateRow();
    }

    private static void OnDisplayPropertyChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((MeasureRow)bindable).UpdateRow();

    private bool _updating;

    private void UpdateRow()
    {
        if (_updating) return;
        _updating = true;
        try
        {
            var item = Measurement;
            NameLabel.Text = item?.Name ?? Label;
            var display = item != null ? item.FormattedValue : Value;
            if (ValueEntry.Text != display)
                ValueEntry.Text = display;
            UnitLabel.Text = Unit;

            if (item?.IsHighlighted ?? IsHighlighted)
            {
                RowBorder.BackgroundColor = Color.FromArgb("#FDF3D6");
                RowBorder.Stroke = Color.FromArgb("#F4C95D");
                RowBorder.StrokeThickness = 1.5;
                ValueEntry.TextColor = Color.FromArgb("#8A6A10");
                InputContainer.BackgroundColor = Color.FromArgb("#FDF3D6");
            }
            else
            {
                RowBorder.BackgroundColor = Colors.White;
                RowBorder.Stroke = Color.FromArgb("#E8E9F3");
                RowBorder.StrokeThickness = 1;
                ValueEntry.TextColor = Color.FromArgb("#26306B");
                InputContainer.BackgroundColor = Color.FromArgb("#F6F5FA");
            }
        }
        finally
        {
            _updating = false;
        }
    }

    private void OnValueChanged(object? sender, TextChangedEventArgs e)
    {
        if (_updating) return;
        Value = e.NewTextValue ?? string.Empty;
        if (Measurement != null && double.TryParse(e.NewTextValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsed))
        {
            Measurement.Value = parsed;
        }
    }
}
