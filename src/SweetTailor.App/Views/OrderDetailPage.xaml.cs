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
}
