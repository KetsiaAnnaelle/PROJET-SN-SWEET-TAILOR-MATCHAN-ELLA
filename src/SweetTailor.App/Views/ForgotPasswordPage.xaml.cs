using SweetTailor.Core.ViewModels;

namespace SweetTailor.App.Views;

public partial class ForgotPasswordPage : ContentPage
{
    private readonly ForgotPasswordViewModel _viewModel;

    public ForgotPasswordPage(ForgotPasswordViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    private async void OnBackTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
