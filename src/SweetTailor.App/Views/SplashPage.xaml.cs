namespace SweetTailor.App.Views;

public partial class SplashPage : ContentPage
{
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
            BrandMark.FadeToAsync(1, 450, Easing.CubicOut),
            BrandMark.ScaleToAsync(1, 650, Easing.SpringOut));

        await Task.Delay(1300);
        await NavigateToOnboarding();
    }

    private async Task NavigateToOnboarding()
    {
        if (_hasNavigated) return;
        _hasNavigated = true;
        await Shell.Current.GoToAsync("//OnboardingPage");
    }

    private async void OnSplashTapped(object? sender, EventArgs e)
    {
        await NavigateToOnboarding();
    }
}
