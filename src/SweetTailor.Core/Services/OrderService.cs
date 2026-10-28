using SweetTailor.Core.Models;

namespace SweetTailor.Core.Services;

public sealed class OrderPaymentRequiredException()
    : InvalidOperationException("Le client doit régler le solde avant de passer la commande à « Prête ».") { }

public interface IOrderService
{
    Task<List<Order>> GetAllOrdersAsync();
    Task<List<Order>> SearchOrdersAsync(string query);
    Task<List<Order>> GetOrdersByFilterAsync(string statusFilter);
    Task<Order?> GetOrderByIdAsync(int id);
    Task<List<Order>> GetOrdersForClientAsync(int clientId);
    Task<Order> CreateOrderAsync(Order order);
    Task<bool> UpdateOrderAsync(Order order);
    Task<bool> DeleteOrderAsync(int orderId);
    Task<bool> DeleteOrdersForClientAsync(int clientId);
    Task<bool> AdvanceOrderStatusAsync(int orderId);
    Task<int> GetActiveOrdersCountAsync();
}

public class OrderService : IOrderService
{
    private readonly List<Order> _orders;
    private readonly IDemoModeService _demoMode;
    private readonly IAppDataStore? _dataStore;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private bool _isInitialized;

    public OrderService(IDemoModeService demoMode, List<Order>? seed = null, IAppDataStore? dataStore = null)
    {
        _demoMode = demoMode;
        _orders = seed ?? MockDataSeeder.GetInitialOrders();
        _dataStore = dataStore;
    }

    public OrderService(List<Order>? seed = null) : this(new DemoModeService(), seed)
    {
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        await EnsureInitializedAsync();
        await _demoMode.SimulateDelayAsync();
        if (_demoMode.CurrentState == DemoListState.Empty)
        {
            return new List<Order>();
        }

        return _orders.OrderBy(o => o.DueDate).ToList();
    }

    public async Task<List<Order>> SearchOrdersAsync(string query)
    {
        var orders = await GetAllOrdersAsync();
        if (string.IsNullOrWhiteSpace(query))
        {
            return orders;
        }

        var q = query.Trim();
        return orders.Where(order =>
            order.ModelName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            order.ClientName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            order.Fabric.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            order.OrderNumber.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<List<Order>> GetOrdersByFilterAsync(string statusFilter)
    {
        await EnsureInitializedAsync();
        await _demoMode.SimulateDelayAsync();
        if (_demoMode.CurrentState == DemoListState.Empty)
        {
            return new List<Order>();
        }

        var result = _orders.AsEnumerable();

        result = statusFilter switch
        {
            "Reçues" => result.Where(o => o.Status == OrderStatus.Recue),
            "Couture" => result.Where(o => o.Status == OrderStatus.Couture),
            "Essayage" => result.Where(o => o.Status == OrderStatus.Essayage),
            "Prêtes" => result.Where(o => o.Status == OrderStatus.Prete),
            "Livrée" or "Livrées" => result.Where(o => o.Status == OrderStatus.Livree),
            _ => result // "Toutes"
        };

        return result.OrderBy(o => o.DueDate).ToList();
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        await EnsureInitializedAsync();
        await _demoMode.SimulateDelayAsync();
        var order = _orders.FirstOrDefault(o => o.Id == id);
        return order;
    }

    public async Task<List<Order>> GetOrdersForClientAsync(int clientId)
    {
        await EnsureInitializedAsync();
        await _demoMode.SimulateDelayAsync();
        var orders = _orders.Where(o => o.ClientId == clientId).OrderByDescending(o => o.DueDate).ToList();
        return orders;
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        ValidateOrder(order);
        await EnsureInitializedAsync();
        await _writeLock.WaitAsync();
        try
        {
            order.Id = _orders.Any() ? _orders.Max(o => o.Id) + 1 : 101;

            if (string.IsNullOrWhiteSpace(order.OrderNumber))
            {
                order.OrderNumber = $"#{order.Id:D4}";
            }

            if (order.Steps.Count == 0)
            {
                order.Steps = CreateDefaultSteps();
            }

            _orders.Insert(0, order);
            await SaveAsync();
            return order;
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task<bool> UpdateOrderAsync(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        ValidateOrder(order);
        await EnsureInitializedAsync();
        await _writeLock.WaitAsync();
        try
        {
            var index = _orders.FindIndex(existing => existing.Id == order.Id);
            if (index < 0)
            {
                return false;
            }

            EnsureBalanceIsSettled(_orders[index], order.Status);
            _orders[index] = order;
            await SaveAsync();
            return true;
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task<bool> DeleteOrderAsync(int orderId)
    {
        await EnsureInitializedAsync();
        await _writeLock.WaitAsync();
        try
        {
            var order = _orders.FirstOrDefault(existing => existing.Id == orderId);
            if (order == null)
            {
                return false;
            }

            _orders.Remove(order);
            await SaveAsync();
            return true;
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task<bool> DeleteOrdersForClientAsync(int clientId)
    {
        await EnsureInitializedAsync();
        await _writeLock.WaitAsync();
        try
        {
            var removed = _orders.RemoveAll(order => order.ClientId == clientId);
            if (removed == 0)
            {
                return false;
            }

            await SaveAsync();
            return true;
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task<bool> AdvanceOrderStatusAsync(int orderId)
    {
        await EnsureInitializedAsync();
        await _writeLock.WaitAsync();
        try
        {
            var order = _orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null) return false;
            EnsureBalanceIsSettled(order, order.Status == OrderStatus.Essayage
                ? OrderStatus.Prete
                : order.Status);

            switch (order.Status)
            {
                case OrderStatus.Recue:
                    order.Status = OrderStatus.Couture;
                    UpdateSteps(order, 1);
                    break;
                case OrderStatus.Couture:
                    order.Status = OrderStatus.Essayage;
                    UpdateSteps(order, 2);
                    break;
                case OrderStatus.Essayage:
                    order.Status = OrderStatus.Prete;
                    UpdateSteps(order, 3);
                    break;
                case OrderStatus.Prete:
                    order.Status = OrderStatus.Livree;
                    break;
            }

            await SaveAsync();
            return true;
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private static void UpdateSteps(Order order, int activeStepIndex)
    {
        for (int i = 0; i < order.Steps.Count; i++)
        {
            if (i < activeStepIndex)
            {
                order.Steps[i].State = StepState.Done;
            }
            else if (i == activeStepIndex)
            {
                order.Steps[i].State = StepState.Now;
                order.Steps[i].Subtitle = "En cours";
            }
            else
            {
                order.Steps[i].State = StepState.Todo;
            }
        }
    }

    private static void EnsureBalanceIsSettled(Order order, OrderStatus nextStatus)
    {
        if (order.Status == OrderStatus.Essayage &&
            nextStatus == OrderStatus.Prete &&
            order.RemainingPayment > 0)
        {
            throw new OrderPaymentRequiredException();
        }
    }

    public async Task<int> GetActiveOrdersCountAsync()
    {
        await EnsureInitializedAsync();
        return _orders.Count(o => o.Status != OrderStatus.Livree);
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

            var storedOrders = await _dataStore.LoadOrdersAsync();
            if (storedOrders != null)
            {
                _orders.Clear();
                _orders.AddRange(storedOrders);
            }
            _isInitialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    private Task SaveAsync() => _dataStore?.SaveOrdersAsync(_orders) ?? Task.CompletedTask;

    private static List<OrderStep> CreateDefaultSteps() =>
    [
        new() { Title = "Commande reçue", Subtitle = DateTime.Now.ToString("d MMM", new System.Globalization.CultureInfo("fr-FR")), State = StepState.Done },
        new() { Title = "Coupe et couture", Subtitle = "En cours", State = StepState.Now },
        new() { Title = "Essayage client", Subtitle = string.Empty, State = StepState.Todo },
        new() { Title = "Prête à retirer", Subtitle = string.Empty, State = StepState.Todo }
    ];

    private static void ValidateOrder(Order order)
    {
        if (order.ClientId <= 0)
        {
            throw new ArgumentException("Un client doit être sélectionné pour la commande.", nameof(order));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(order.ModelName);
        if (order.TotalPrice < 0 || order.AdvancePayment < 0 || order.AdvancePayment > order.TotalPrice)
        {
            throw new ArgumentException("Le prix et l'acompte doivent être positifs, et l'acompte ne peut pas dépasser le prix total.", nameof(order));
        }
    }
}
