using SweetTailor.Core.ViewModels;

namespace SweetTailor.App.Views;

public partial class ComponentsCharterPage : ContentPage
{
    public ComponentsCharterPage()
    {
        InitializeComponent();
    }

    private void OnShowSuccessToastClicked(object? sender, EventArgs e)
    {
        LiveToast.Type = ToastType.Success;
        LiveToast.Message = "Mesure enregistrée avec succès (28 cm) !";
        LiveToast.IsVisible = true;
    }

    private void OnShowErrorToastClicked(object? sender, EventArgs e)
    {
        LiveToast.Type = ToastType.Error;
        LiveToast.Message = "Erreur de synchronisation réseau (NET-01).";
        LiveToast.IsVisible = true;
    }

    private void OnShowInfoToastClicked(object? sender, EventArgs e)
    {
        LiveToast.Type = ToastType.Info;
        LiveToast.Message = "Rappel : essayage de Nadège Mbarga dans 2 jours.";
        LiveToast.IsVisible = true;
    }
}
