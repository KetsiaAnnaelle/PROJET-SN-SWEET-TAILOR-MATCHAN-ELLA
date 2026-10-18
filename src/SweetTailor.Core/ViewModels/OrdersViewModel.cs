using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SweetTailor.Core.Models;
using SweetTailor.Core.Services;

namespace SweetTailor.Core.ViewModels;

public enum OrdersViewState
{
    Normal,
    Empty,
    Loading,
    NetworkError
}

public partial class OrdersViewModel : BaseViewModel
{
    private readonly IOrderService _orderService;

    [ObservableProperty]
    private OrdersViewState _viewState = OrdersViewState.Normal;

    public bool IsNormalState => ViewState == OrdersViewState.Normal;
    public bool IsEmptyState => ViewState == OrdersViewState.Empty;
    public bool IsLoadingState => ViewState == OrdersViewState.Loading;
    public bool IsErrorState => ViewState == OrdersViewState.NetworkError;

    partial void OnViewStateChanged(OrdersViewState value)
    {
        OnPropertyChanged(nameof(IsNormalState));
        OnPropertyChanged(nameof(IsEmptyState));
        OnPropertyChanged(nameof(IsLoadingState));
        OnPropertyChanged(nameof(IsErrorState));
    }

    [ObservableProperty]
    private string _activeCountLabel = "12 en cours";

    [ObservableProperty]
    private string _selectedFilter = "Toutes";

    public List<string> FilterChips { get; } = new()
    {
        "Toutes",
        "Reçues",
        "Couture",
        "Essayage",
        "Prêtes"
    };

    public ObservableCollection<Order> ThisWeekOrders { get; } = new();
    public ObservableCollection<Order> NextWeekOrders { get; } = new();

    public OrdersViewModel(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [RelayCommand]
    public async Task LoadOrdersAsync()
    {
        ViewState = OrdersViewState.Loading;
        try
        {
            var orders = await _orderService.GetOrdersByFilterAsync(SelectedFilter);
            ThisWeekOrders.Clear();
            NextWeekOrders.Clear();

            var refDate = DateTime.Today.AddDays(7);

            foreach (var order in orders)
            {
                if (order.DueDate <= refDate)
                {
                    ThisWeekOrders.Add(order);
                }
                else
                {
                    NextWeekOrders.Add(order);
                }
            }

            var activeCount = await _orderService.GetActiveOrdersCountAsync();
            ActiveCountLabel = $"{activeCount} en cours";

            ViewState = (ThisWeekOrders.Count == 0 && NextWeekOrders.Count == 0)
                ? OrdersViewState.Empty
                : OrdersViewState.Normal;
        }
        catch (Exception)
        {
            ViewState = OrdersViewState.NetworkError;
            ShowToast("Impossible de charger les commandes.", ToastType.Error);
        }
    }

    [RelayCommand]
    public async Task SelectFilterAsync(string filter)
    {
        SelectedFilter = filter;
        await LoadOrdersAsync();
    }

    [RelayCommand]
    public async Task RetryAsync()
    {
        HideToast();
        await LoadOrdersAsync();
    }

    [RelayCommand]
    public void SetNormalState() => ViewState = OrdersViewState.Normal;

    [RelayCommand]
    public void SetEmptyState() => ViewState = OrdersViewState.Empty;

    [RelayCommand]
    public void SetLoadingState() => ViewState = OrdersViewState.Loading;

    [RelayCommand]
    public void SetErrorState()
    {
        ViewState = OrdersViewState.NetworkError;
        ShowToast("Impossible de charger les commandes.", ToastType.Error);
    }
}

public partial class OrderDetailViewModel : BaseViewModel
{
    private readonly IOrderService _orderService;

    [ObservableProperty]
    private Order? _order;

    [ObservableProperty]
    private string _nextActionTitle = "Passer à « Prête à retirer »";

    public OrderDetailViewModel(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task LoadOrderAsync(int orderId)
    {
        Order = await _orderService.GetOrderByIdAsync(orderId);
        UpdateNextActionTitle();
    }

    private void UpdateNextActionTitle()
    {
        if (Order == null) return;
        NextActionTitle = Order.Status switch
        {
            OrderStatus.Recue => "Passer à « Coupe et couture »",
            OrderStatus.Couture => "Passer à « Essayage client »",
            OrderStatus.Essayage => "Passer à « Prête à retirer »",
            OrderStatus.Prete => "Marquer comme « Livrée »",
            OrderStatus.Livree => "Commande clôturée",
            _ => "Avancer le statut"
        };
    }

    [RelayCommand]
    public async Task AdvanceStatusAsync()
    {
        if (Order == null || Order.Status == OrderStatus.Livree) return;

        await _orderService.AdvanceOrderStatusAsync(Order.Id);
        await LoadOrderAsync(Order.Id);
        ShowToast($"Statut mis à jour : {Order.StatusText}", ToastType.Success);
    }
}

public partial class NewOrderViewModel : BaseViewModel
{
    private readonly IOrderService _orderService;
    private readonly IClientService _clientService;

    [ObservableProperty]
    private Client? _selectedClient;

    [ObservableProperty]
    private string _modelName = "Robe de soirée longue";

    [ObservableProperty]
    private string _fabric = "Satin bleu nuit";

    [ObservableProperty]
    private decimal _totalPrice = 45000;

    [ObservableProperty]
    private decimal _advancePayment = 20000;

    [ObservableProperty]
    private DateTime _dueDate = new(2026, 10, 8);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPhoto))]
    private string _photoPath = string.Empty;

    public bool HasPhoto => !string.IsNullOrWhiteSpace(PhotoPath);

    public ObservableCollection<Client> Clients { get; } = new();

    public NewOrderViewModel(IOrderService orderService, IClientService clientService)
    {
        _orderService = orderService;
        _clientService = clientService;
    }

    public async Task InitializeAsync()
    {
        var clients = await _clientService.GetAllClientsAsync();
        Clients.Clear();
        foreach (var c in clients)
        {
            Clients.Add(c);
        }
        SelectedClient = Clients.FirstOrDefault(c => c.FullName.Contains("Nadège")) ?? Clients.FirstOrDefault();
    }

    [RelayCommand]
    public async Task<Order> CreateOrderAsync()
    {
        var order = new Order
        {
            ClientId = SelectedClient?.Id ?? 1,
            ClientName = SelectedClient?.FullName ?? "Client sans nom",
            ClientPhone = SelectedClient?.PhoneNumber ?? string.Empty,
            ModelName = ModelName,
            Fabric = Fabric,
            TotalPrice = TotalPrice,
            AdvancePayment = AdvancePayment,
            DueDate = DueDate,
            DueDateLabel = $"Pour le {DueDate:d MMM yyyy}",
            Status = OrderStatus.Recue,
            SvgAsset = "dress",
            GradientClass = "g-ind",
            ImagePath = PhotoPath
        };

        var created = await _orderService.CreateOrderAsync(order);
        ShowToast("Commande enregistrée avec succès !", ToastType.Success);
        return created;
    }
}
