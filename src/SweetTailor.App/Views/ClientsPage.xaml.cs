using SweetTailor.Core.Models;
using SweetTailor.Core.ViewModels;

namespace SweetTailor.App.Views;

public partial class ClientsPage : ContentPage
{
    private readonly ClientsViewModel _viewModel;

    public ClientsPage(ClientsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadClientsAsync();
    }

    private async void OnClientRowClicked(object? sender, EventArgs e)
    {
        if (sender is View view && view.BindingContext is Client client)
        {
            await Shell.Current.GoToAsync($"{nameof(ClientDetailPage)}?ClientId={client.Id}");
        }
    }

    private async void OnAddClientFabTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ClientFormPage));
    }

    private async void OnSearchTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SearchPage));
    }

    private async void OnEmptyStateActionClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ClientFormPage));
    }

    private async void OnFilterChipClicked(object? sender, EventArgs e)
    {
        if (sender is Controls.FilterChip chip)
        {
            chip.SelectExclusively();
            await _viewModel.SelectFilterChipAsync(chip.Text);
        }
    }
}
