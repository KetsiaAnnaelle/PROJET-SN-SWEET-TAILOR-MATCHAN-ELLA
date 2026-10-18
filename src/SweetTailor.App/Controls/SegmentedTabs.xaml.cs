namespace SweetTailor.App.Controls;

public partial class SegmentedTabs : ContentView
{
    public static readonly BindableProperty TabsProperty =
        BindableProperty.Create(nameof(Tabs), typeof(string), typeof(SegmentedTabs), "Mesures,Commandes,Notes", propertyChanged: (b, o, n) => ((SegmentedTabs)b).RebuildTabs());

    public static readonly BindableProperty SelectedTabProperty =
        BindableProperty.Create(nameof(SelectedTab), typeof(string), typeof(SegmentedTabs), "Mesures", BindingMode.TwoWay, propertyChanged: (b, o, n) => ((SegmentedTabs)b).UpdateSelection((string)n));

    public event EventHandler<string>? TabChanged;

    public string Tabs
    {
        get => (string)GetValue(TabsProperty);
        set => SetValue(TabsProperty, value);
    }

    public string SelectedTab
    {
        get => (string)GetValue(SelectedTabProperty);
        set => SetValue(SelectedTabProperty, value);
    }

    public SegmentedTabs()
    {
        InitializeComponent();
        RebuildTabs();
    }

    private void RebuildTabs()
    {
        TabsGrid.Children.Clear();
        TabsGrid.ColumnDefinitions.Clear();

        var tabTitles = Tabs.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (tabTitles.Length == 0) return;

        for (int i = 0; i < tabTitles.Length; i++)
        {
            TabsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            var tabTitle = tabTitles[i].Trim();
            var tabBorder = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(11) },
                StrokeThickness = 0,
                Padding = new Thickness(6, 0)
            };

            var label = new Label
            {
                Text = tabTitle,
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
            tabBorder.Content = label;

            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) =>
            {
                SelectedTab = tabTitle;
                TabChanged?.Invoke(this, tabTitle);
            };
            tabBorder.GestureRecognizers.Add(tap);

            Grid.SetColumn(tabBorder, i);
            TabsGrid.Children.Add(tabBorder);
        }

        UpdateSelection(SelectedTab);
    }

    private void UpdateSelection(string selectedTab)
    {
        for (int i = 0; i < TabsGrid.Children.Count; i++)
        {
            if (TabsGrid.Children[i] is Border border && border.Content is Label label)
            {
                bool isSelected = label.Text == selectedTab;
                border.BackgroundColor = isSelected ? Colors.White : Colors.Transparent;
                border.Shadow = isSelected
                    ? new Shadow { Offset = new Point(0, 1), Radius = 4, Opacity = 0.15f }
                    : new Shadow { Opacity = 0 };
                label.TextColor = isSelected ? Color.FromArgb("#26306B") : Color.FromArgb("#6B7094");
            }
        }
    }
}
