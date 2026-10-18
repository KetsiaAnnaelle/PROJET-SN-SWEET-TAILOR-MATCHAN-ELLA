using SweetTailor.Core.Models;

namespace SweetTailor.Core.Services;

public interface IOrderService
{
    Task<List<Order>> GetAllOrdersAsync();
    Task<List<Order>> GetOrdersByFilterAsync(string statusFilter);
    Task<Order?> GetOrderByIdAsync(int id);
    Task<List<Order>> GetOrdersForClientAsync(int clientId);
    Task<Order> CreateOrderAsync(Order order);
    Task<bool> AdvanceOrderStatusAsync(int orderId);
    Task<int> GetActiveOrdersCountAsync();
}

public class OrderService : IOrderService
{
    private readonly List<Order> _orders;
    private readonly IDemoModeService _demoMode;

    public OrderService(IDemoModeService demoMode, List<Order>? seed = null)
    {
        _demoMode = demoMode;
        _orders = seed ?? MockDataSeeder.GetInitialOrders();
    }

    public OrderService(List<Order>? seed = null) : this(new DemoModeService(), seed)
    {
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        await _demoMode.SimulateDelayAsync();
        if (_demoMode.CurrentState == DemoListState.Empty)
        {
            return new List<Order>();
        }
        return _orders.OrderBy(o => o.DueDate).ToList();
    }

    public async Task<List<Order>> GetOrdersByFilterAsync(string statusFilter)
    {
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
        await _demoMode.SimulateDelayAsync();
        var order = _orders.FirstOrDefault(o => o.Id == id);
        return order;
    }

    public async Task<List<Order>> GetOrdersForClientAsync(int clientId)
    {
        await _demoMode.SimulateDelayAsync();
        var orders = _orders.Where(o => o.ClientId == clientId).OrderByDescending(o => o.DueDate).ToList();
        return orders;
    }

    public Task<Order> CreateOrderAsync(Order order)
    {
        if (order.Id <= 0)
        {
            order.Id = _orders.Any() ? _orders.Max(o => o.Id) + 1 : 101;
        }

        if (string.IsNullOrWhiteSpace(order.OrderNumber))
        {
            order.OrderNumber = $"#{order.Id:D4}";
        }

        if (order.Steps.Count == 0)
        {
            order.Steps = new List<OrderStep>
            {
                new() { Title = "Commande reçue", Subtitle = DateTime.Now.ToString("d MMM", new System.Globalization.CultureInfo("fr-FR")), State = StepState.Done },
                new() { Title = "Coupe et couture", Subtitle = "En cours", State = StepState.Now },
                new() { Title = "Essayage client", Subtitle = string.Empty, State = StepState.Todo },
                new() { Title = "Prête à retirer", Subtitle = string.Empty, State = StepState.Todo }
            };
        }

        _orders.Insert(0, order);
        return Task.FromResult(order);
    }

    public Task<bool> AdvanceOrderStatusAsync(int orderId)
    {
        var order = _orders.FirstOrDefault(o => o.Id == orderId);
        if (order == null) return Task.FromResult(false);

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

        return Task.FromResult(true);
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

    public Task<int> GetActiveOrdersCountAsync()
    {
        return Task.FromResult(_orders.Count(o => o.Status != OrderStatus.Livree));
    }
}
