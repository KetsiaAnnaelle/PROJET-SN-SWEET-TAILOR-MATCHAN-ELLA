using SweetTailor.Core.Models;

namespace SweetTailor.Core.Services;

public interface ICatalogService
{
    Task<List<CatalogModel>> GetCatalogAsync(string categoryFilter = "Tout", string searchQuery = "");
    Task<CatalogModel?> GetModelByIdAsync(int id);
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

    public async Task<List<CatalogModel>> GetCatalogAsync(string categoryFilter = "Tout", string searchQuery = "")
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

        return result.ToList();
    }

    public async Task<CatalogModel?> GetModelByIdAsync(int id)
    {
        await _demoMode.SimulateDelayAsync();
        return _models.FirstOrDefault(m => m.Id == id);
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
}
