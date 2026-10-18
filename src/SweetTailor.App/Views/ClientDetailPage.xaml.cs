using SweetTailor.Core.Models;
using SweetTailor.Core.ViewModels;

namespace SweetTailor.App.Views;

[QueryProperty(nameof(ClientId), "ClientId")]
public partial class ClientDetailPage : ContentPage
{
    private readonly ClientDetailViewModel _viewModel;
    private int _clientId = 1;

    public string ClientId
    {
        set
        {
            if (int.TryParse(value, out var id))
            {
                _clientId = id;
            }
        }
    }

    public ClientDetailPage(ClientDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadClientAsync(_clientId);
    }

    private void OnTabChanged(object? sender, string tab)
    {
        MesuresTabContent.IsVisible = tab == "Mesures";
        CommandesTabContent.IsVisible = tab == "Commandes";
        NotesTabContent.IsVisible = tab == "Notes";
    }

    private void OnCallClicked(object? sender, EventArgs e)
    {
        if (_viewModel.Client != null && !string.IsNullOrWhiteSpace(_viewModel.Client.PhoneNumber))
        {
            try
            {
                PhoneDialer.Default.Open(_viewModel.Client.PhoneNumber);
            }
            catch
            {
                _viewModel.ShowToast("Impossible d'ouvrir le composeur téléphonique", ToastType.Error);
            }
        }
    }

    private async void OnWhatsAppClicked(object? sender, EventArgs e)
    {
        if (_viewModel.Client != null && !string.IsNullOrWhiteSpace(_viewModel.Client.PhoneNumber))
        {
            try
            {
                var cleanPhone = _viewModel.Client.PhoneNumber.Replace(" ", "").Replace("+", "");
                var url = $"https://wa.me/{cleanPhone}";
                await Launcher.Default.OpenAsync(new Uri(url));
            }
            catch
            {
                _viewModel.ShowToast("Impossible d'ouvrir WhatsApp", ToastType.Error);
            }
        }
    }

    private async void OnBackTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnEditTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ClientFormPage));
    }

    private async void OnViewAllMeasurementsClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(MeasurementsPage)}?ClientId={_clientId}");
    }

    private async void OnNewOrderClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(NewOrderPage));
    }

    private async void OnOrderRowClicked(object? sender, EventArgs e)
    {
        if (sender is View view && view.BindingContext is Order order)
        {
            await Shell.Current.GoToAsync($"{nameof(OrderDetailPage)}?OrderId={order.Id}");
        }
    }
}
