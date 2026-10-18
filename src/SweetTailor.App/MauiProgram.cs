using Microsoft.Extensions.Logging;
using SweetTailor.App.Views;
using SweetTailor.Core.Services;
using SweetTailor.Core.ViewModels;

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

        // Register Business Services (Singleton for mock state continuity in memory)
        builder.Services.AddSingleton<IDemoModeService, DemoModeService>();
        builder.Services.AddSingleton<IClientService, ClientService>();
        builder.Services.AddSingleton<IOrderService, OrderService>();
        builder.Services.AddSingleton<ICatalogService, CatalogService>();
        builder.Services.AddSingleton<IAuthService, AuthService>();

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
        builder.Services.AddTransient<SearchPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<ComponentsCharterPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
