using Microsoft.Maui.Storage;

namespace SweetTailor.App.Views;

public partial class SplashPage : ContentPage
{
    private const string OnboardingDoneKey = "HasCompletedOnboardingV2";
    private bool _hasNavigated = false;

    public SplashPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_hasNavigated) return;

        BrandMark.Scale = 0.88;
        BrandMark.Opacity = 0;
        await Task.WhenAll(
            BrandMark.FadeToAsync(1, 180, Easing.CubicOut),
            BrandMark.ScaleToAsync(1, 250, Easing.SpringOut));
        await NavigateToOnboarding();
    }

    private async Task NavigateToOnboarding()
    {
        if (_hasNavigated) return;
        _hasNavigated = true;
        var route = Preferences.Default.Get(OnboardingDoneKey, false)
            ? "//LoginPage"
            : "//OnboardingPage";
        await Shell.Current.GoToAsync(route);
    }

    private async void OnSplashTapped(object? sender, EventArgs e)
    {
        await NavigateToOnboarding();
    }
}
