using SweetTailor.Core.ViewModels;

namespace SweetTailor.App.Views;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;

    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _viewModel.LoginSucceeded += OnLoginSucceeded;
    }

    private async void OnLoginSucceeded(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ClientsPage");
    }

    private async void OnForgotPasswordTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ForgotPasswordPage));
    }

    private async void OnRegisterTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(RegisterPage));
    }
}
