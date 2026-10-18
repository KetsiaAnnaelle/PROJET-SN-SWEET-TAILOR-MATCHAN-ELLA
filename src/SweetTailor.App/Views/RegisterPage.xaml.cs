using SweetTailor.Core.ViewModels;

namespace SweetTailor.App.Views;

public partial class RegisterPage : ContentPage
{
    private readonly RegisterViewModel _viewModel;

    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _viewModel.RegistrationSucceeded += OnRegistrationSucceeded;
    }

    private async void OnRegistrationSucceeded(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ClientsPage");
    }

    private async void OnBackTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
