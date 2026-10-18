using SweetTailor.Core.Models;

namespace SweetTailor.Core.Services;

public interface IClientService
{
    Task<List<Client>> GetClientsAsync();
    Task<List<Client>> GetAllClientsAsync();
    Task<Client?> GetClientByIdAsync(int id);
    Task<List<Client>> SearchClientsAsync(string query, string filterChip = "Tous");
    Task<Client> AddClientAsync(Client client);
    Task<bool> UpdateClientAsync(Client client);
    Task<ClientNote> AddNoteAsync(int clientId, string content, bool isImportant = false);
    Task<bool> UpdateMeasurementsAsync(int clientId, List<MeasurementItem> measurements);
    Task<int> GetClientsCountAsync();
    Task<int> GetActiveDeliveriesCountAsync();
}

public class ClientService : IClientService
{
    private readonly List<Client> _clients;
    private readonly IDemoModeService _demoMode;

    public ClientService(IDemoModeService demoMode, List<Client>? seed = null)
    {
        _demoMode = demoMode;
        _clients = seed ?? MockDataSeeder.GetInitialClients();
    }

    // Secondary constructor for tests without DI
    public ClientService(List<Client>? seed = null) : this(new DemoModeService(), seed)
    {
    }

    public async Task<List<Client>> GetClientsAsync()
    {
        await _demoMode.SimulateDelayAsync();
        if (_demoMode.CurrentState == DemoListState.Empty)
        {
            return new List<Client>();
        }
        return _clients.OrderByDescending(c => c.CreatedAt).ToList();
    }

    public Task<List<Client>> GetAllClientsAsync() => GetClientsAsync();

    public async Task<Client?> GetClientByIdAsync(int id)
    {
        await _demoMode.SimulateDelayAsync();
        return _clients.FirstOrDefault(c => c.Id == id);
    }

    public async Task<List<Client>> SearchClientsAsync(string query, string filterChip = "Tous")
    {
        await _demoMode.SimulateDelayAsync();
        if (_demoMode.CurrentState == DemoListState.Empty)
        {
            return new List<Client>();
        }

        var result = _clients.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.Trim().ToLowerInvariant();
            result = result.Where(c => c.FullName.ToLowerInvariant().Contains(q) ||
                                       c.PhoneNumber.Contains(q) ||
                                       c.CurrentOrderSummary.ToLowerInvariant().Contains(q));
        }

        result = filterChip switch
        {
            "En cours · 12" or "En cours" => result.Where(c => c.ActiveOrdersCount > 0),
            "À livrer · 5" or "À livrer" => result.Where(c => c.CurrentOrderStatus == OrderStatus.Prete || c.CurrentOrderStatus == OrderStatus.Essayage),
            "Livrés" => result.Where(c => c.CurrentOrderStatus == OrderStatus.Livree),
            _ => result
        };

        return result.ToList();
    }

    public async Task<Client> AddClientAsync(Client client)
    {
        await Task.Delay(800);
        client.Id = _clients.Any() ? _clients.Max(c => c.Id) + 1 : 1;
        client.CreatedAt = DateTime.Now;
        _clients.Insert(0, client);
        return client;
    }

    public async Task<bool> UpdateClientAsync(Client client)
    {
        await Task.Delay(500);
        var idx = _clients.FindIndex(c => c.Id == client.Id);
        if (idx >= 0)
        {
            _clients[idx] = client;
            return true;
        }
        return false;
    }

    public async Task<ClientNote> AddNoteAsync(int clientId, string content, bool isImportant = false)
    {
        await Task.Delay(500);
        var client = _clients.FirstOrDefault(c => c.Id == clientId);
        if (client == null) throw new InvalidOperationException("Client non trouvé");

        var note = new ClientNote
        {
            Id = client.Notes.Any() ? client.Notes.Max(n => n.Id) + 1 : 1,
            ClientId = clientId,
            Date = DateTime.Now,
            Content = content,
            IsImportant = isImportant
        };
        client.Notes.Insert(0, note);
        return note;
    }

    public async Task<bool> UpdateMeasurementsAsync(int clientId, List<MeasurementItem> measurements)
    {
        await Task.Delay(600);
        var client = _clients.FirstOrDefault(c => c.Id == clientId);
        if (client != null)
        {
            client.Measurements = measurements;
            return true;
        }
        return false;
    }

    public Task<int> GetClientsCountAsync() => Task.FromResult(_clients.Count);

    public Task<int> GetActiveDeliveriesCountAsync() =>
        Task.FromResult(_clients.Count(c => c.CurrentOrderStatus == OrderStatus.Prete || c.CurrentOrderStatus == OrderStatus.Essayage));
}
