using SweetTailor.Core.Models;

namespace SweetTailor.Core.Services;

public interface ICatalogService
{
    Task<List<CatalogModel>> GetCatalogAsync(string categoryFilter = "Tout", string searchQuery = "", string audienceFilter = "Tout");
    Task<CatalogModel?> GetModelByIdAsync(int id);
    Task<CatalogModel> AddModelAsync(CatalogModel model);
    Task<bool> ToggleFavoriteAsync(int modelId);
    Task<List<CatalogModel>> GetSimilarModelsAsync(int modelId);
}

public class CatalogService : ICatalogService
{
    private readonly List<CatalogModel> _models;
    private readonly IDemoModeService _demoMode;

    public CatalogService(IDemoModeService demoMode, List<CatalogModel>? seed = null)
    {
        _demoMode = demoMode;
        _models = seed ?? MockDataSeeder.GetInitialCatalog();
    }

    public CatalogService(List<CatalogModel>? seed = null) : this(new DemoModeService(), seed)
    {
    }

    public async Task<List<CatalogModel>> GetCatalogAsync(string categoryFilter = "Tout", string searchQuery = "", string audienceFilter = "Tout")
    {
        await _demoMode.SimulateDelayAsync();
        if (_demoMode.CurrentState == DemoListState.Empty)
        {
            return new List<CatalogModel>();
        }

        var result = _models.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var q = searchQuery.Trim().ToLowerInvariant();
            result = result.Where(m => m.Title.ToLowerInvariant().Contains(q) ||
                                       m.Category.ToLowerInvariant().Contains(q) ||
                                       m.Subtitle.ToLowerInvariant().Contains(q));
        }

        if (!string.IsNullOrWhiteSpace(categoryFilter) && categoryFilter != "Tout")
        {
            result = result.Where(m => m.Category.Equals(categoryFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(audienceFilter) && audienceFilter != "Tout")
        {
            result = result.Where(m => m.Audience.Equals(audienceFilter, StringComparison.OrdinalIgnoreCase));
        }

        return result.ToList();
    }

    public async Task<CatalogModel?> GetModelByIdAsync(int id)
    {
        await _demoMode.SimulateDelayAsync();
        return _models.FirstOrDefault(m => m.Id == id);
    }

    public Task<CatalogModel> AddModelAsync(CatalogModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        ValidateModel(model);
        model.Id = _models.Count == 0 ? 1 : _models.Max(item => item.Id) + 1;
        model.Title = model.Title.Trim();
        model.Category = model.Category.Trim();
        model.Audience = model.Audience.Trim();
        model.Subtitle = model.Subtitle?.Trim() ?? string.Empty;
        _models.Add(model);
        return Task.FromResult(model);
    }

    public Task<bool> ToggleFavoriteAsync(int modelId)
    {
        var item = _models.FirstOrDefault(m => m.Id == modelId);
        if (item == null) return Task.FromResult(false);

        item.IsFavorite = !item.IsFavorite;
        return Task.FromResult(item.IsFavorite);
    }

    public Task<List<CatalogModel>> GetSimilarModelsAsync(int modelId)
    {
        var current = _models.FirstOrDefault(m => m.Id == modelId);
        if (current == null) return Task.FromResult(new List<CatalogModel>());

        var similar = _models.Where(m => m.Id != modelId &&
                                         (current.SimilarModelIds.Contains(m.Id) || m.Category == current.Category))
                             .Take(3)
                             .ToList();
        return Task.FromResult(similar);
    }

    private static void ValidateModel(CatalogModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Title) || model.Title.Trim().Length is < 2 or > 120)
        {
            throw new ArgumentException("Le nom du modèle doit contenir entre 2 et 120 caractères.");
        }
        if (string.IsNullOrWhiteSpace(model.Category) || model.Category.Trim().Length > 60)
        {
            throw new ArgumentException("La catégorie du modèle est obligatoire (60 caractères maximum).");
        }
        if (model.Audience is not ("Homme" or "Femme" or "Enfant"))
        {
            throw new ArgumentException("Choisissez une catégorie : homme, femme ou enfant.");
        }
        if (model.Subtitle?.Length > 300 || string.IsNullOrWhiteSpace(model.ImagePath))
        {
            throw new ArgumentException("Ajoutez une image et une description de 300 caractères maximum.");
        }
    }
}
