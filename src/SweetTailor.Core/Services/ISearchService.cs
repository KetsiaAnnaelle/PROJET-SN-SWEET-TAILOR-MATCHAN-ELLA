using SweetTailor.Core.Models;

namespace SweetTailor.Core.Services;

public class SearchResults
{
    public List<Client> Clients { get; set; } = new();
    public List<Order> Orders { get; set; } = new();
    public bool HasResults => Clients.Count > 0 || Orders.Count > 0;
}

public interface ISearchService
{
    Task<SearchResults> SearchAsync(string query, string category = "Tout");
    List<string> GetRecentSearches();
    void AddRecentSearch(string query);
}

public class SearchService : ISearchService
{
    private readonly IClientService _clientService;
    private readonly IOrderService _orderService;
    private readonly IDemoModeService _demoMode;
    private readonly List<string> _recentSearches = new() { "Kaba", "Costume", "Tchoua" };

    public SearchService(IClientService clientService, IOrderService orderService, IDemoModeService demoMode)
    {
        _clientService = clientService;
        _orderService = orderService;
        _demoMode = demoMode;
    }

    public async Task<SearchResults> SearchAsync(string query, string category = "Tout")
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new SearchResults();
        }

        await _demoMode.SimulateDelayAsync();

        if (_demoMode.CurrentState == DemoListState.Empty || query.Trim().Equals("Zzz", StringComparison.OrdinalIgnoreCase))
        {
            return new SearchResults();
        }

        var results = new SearchResults();
        var q = query.Trim().ToLowerInvariant();

        if (category == "Tout" || category == "Clients")
        {
            var clients = await _clientService.GetClientsAsync();
            results.Clients = clients.Where(c =>
                c.FullName.ToLowerInvariant().Contains(q) ||
                c.PhoneNumber.Contains(q) ||
                c.Address.ToLowerInvariant().Contains(q) ||
                c.CurrentOrderSummary.ToLowerInvariant().Contains(q)
            ).ToList();
        }

        if (category == "Tout" || category == "Commandes")
        {
            var orders = await _orderService.GetAllOrdersAsync();
            results.Orders = orders.Where(o =>
                o.ModelName.ToLowerInvariant().Contains(q) ||
                o.ClientName.ToLowerInvariant().Contains(q) ||
                o.Fabric.ToLowerInvariant().Contains(q)
            ).ToList();
        }

        return results;
    }

    public List<string> GetRecentSearches() => new(_recentSearches);

    public void AddRecentSearch(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return;
        _recentSearches.Remove(query);
        _recentSearches.Insert(0, query);
        if (_recentSearches.Count > 6)
        {
            _recentSearches.RemoveAt(_recentSearches.Count - 1);
        }
    }
}
