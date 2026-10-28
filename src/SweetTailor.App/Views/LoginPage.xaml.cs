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

    // Animation d'entrée : logo, titre, carte du formulaire et pied de page apparaissent en cascade
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        View[] blocks = { LogoBlock, TitleBlock, FormCard, FooterBlock };

        foreach (var block in blocks)
        {
            block.Opacity = 0;
            block.TranslationY = 22;
        }

        await Task.WhenAll(blocks.Select(async (block, index) =>
        {
            await Task.Delay(index * 90);
            await Task.WhenAll(
                block.FadeTo(1, 380, Easing.CubicOut),
                block.TranslateTo(0, 0, 380, Easing.CubicOut));
        }));
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

    private async void OnOnboardingTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//OnboardingPage");
    }
}