using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SweetTailor.Core.Models;
using SweetTailor.Core.Services;

namespace SweetTailor.Core.ViewModels;

public partial class CatalogViewModel : BaseViewModel
{
    private readonly ICatalogService _catalogService;

    [ObservableProperty]
    private string _modelCountLabel = "248 modèles";

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _selectedCategory = "Tout";

    [ObservableProperty]
    private string _selectedAudience = "Tout";

    [ObservableProperty]
    private bool _showFavoritesOnly;

    public List<string> Categories { get; } = new()
    {
        "Tout",
        "Robes",
        "Pagne",
        "Costumes",
        "Mariage",
        "Soirée",
        "Traditionnel"
    };

    public List<string> Audiences { get; } = new()
    {
        "Tout",
        "Femme",
        "Homme",
        "Enfant"
    };

    public ObservableCollection<CatalogModel> Column1Models { get; } = new();
    public ObservableCollection<CatalogModel> Column2Models { get; } = new();

    public CatalogViewModel(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [RelayCommand]
    public async Task LoadCatalogAsync()
    {
        var models = await _catalogService.GetCatalogAsync(SelectedCategory, SearchQuery, SelectedAudience);
        if (ShowFavoritesOnly)
        {
            models = models.Where(model => model.IsFavorite).ToList();
        }

        Column1Models.Clear();
        Column2Models.Clear();
        ModelCountLabel = $"{models.Count} modèle{(models.Count == 1 ? string.Empty : "s")}";

        for (int i = 0; i < models.Count; i++)
        {
            if (i % 2 == 0)
                Column1Models.Add(models[i]);
            else
                Column2Models.Add(models[i]);
        }
    }

    [RelayCommand]
    public async Task SelectCategoryAsync(string category)
    {
        SelectedCategory = category;
        await LoadCatalogAsync();
    }

    [RelayCommand]
    public async Task SelectAudienceAsync(string audience)
    {
        SelectedAudience = audience;
        await LoadCatalogAsync();
    }

    [RelayCommand]
    public async Task ToggleFavoriteAsync(CatalogModel model)
    {
        model.IsFavorite = await _catalogService.ToggleFavoriteAsync(model.Id);
    }

    [RelayCommand]
    public async Task ToggleFavoritesOnlyAsync()
    {
        ShowFavoritesOnly = !ShowFavoritesOnly;
        await LoadCatalogAsync();
    }
}

public partial class CatalogDetailViewModel : BaseViewModel
{
    private readonly ICatalogService _catalogService;

    [ObservableProperty]
    private CatalogModel? _model;

    public ObservableCollection<CatalogModel> SimilarModels { get; } = new();

    public CatalogDetailViewModel(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    public async Task LoadModelAsync(int modelId)
    {
        Model = await _catalogService.GetModelByIdAsync(modelId);
        if (Model == null) return;

        SimilarModels.Clear();
        var similar = await _catalogService.GetSimilarModelsAsync(modelId);
        foreach (var m in similar)
        {
            SimilarModels.Add(m);
        }
    }

    [RelayCommand]
    public async Task ToggleFavoriteAsync()
    {
        if (Model == null) return;
        Model.IsFavorite = await _catalogService.ToggleFavoriteAsync(Model.Id);
        OnPropertyChanged(nameof(Model));
    }
}

public partial class CatalogModelFormViewModel : BaseViewModel
{
    private readonly ICatalogService _catalogService;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _category = "Robes";

    [ObservableProperty]
    private string _audience = "Femme";

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasImage))]
    private string _imagePath = string.Empty;

    public bool HasImage => !string.IsNullOrWhiteSpace(ImagePath);
    public List<string> Categories { get; } = ["Robes", "Pagne", "Costumes", "Mariage", "Traditionnel"];
    public List<string> Audiences { get; } = ["Femme", "Homme", "Enfant"];
    public event EventHandler? ModelSaved;

    public CatalogModelFormViewModel(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Title) || Title.Trim().Length is < 2 or > 120)
        {
            ShowToast("Le nom du modèle doit contenir entre 2 et 120 caractères.", ToastType.Error);
            return;
        }
        if (string.IsNullOrWhiteSpace(Category) || Description.Trim().Length > 300)
        {
            ShowToast("Vérifiez la catégorie et la description du modèle (300 caractères maximum).", ToastType.Error);
            return;
        }
        if (Audience is not ("Homme" or "Femme" or "Enfant"))
        {
            ShowToast("Choisissez homme, femme ou enfant comme catégorie.", ToastType.Error);
            return;
        }
        if (string.IsNullOrWhiteSpace(ImagePath))
        {
            ShowToast("Ajoutez une image au modèle avant de l’enregistrer.", ToastType.Error);
            return;
        }

        IsBusy = true;
        try
        {
            await _catalogService.AddModelAsync(new CatalogModel
            {
                Title = Title.Trim(),
                Category = Category,
                Audience = Audience,
                Subtitle = Description.Trim(),
                SvgAsset = "dress",
                ImagePath = ImagePath,
                CardHeight = 180
            });
            ShowToast("Modèle ajouté au catalogue.", ToastType.Success);
            ModelSaved?.Invoke(this, EventArgs.Empty);
        }
        catch (HttpRequestException ex)
        {
            ShowToast($"Impossible d’enregistrer le modèle : {ex.Message}", ToastType.Error);
        }
        catch (ArgumentException ex)
        {
            ShowToast(ex.Message, ToastType.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public partial class SearchViewModel : BaseViewModel
{
    private readonly IClientService _clientService;
    private readonly IOrderService _orderService;

    [ObservableProperty]
    private string _query = string.Empty;

    [ObservableProperty]
    private string _selectedFilter = "Tout";

    public List<string> FilterChips { get; } = new()
    {
        "Tout",
        "Clients",
        "Commandes",
        "Statut ▾"
    };

    public List<string> RecentSearches { get; } = new()
    {
        "Kaba",
        "Costume",
        "Tchoua"
    };

    public ObservableCollection<Client> ClientResults { get; } = new();
    public ObservableCollection<Order> OrderResults { get; } = new();

    public bool HasNoResults => !string.IsNullOrWhiteSpace(Query) && ClientResults.Count == 0 && OrderResults.Count == 0;

    public SearchViewModel(IClientService clientService, IOrderService orderService)
    {
        _clientService = clientService;
        _orderService = orderService;
    }

    [RelayCommand]
    public async Task SearchAsync()
    {
        ClientResults.Clear();
        OrderResults.Clear();

        if (string.IsNullOrWhiteSpace(Query))
        {
            OnPropertyChanged(nameof(HasNoResults));
            return;
        }

        if (SelectedFilter is "Tout" or "Clients")
        {
            foreach (var c in await _clientService.SearchClientsAsync(Query))
            {
                ClientResults.Add(c);
            }
        }

        if (SelectedFilter is "Tout" or "Commandes")
        {
            foreach (var o in await _orderService.SearchOrdersAsync(Query))
            {
                OrderResults.Add(o);
            }
        }

        OnPropertyChanged(nameof(HasNoResults));
    }

    [RelayCommand]
    public void ClearQuery()
    {
        Query = string.Empty;
        ClientResults.Clear();
        OrderResults.Clear();
        OnPropertyChanged(nameof(HasNoResults));
    }

    [RelayCommand]
    public async Task SelectRecentSearchAsync(string term)
    {
        Query = term;
        await SearchAsync();
    }
}
