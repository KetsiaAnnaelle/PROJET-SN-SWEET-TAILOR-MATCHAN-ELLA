using SweetTailor.Core.ViewModels;

namespace SweetTailor.App.Views;

[QueryProperty(nameof(ClientId), "ClientId")]
public partial class MeasurementsPage : ContentPage
{
    private readonly MeasurementsViewModel _viewModel;
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

    public MeasurementsPage(MeasurementsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadMeasurementsAsync(_clientId);
    }

    private void OnCategoryChipClicked(object? sender, EventArgs e)
    {
        if (sender is Controls.FilterChip chip)
        {
            chip.SelectExclusively();
            _viewModel.SelectCategory(chip.Text);
        }
    }

    private async void OnBackTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
