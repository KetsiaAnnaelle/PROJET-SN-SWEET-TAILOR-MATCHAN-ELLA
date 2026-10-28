using SweetTailor.App.Services;
using SweetTailor.Core.ViewModels;

namespace SweetTailor.App.Views;

public partial class CatalogModelFormPage : ContentPage
{
    private readonly CatalogModelFormViewModel _viewModel;

    public CatalogModelFormPage(CatalogModelFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _viewModel.ModelSaved += OnModelSaved;
    }

    private async void OnBackTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnPickImageTapped(object? sender, EventArgs e)
    {
        try
        {
            var imagePath = await ImageFilePicker.PickAndSaveImageAsync("Choisir la photo du modèle");
            if (imagePath != null)
            {
                _viewModel.ImagePath = imagePath;
            }
        }
        catch (IOException ex)
        {
            _viewModel.ShowToast($"Impossible de lire l’image : {ex.Message}", ToastType.Error);
        }
        catch (UnauthorizedAccessException ex)
        {
            _viewModel.ShowToast($"Accès à l’image refusé : {ex.Message}", ToastType.Error);
        }
    }

    private async void OnModelSaved(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
