namespace SweetTailor.App.Controls;

public partial class BottomNavigationBar : ContentView
{
    public static readonly BindableProperty CurrentTabProperty = BindableProperty.Create(
        nameof(CurrentTab),
        typeof(string),
        typeof(BottomNavigationBar),
        "Clients",
        propertyChanged: OnCurrentTabChanged);

    public BottomNavigationBar()
    {
        InitializeComponent();
        UpdateSelectedTab();
    }

    public string CurrentTab
    {
        get => (string)GetValue(CurrentTabProperty);
        set => SetValue(CurrentTabProperty, value);
    }

    private static void OnCurrentTabChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((BottomNavigationBar)bindable).UpdateSelectedTab();
    }

    private void UpdateSelectedTab()
    {
        var activeColor = (Color)Application.Current!.Resources["Rose"];
        var activeBackground = (Color)Application.Current.Resources["RoseSoft"];
        var inactiveColor = (Color)Application.Current.Resources["Muted"];

        SetTabStyle(ClientsItem, ClientsIcon, ClientsLabel, CurrentTab == "Clients", activeColor, activeBackground, inactiveColor);
        SetTabStyle(OrdersItem, OrdersIcon, OrdersLabel, CurrentTab == "Orders", activeColor, activeBackground, inactiveColor);
        SetTabStyle(CatalogItem, CatalogIcon, CatalogLabel, CurrentTab == "Catalog", activeColor, activeBackground, inactiveColor);
        SetTabStyle(ProfileItem, ProfileIcon, ProfileLabel, CurrentTab == "Profile", activeColor, activeBackground, inactiveColor);
    }

    private static void SetTabStyle(Border item, Microsoft.Maui.Controls.Shapes.Path icon, Label label, bool isSelected, Color activeColor, Color activeBackground, Color inactiveColor)
    {
        var color = isSelected ? activeColor : inactiveColor;
        icon.Stroke = color;
        label.TextColor = color;
        label.FontFamily = isSelected ? "DMSansBold" : "DMSans";
        item.BackgroundColor = isSelected ? activeBackground : Colors.Transparent;
    }

    private async void OnClientsTapped(object? sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("//ClientsPage");

    private async void OnOrdersTapped(object? sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("//OrdersPage");

    private async void OnCatalogTapped(object? sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("//CatalogPage");

    private async void OnProfileTapped(object? sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("//ProfilePage");
}
