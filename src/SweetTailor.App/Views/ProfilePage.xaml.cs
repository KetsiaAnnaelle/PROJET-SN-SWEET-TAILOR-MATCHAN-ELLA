using SweetTailor.Core.ViewModels;

namespace SweetTailor.App.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _viewModel;

    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _viewModel.LogoutRequested += OnLogoutRequested;
    }

    private async void OnLogoutRequested(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }

    private async void OnNavigateToComponentsCharterClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ComponentsCharterPage));
    }
}
