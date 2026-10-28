using SweetTailor.Core.Models;
using SweetTailor.Core.ViewModels;

namespace SweetTailor.App.Views;

public partial class CatalogPage : ContentPage
{
    private readonly CatalogViewModel _viewModel;

    public CatalogPage(CatalogViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        FavoriteFilterIcon.Fill = _viewModel.ShowFavoritesOnly
            ? Color.FromArgb("#E8607F")
            : Colors.Transparent;
        FavoriteFilterIcon.Stroke = _viewModel.ShowFavoritesOnly
            ? Color.FromArgb("#E8607F")
            : Color.FromArgb("#6B7094");
        UpdateChipSelection(AudienceFilterChips, _viewModel.SelectedAudience);
        UpdateChipSelection(StyleFilterChips, _viewModel.SelectedCategory);
        await _viewModel.LoadCatalogAsync();
    }

    private static void UpdateChipSelection(HorizontalStackLayout chips, string selectedValue)
    {
        foreach (var chip in chips.Children.OfType<Controls.FilterChip>())
        {
            chip.IsSelected = chip.Text == selectedValue;
        }
    }

    private async void OnCategoryChipClicked(object? sender, EventArgs e)
    {
        if (sender is Controls.FilterChip chip)
        {
            chip.SelectExclusively();
            await _viewModel.SelectCategoryAsync(chip.Text);
        }
    }

    private async void OnAudienceChipClicked(object? sender, EventArgs e)
    {
        if (sender is Controls.FilterChip chip)
        {
            chip.SelectExclusively();
            await _viewModel.SelectAudienceAsync(chip.Text);
        }
    }

    private async void OnAddModelTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CatalogModelFormPage));
    }

    private async void OnModelCardClicked(object? sender, EventArgs e)
    {
        if (sender is View view && view.BindingContext is CatalogModel model)
        {
            await Shell.Current.GoToAsync($"{nameof(CatalogDetailPage)}?ModelId={model.Id}");
        }
    }

    private async void OnFavoriteToggled(object? sender, EventArgs e)
    {
        if (sender is Controls.ModelCard card && card.BindingContext is CatalogModel model)
        {
            await _viewModel.ToggleFavoriteAsync(model);
            if (_viewModel.ShowFavoritesOnly && !model.IsFavorite)
            {
                await _viewModel.LoadCatalogAsync();
            }
            else
            {
                card.RefreshModel();
            }
        }
    }

    private async void OnFavoritesTapped(object? sender, EventArgs e)
    {
        await _viewModel.ToggleFavoritesOnlyAsync();
        FavoriteFilterIcon.Fill = _viewModel.ShowFavoritesOnly
            ? Color.FromArgb("#E8607F")
            : Colors.Transparent;
        FavoriteFilterIcon.Stroke = _viewModel.ShowFavoritesOnly
            ? Color.FromArgb("#E8607F")
            : Color.FromArgb("#6B7094");
    }
}
