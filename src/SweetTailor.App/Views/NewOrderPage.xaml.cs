using SweetTailor.Core.ViewModels;
using SweetTailor.App.Services;

namespace SweetTailor.App.Views;

public partial class NewOrderPage : ContentPage
{
    private readonly NewOrderViewModel _viewModel;

    public NewOrderPage(NewOrderViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    private async void OnCloseTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnSaveOrderClicked(object? sender, EventArgs e)
    {
        var order = await _viewModel.CreateOrderAsync();
        await Task.Delay(400);
        await Shell.Current.GoToAsync($"{nameof(OrderDetailPage)}?OrderId={order.Id}");
    }

    private async void OnPickModelPhotoTapped(object? sender, TappedEventArgs e)
    {
        var path = await ImageFilePicker.PickAndSaveImageAsync("Choisir la photo du modèle");
        if (path is not null)
        {
            _viewModel.PhotoPath = path;
        }
    }
}
