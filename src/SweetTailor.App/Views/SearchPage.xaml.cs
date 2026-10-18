using SweetTailor.Core.Models;
using SweetTailor.Core.ViewModels;

namespace SweetTailor.App.Views;

public partial class SearchPage : ContentPage
{
    private readonly SearchViewModel _viewModel;

    public SearchPage(SearchViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.SearchAsync();
    }

    private async void OnBackTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnRecentSearchClicked(object? sender, EventArgs e)
    {
        if (sender is Controls.FilterChip chip)
        {
            await _viewModel.SelectRecentSearchAsync(chip.Text);
        }
    }

    private async void OnClientResultClicked(object? sender, EventArgs e)
    {
        if (sender is View view && view.BindingContext is Client client)
        {
            await Shell.Current.GoToAsync($"{nameof(ClientDetailPage)}?ClientId={client.Id}");
        }
    }

    private async void OnOrderResultClicked(object? sender, EventArgs e)
    {
        if (sender is View view && view.BindingContext is Order order)
        {
            await Shell.Current.GoToAsync($"{nameof(OrderDetailPage)}?OrderId={order.Id}");
        }
    }
}
