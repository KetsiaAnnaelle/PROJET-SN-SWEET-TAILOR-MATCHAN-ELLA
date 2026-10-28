using SweetTailor.Core.Models;
using SweetTailor.Core.ViewModels;
using SweetTailor.App.Services;

namespace SweetTailor.App.Views;

[QueryProperty(nameof(ClientId), "ClientId")]
public partial class ClientFormPage : ContentPage
{
    private readonly ClientFormViewModel _viewModel;
    private int _clientId;
    private int _loadedClientId;

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

    public ClientFormPage(ClientFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_clientId == 0 || _loadedClientId == _clientId)
        {
            return;
        }

        try
        {
            await _viewModel.LoadClientAsync(_clientId);
            _loadedClientId = _clientId;
        }
        catch (Exception ex)
        {
            _viewModel.ShowToast($"Impossible de charger le client : {ex.Message}", ToastType.Error);
        }
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
        try
        {
            var client = await _viewModel.SaveAsync(true);
            await Shell.Current.GoToAsync($"{nameof(MeasurementsPage)}?ClientId={client.Id}");
        }
        catch (Exception ex)
        {
            _viewModel.ShowToast(ex.Message, ToastType.Error);
        }
    }

    private async void OnSaveOnlyClicked(object? sender, EventArgs e)
    {
        try
        {
            await _viewModel.SaveAsync(false);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            _viewModel.ShowToast(ex.Message, ToastType.Error);
        }
    }

    private async void OnDeleteClientClicked(object? sender, EventArgs e)
    {
        var confirmed = await DisplayAlertAsync("Supprimer le client", "Cette action supprimera également ses commandes. Continuer ?", "Supprimer", "Annuler");
        if (!confirmed)
        {
            return;
        }

        try
        {
            if (await _viewModel.DeleteAsync())
            {
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            _viewModel.ShowToast($"Impossible de supprimer le client : {ex.Message}", ToastType.Error);
        }
    }
}
