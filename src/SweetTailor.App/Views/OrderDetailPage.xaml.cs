using SweetTailor.Core.ViewModels;

namespace SweetTailor.App.Views;

[QueryProperty(nameof(OrderId), "OrderId")]
public partial class OrderDetailPage : ContentPage
{
    private readonly OrderDetailViewModel _viewModel;
    private int _orderId = 142;

    public string OrderId
    {
        set
        {
            if (int.TryParse(value, out var id))
            {
                _orderId = id;
            }
        }
    }

    public OrderDetailPage(OrderDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadOrderAsync(_orderId);
    }

    private async void OnBackTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnEditTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(NewOrderPage)}?OrderId={_orderId}");
    }

    private async void OnDeleteTapped(object? sender, EventArgs e)
    {
        var confirmed = await DisplayAlertAsync("Supprimer la commande", "Cette action est définitive. Continuer ?", "Supprimer", "Annuler");
        if (!confirmed)
        {
            return;
        }

        try
        {
            if (!await _viewModel.DeleteOrderAsync(_orderId))
            {
                _viewModel.ShowToast("La commande n'existe plus.", ToastType.Error);
                return;
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            _viewModel.ShowToast($"Impossible de supprimer la commande : {ex.Message}", ToastType.Error);
        }
    }
}
