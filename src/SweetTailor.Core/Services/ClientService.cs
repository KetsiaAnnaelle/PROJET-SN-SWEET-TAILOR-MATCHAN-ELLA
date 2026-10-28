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
    Task<bool> DeleteClientAsync(int id);
    Task<ClientNote> AddNoteAsync(int clientId, string content, bool isImportant = false);
    Task<bool> DeleteNoteAsync(int clientId, int noteId);
    Task<bool> UpdateMeasurementsAsync(int clientId, List<MeasurementItem> measurements);
    Task<int> GetClientsCountAsync();
    Task<int> GetActiveDeliveriesCountAsync();
}

public class ClientService : IClientService
{
    private readonly List<Client> _clients;
    private readonly IDemoModeService _demoMode;
    private readonly IAppDataStore? _dataStore;
    private readonly IOrderService? _orderService;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private bool _isInitialized;

    public ClientService(IDemoModeService demoMode, List<Client>? seed = null, IAppDataStore? dataStore = null, IOrderService? orderService = null)
    {
        _demoMode = demoMode;
        _clients = seed ?? MockDataSeeder.GetInitialClients();
        _dataStore = dataStore;
        _orderService = orderService;
    }

    // Secondary constructor for tests without DI
    public ClientService(List<Client>? seed = null) : this(new DemoModeService(), seed)
    {
    }

    public async Task<List<Client>> GetClientsAsync()
    {
        await EnsureInitializedAsync();
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
        await EnsureInitializedAsync();
        await _demoMode.SimulateDelayAsync();
        return _clients.FirstOrDefault(c => c.Id == id);
    }

    public async Task<List<Client>> SearchClientsAsync(string query, string filterChip = "Tous")
    {
        await EnsureInitializedAsync();
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
                                       c.Address.ToLowerInvariant().Contains(q) ||
                                       c.CurrentOrderSummary.ToLowerInvariant().Contains(q));
        }

        result = NormalizeFilter(filterChip) switch
        {
            "En cours" => result.Where(c => c.ActiveOrdersCount > 0),
            "À livrer" => result.Where(c => c.CurrentOrderStatus == OrderStatus.Prete || c.CurrentOrderStatus == OrderStatus.Essayage),
            "Livrés" => result.Where(c => c.CurrentOrderStatus == OrderStatus.Livree),
            _ => result
        };

        return result.ToList();
    }

    public async Task<Client> AddClientAsync(Client client)
    {
        ArgumentNullException.ThrowIfNull(client);
        ValidateClient(client);
        await EnsureInitializedAsync();
        await _writeLock.WaitAsync();
        try
        {
            client.Id = _clients.Any() ? _clients.Max(c => c.Id) + 1 : 1;
            client.CreatedAt = DateTime.Now;
            client.AvatarInitials = Client.ComputeInitials(client.FullName);
            if (client.Measurements.Count == 0)
            {
                client.Measurements = MockDataSeeder.CreateBlankMeasurements();
            }
            EnsureInitialNote(client);
            _clients.Insert(0, client);
            await SaveAsync();
            return client;
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task<bool> UpdateClientAsync(Client client)
    {
        ArgumentNullException.ThrowIfNull(client);
        ValidateClient(client);
        await EnsureInitializedAsync();
        await _writeLock.WaitAsync();
        try
        {
            var idx = _clients.FindIndex(c => c.Id == client.Id);
            if (idx < 0)
            {
                return false;
            }

            var existing = _clients[idx];
            client.AvatarInitials = Client.ComputeInitials(client.FullName);
            if (client.Measurements.Count == 0)
            {
                client.Measurements = existing.Measurements;
            }
            if (client.Notes.Count == 0)
            {
                client.Notes = existing.Notes;
            }
            if (string.IsNullOrWhiteSpace(client.PhotoPath))
            {
                client.PhotoPath = existing.PhotoPath;
            }
            client.CreatedAt = existing.CreatedAt;
            _clients[idx] = client;
            await SaveAsync();
            return true;
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task<bool> DeleteClientAsync(int id)
    {
        await EnsureInitializedAsync();
        await _writeLock.WaitAsync();
        try
        {
            var client = _clients.FirstOrDefault(c => c.Id == id);
            if (client == null)
            {
                return false;
            }

            if (_orderService != null)
            {
                await _orderService.DeleteOrdersForClientAsync(id);
            }

            _clients.Remove(client);
            await SaveAsync();
            return true;
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task<ClientNote> AddNoteAsync(int clientId, string content, bool isImportant = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        await EnsureInitializedAsync();
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
        await PersistChangesAsync();
        return note;
    }

    public async Task<bool> DeleteNoteAsync(int clientId, int noteId)
    {
        await EnsureInitializedAsync();
        var client = _clients.FirstOrDefault(c => c.Id == clientId);
        if (client == null) return false;

        var note = client.Notes.FirstOrDefault(n => n.Id == noteId);
        if (note == null) return false;

        client.Notes.Remove(note);
        await PersistChangesAsync();
        return true;
    }

    public async Task<bool> UpdateMeasurementsAsync(int clientId, List<MeasurementItem> measurements)
    {
        ArgumentNullException.ThrowIfNull(measurements);
        await EnsureInitializedAsync();
        var client = _clients.FirstOrDefault(c => c.Id == clientId);
        if (client != null)
        {
            client.Measurements = measurements;
            await PersistChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<int> GetClientsCountAsync()
    {
        await EnsureInitializedAsync();
        return _clients.Count;
    }

    public async Task<int> GetActiveDeliveriesCountAsync()
    {
        await EnsureInitializedAsync();
        return _clients.Count(c => c.CurrentOrderStatus == OrderStatus.Prete || c.CurrentOrderStatus == OrderStatus.Essayage);
    }

    private async Task EnsureInitializedAsync()
    {
        if (_isInitialized || _dataStore == null)
        {
            _isInitialized = true;
            return;
        }

        await _initializationLock.WaitAsync();
        try
        {
            if (_isInitialized)
            {
                return;
            }

            var storedClients = await _dataStore.LoadClientsAsync();
            if (storedClients != null)
            {
                _clients.Clear();
                _clients.AddRange(storedClients);
            }
            _isInitialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    private async Task PersistChangesAsync()
    {
        if (_dataStore == null)
        {
            return;
        }

        await _writeLock.WaitAsync();
        try
        {
            await SaveAsync();
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private Task SaveAsync() => _dataStore?.SaveClientsAsync(_clients) ?? Task.CompletedTask;

    private static void ValidateClient(Client client)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(client.FullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(client.PhoneNumber);
    }

    public static string NormalizeFilter(string filterChip)
    {
        var chip = filterChip.Split(['·', '•'], 2)[0].Trim();
        return chip;
    }

    private static void EnsureInitialNote(Client client)
    {
        if (string.IsNullOrWhiteSpace(client.NotesSummary) || client.Notes.Count > 0)
        {
            return;
        }

        client.Notes.Add(new ClientNote
        {
            Id = 1,
            ClientId = client.Id,
            Content = client.NotesSummary.Trim(),
            Date = DateTime.Now
        });
    }
}
