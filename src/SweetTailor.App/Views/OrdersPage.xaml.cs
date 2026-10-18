using SweetTailor.Core.Models;
using SweetTailor.Core.ViewModels;

namespace SweetTailor.App.Views;

public partial class OrdersPage : ContentPage
{
    private readonly OrdersViewModel _viewModel;

    public OrdersPage(OrdersViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadOrdersAsync();
    }

    private async void OnOrderRowClicked(object? sender, EventArgs e)
    {
        if (sender is View view && view.BindingContext is Order order)
        {
            await Shell.Current.GoToAsync($"{nameof(OrderDetailPage)}?OrderId={order.Id}");
        }
    }

    private async void OnAddOrderFabTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(NewOrderPage));
    }

    private async void OnEmptyStateActionClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(NewOrderPage));
    }

    private async void OnFilterChipClicked(object? sender, EventArgs e)
    {
        if (sender is Controls.FilterChip chip)
        {
            chip.SelectExclusively();
            await _viewModel.SelectFilterAsync(chip.Text);
        }
    }
}
