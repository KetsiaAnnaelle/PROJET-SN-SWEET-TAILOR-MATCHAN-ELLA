using SweetTailor.Core.Models;
using SweetTailor.Core.ViewModels;
using SweetTailor.App.Services;

namespace SweetTailor.App.Views;

public partial class ClientFormPage : ContentPage
{
    private readonly ClientFormViewModel _viewModel;

    public ClientFormPage(ClientFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    private void OnCategoryChanged(object? sender, string tab)
    {
        var cat = tab switch
        {
            "Homme" => ClientCategory.Homme,
            "Enfant" => ClientCategory.Enfant,
            _ => ClientCategory.Femme
        };
        _viewModel.SelectCategory(cat);
    }

    private async void OnCloseTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnPickClientPhotoTapped(object? sender, TappedEventArgs e)
    {
        var path = await ImageFilePicker.PickAndSaveImageAsync("Choisir la photo du client");
        if (path is not null)
        {
            _viewModel.PhotoPath = path;
        }
    }

    private async void OnSaveAndAddMeasurementsClicked(object? sender, EventArgs e)
    {
        var client = await _viewModel.SaveAsync(true);
        await Task.Delay(300);
        await Shell.Current.GoToAsync($"{nameof(MeasurementsPage)}?ClientId={client.Id}");
    }

    private async void OnSaveOnlyClicked(object? sender, EventArgs e)
    {
        await _viewModel.SaveAsync(false);
        await Task.Delay(300);
        await Shell.Current.GoToAsync("..");
    }
}
