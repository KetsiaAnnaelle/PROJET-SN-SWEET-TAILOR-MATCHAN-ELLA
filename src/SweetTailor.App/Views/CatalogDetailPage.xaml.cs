using SweetTailor.Core.ViewModels;

namespace SweetTailor.App.Views;

[QueryProperty(nameof(ModelId), "ModelId")]
public partial class CatalogDetailPage : ContentPage
{
    private readonly CatalogDetailViewModel _viewModel;
    private int _modelId = 1;

    public string ModelId
    {
        set
        {
            if (int.TryParse(value, out var id))
            {
                _modelId = id;
            }
        }
    }

    public CatalogDetailPage(CatalogDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadModelAsync(_modelId);
    }

    private async void OnBackTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnShareTapped(object? sender, EventArgs e)
    {
        if (_viewModel.Model != null)
        {
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Title = _viewModel.Model.Title,
                Text = $"Regarde ce modèle sur Sweet Tailor : {_viewModel.Model.Title} ({_viewModel.Model.Category})",
                Uri = "https://sweettailor.app"
            });
        }
    }

    private async void OnCreateOrderClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(NewOrderPage));
    }
}
