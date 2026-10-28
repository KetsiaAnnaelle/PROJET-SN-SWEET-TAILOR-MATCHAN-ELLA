using Microsoft.Extensions.DependencyInjection;
using SweetTailor.App.Views;

namespace SweetTailor.App;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();

        SplashShellContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<SplashPage>());
        OnboardingShellContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<OnboardingPage>());
        LoginShellContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<LoginPage>());

        ClientsShellContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<ClientsPage>());
        OrdersShellContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<OrdersPage>());
        CatalogShellContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<CatalogPage>());
        ProfileShellContent.ContentTemplate = new DataTemplate(() => services.GetRequiredService<ProfilePage>());

        Routing.RegisterRoute(nameof(ForgotPasswordPage), typeof(ForgotPasswordPage));
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(ClientDetailPage), typeof(ClientDetailPage));
        Routing.RegisterRoute(nameof(ClientFormPage), typeof(ClientFormPage));
        Routing.RegisterRoute(nameof(MeasurementsPage), typeof(MeasurementsPage));
        Routing.RegisterRoute(nameof(OrderDetailPage), typeof(OrderDetailPage));
        Routing.RegisterRoute(nameof(NewOrderPage), typeof(NewOrderPage));
        Routing.RegisterRoute(nameof(CatalogDetailPage), typeof(CatalogDetailPage));
        Routing.RegisterRoute(nameof(CatalogModelFormPage), typeof(CatalogModelFormPage));
        Routing.RegisterRoute(nameof(SearchPage), typeof(SearchPage));
        Routing.RegisterRoute(nameof(ComponentsCharterPage), typeof(ComponentsCharterPage));
    }

}
