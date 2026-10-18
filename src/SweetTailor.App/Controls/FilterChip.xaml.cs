namespace SweetTailor.App.Controls;

public enum ChipState
{
    Normal,
    Selected,
    Disabled,
    Error
}

public partial class FilterChip : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(FilterChip), string.Empty, propertyChanged: (b, o, n) => ((FilterChip)b).ChipLabel.Text = (string)n);

    public static readonly BindableProperty IsSelectedProperty =
        BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(FilterChip), false, propertyChanged: (b, o, n) => ((FilterChip)b).UpdateState());

    public static readonly BindableProperty StateProperty =
        BindableProperty.Create(nameof(State), typeof(ChipState), typeof(FilterChip), ChipState.Normal, propertyChanged: (b, o, n) => ((FilterChip)b).UpdateState());

    public event EventHandler? Tapped;

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public ChipState State
    {
        get => (ChipState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public FilterChip()
    {
        InitializeComponent();

        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) =>
        {
            if (State == ChipState.Disabled || !IsEnabled) return;
            IsSelected = !IsSelected;
            Tapped?.Invoke(this, EventArgs.Empty);
        };
        GestureRecognizers.Add(tap);

        UpdateState();
    }

    public void UpdateState()
    {
        if (!IsEnabled || State == ChipState.Disabled)
        {
            ChipBorder.BackgroundColor = Color.FromArgb("#EEEFF6");
            ChipBorder.Stroke = Color.FromArgb("#E2E3EF");
            ChipLabel.TextColor = Color.FromArgb("#A6AAC8");
            return;
        }

        if (State == ChipState.Error)
        {
            ChipBorder.BackgroundColor = Color.FromArgb("#FFF8F9");
            ChipBorder.Stroke = Color.FromArgb("#D93A4A");
            ChipLabel.TextColor = Color.FromArgb("#D93A4A");
            return;
        }

        if (IsSelected || State == ChipState.Selected)
        {
            ChipBorder.BackgroundColor = Color.FromArgb("#26306B");
            ChipBorder.Stroke = Color.FromArgb("#26306B");
            ChipLabel.TextColor = Colors.White;
        }
        else
        {
            ChipBorder.BackgroundColor = Colors.White;
            ChipBorder.Stroke = Color.FromArgb("#E8E9F3");
            ChipLabel.TextColor = Color.FromArgb("#6B7094");
        }
    }

    public void SelectExclusively()
    {
        if (Parent is HorizontalStackLayout group)
        {
            foreach (var child in group.Children.OfType<FilterChip>())
            {
                child.IsSelected = ReferenceEquals(child, this);
            }
        }
        else
        {
            IsSelected = true;
        }
    }
}
