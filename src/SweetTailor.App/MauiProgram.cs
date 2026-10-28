using Microsoft.Extensions.Logging;
using SweetTailor.App.Views;
using SweetTailor.Core.Services;
using SweetTailor.Core.ViewModels;
using SweetTailor.App.Services;

namespace SweetTailor.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("BricolageGrotesque-ExtraBold.ttf", "BricolageExtraBold");
                fonts.AddFont("BricolageGrotesque-ExtraBold.ttf", "BricolageGrotesque");
                fonts.AddFont("DMSans-Regular.ttf", "DMSans");
                fonts.AddFont("DMSans-Medium.ttf", "DMSansMedium");
                fonts.AddFont("DMSans-SemiBold.ttf", "DMSansSemiBold");
                fonts.AddFont("DMSans-Bold.ttf", "DMSansBold");
            });

        var defaultApiAddress = DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5080/"
            : "http://127.0.0.1:5080/";
        var apiAddress = Preferences.Default.Get("sweet-tailor-api-url", defaultApiAddress);

        builder.Services.AddSingleton(new ApiSession());
        builder.Services.AddSingleton(_ => new HttpClient
        {
            BaseAddress = new Uri(apiAddress, UriKind.Absolute),
            Timeout = TimeSpan.FromSeconds(8)
        });
        builder.Services.AddTransient<IClientService, ApiClientService>();
        builder.Services.AddTransient<IOrderService, ApiOrderService>();
        builder.Services.AddTransient<ICatalogService, ApiCatalogService>();
        builder.Services.AddTransient<IAuthService, ApiAuthService>();
        builder.Services.AddTransient<IWorkshopProfileService, ApiWorkshopProfileService>();

        // Demo state remains available to showcase the screens; production data is provided by the API.
        builder.Services.AddSingleton<IDemoModeService, DemoModeService>();
        builder.Services.AddTransient<IMeasurementService, MeasurementService>();

        // Register ViewModels
        builder.Services.AddTransient<SplashViewModel>();
        builder.Services.AddTransient<OnboardingViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<ForgotPasswordViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<ClientsViewModel>();
        builder.Services.AddTransient<ClientDetailViewModel>();
        builder.Services.AddTransient<ClientFormViewModel>();
        builder.Services.AddTransient<MeasurementsViewModel>();
        builder.Services.AddTransient<OrdersViewModel>();
        builder.Services.AddTransient<OrderDetailViewModel>();
        builder.Services.AddTransient<NewOrderViewModel>();
        builder.Services.AddTransient<CatalogViewModel>();
        builder.Services.AddTransient<CatalogDetailViewModel>();
        builder.Services.AddTransient<CatalogModelFormViewModel>();
        builder.Services.AddTransient<SearchViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();

        // Register Pages
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<SplashPage>();
        builder.Services.AddTransient<OnboardingPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<ForgotPasswordPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<ClientsPage>();
        builder.Services.AddTransient<ClientDetailPage>();
        builder.Services.AddTransient<ClientFormPage>();
        builder.Services.AddTransient<MeasurementsPage>();
        builder.Services.AddTransient<OrdersPage>();
        builder.Services.AddTransient<OrderDetailPage>();
        builder.Services.AddTransient<NewOrderPage>();
        builder.Services.AddTransient<CatalogPage>();
        builder.Services.AddTransient<CatalogDetailPage>();
        builder.Services.AddTransient<CatalogModelFormPage>();
        builder.Services.AddTransient<SearchPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<ComponentsCharterPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
