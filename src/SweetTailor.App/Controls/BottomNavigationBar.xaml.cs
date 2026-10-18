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
        var activeColor = (Color)Application.Current!.Resources["Indigo"];
        var inactiveColor = (Color)Application.Current.Resources["Muted"];

        SetTabStyle(ClientsIcon, ClientsLabel, CurrentTab == "Clients", activeColor, inactiveColor);
        SetTabStyle(OrdersIcon, OrdersLabel, CurrentTab == "Orders", activeColor, inactiveColor);
        SetTabStyle(CatalogIcon, CatalogLabel, CurrentTab == "Catalog", activeColor, inactiveColor);
        SetTabStyle(ProfileIcon, ProfileLabel, CurrentTab == "Profile", activeColor, inactiveColor);
    }

    private static void SetTabStyle(Microsoft.Maui.Controls.Shapes.Path icon, Label label, bool isSelected, Color activeColor, Color inactiveColor)
    {
        var color = isSelected ? activeColor : inactiveColor;
        icon.Stroke = color;
        label.TextColor = color;
        label.FontFamily = isSelected ? "DMSansBold" : "DMSans";
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
