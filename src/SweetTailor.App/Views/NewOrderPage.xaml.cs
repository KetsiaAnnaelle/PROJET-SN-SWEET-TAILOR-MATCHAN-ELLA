using SweetTailor.Core.ViewModels;
using SweetTailor.App.Services;

namespace SweetTailor.App.Views;

[QueryProperty(nameof(OrderId), "OrderId")]
[QueryProperty(nameof(ClientId), "ClientId")]
[QueryProperty(nameof(ModelName), "ModelName")]
public partial class NewOrderPage : ContentPage
{
    private readonly NewOrderViewModel _viewModel;
    private int _orderId;
    private int _clientId;
    private string? _modelName;

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

    public string ModelName
    {
        set => _modelName = Uri.UnescapeDataString(value ?? string.Empty);
    }

    public NewOrderPage(NewOrderViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await _viewModel.InitializeAsync(_orderId, _clientId, _modelName);
        }
        catch (Exception ex)
        {
            _viewModel.ShowToast($"Impossible de charger la commande : {ex.Message}", ToastType.Error);
        }
    }

    private async void OnCloseTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnSaveOrderClicked(object? sender, EventArgs e)
    {
        try
        {
            var order = await _viewModel.CreateOrderAsync();
            await Shell.Current.GoToAsync($"{nameof(OrderDetailPage)}?OrderId={order.Id}");
        }
        catch (Exception ex)
        {
            _viewModel.ShowToast(ex.Message, ToastType.Error);
        }
    }

    private async void OnPickModelPhotoClicked(object? sender, EventArgs e)
    {
        try
        {
            var path = await ImageFilePicker.PickAndSaveImageAsync("Choisir la photo du modèle");
            if (path is not null)
            {
                _viewModel.PhotoPath = path;
                _viewModel.ShowToast("Photo du modèle importée.", ToastType.Success);
            }
        }
        catch (Exception ex) when (ex is FeatureNotSupportedException
                                   or PermissionException
                                   or IOException
                                   or UnauthorizedAccessException)
        {
            _viewModel.ShowToast($"Impossible d'importer la photo : {ex.Message}", ToastType.Error);
        }
    }
}
