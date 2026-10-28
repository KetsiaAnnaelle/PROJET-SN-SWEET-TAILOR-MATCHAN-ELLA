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
    private readonly IDemoModeService? _demoMode;

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

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    public List<string> FilterChips { get; } = new()
    {
        "Toutes",
        "Reçues",
        "Couture",
        "Essayage",
        "Prêtes",
        "Livrées"
    };

    public ObservableCollection<Order> ThisWeekOrders { get; } = new();
    public ObservableCollection<Order> NextWeekOrders { get; } = new();

    public OrdersViewModel(IOrderService orderService, IDemoModeService? demoMode = null)
    {
        _orderService = orderService;
        _demoMode = demoMode;
    }

    [RelayCommand]
    public async Task LoadOrdersAsync()
    {
        ViewState = OrdersViewState.Loading;
        try
        {
            if (_demoMode is { CurrentState: not DemoListState.Normal })
            {
                await _demoMode.SimulateDelayAsync();
                if (_demoMode.CurrentState == DemoListState.Empty)
                {
                    ThisWeekOrders.Clear();
                    NextWeekOrders.Clear();
                    ViewState = OrdersViewState.Empty;
                    return;
                }
            }

            var orders = await _orderService.GetOrdersByFilterAsync(SelectedFilter);
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var query = SearchQuery.Trim();
                orders = orders.Where(order =>
                    order.ModelName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    order.ClientName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    order.OrderNumber.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    order.Fabric.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
            }
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
    public async Task DeleteOrderAsync(Order? order)
    {
        if (order == null) return;
        var success = await _orderService.DeleteOrderAsync(order.Id);
        if (success)
        {
            ThisWeekOrders.Remove(order);
            NextWeekOrders.Remove(order);
            var activeCount = await _orderService.GetActiveOrdersCountAsync();
            ActiveCountLabel = $"{activeCount} en cours";
            if (ThisWeekOrders.Count == 0 && NextWeekOrders.Count == 0)
            {
                ViewState = OrdersViewState.Empty;
            }
            ShowToast($"Commande {order.OrderNumber} supprimée.", ToastType.Success);
        }
    }

    [RelayCommand]
    public async Task RefreshAsync()
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

        var orderId = Order.Id;
        try
        {
            if (!await _orderService.AdvanceOrderStatusAsync(orderId))
            {
                ShowToast("La commande n'existe plus.", ToastType.Error);
                return;
            }

            await LoadOrderAsync(orderId);
            if (Order == null)
            {
                ShowToast("Impossible de recharger la commande.", ToastType.Error);
                return;
            }
            ShowToast($"Statut mis à jour : {Order.StatusText}", ToastType.Success);
        }
        catch (OrderPaymentRequiredException ex)
        {
            ShowToast(ex.Message, ToastType.Error);
        }
    }

    public Task<bool> DeleteOrderAsync(int orderId) => _orderService.DeleteOrderAsync(orderId);

    [RelayCommand]
    public async Task<bool> DeleteCurrentOrderAsync()
    {
        if (Order == null) return false;
        var success = await _orderService.DeleteOrderAsync(Order.Id);
        if (success)
        {
            ShowToast($"Commande {Order.OrderNumber} supprimée.", ToastType.Success);
        }
        return success;
    }
}

public partial class NewOrderViewModel : BaseViewModel
{
    private readonly IOrderService _orderService;
    private readonly IClientService _clientService;
    private Order? _editingOrder;
    private bool _hasAttemptedSave;

    [ObservableProperty]
    private Client? _selectedClient;

    [ObservableProperty]
    private string _modelName = string.Empty;

    [ObservableProperty]
    private string _fabric = string.Empty;

    [ObservableProperty]
    private decimal _totalPrice;

    [ObservableProperty]
    private decimal _advancePayment;

    [ObservableProperty]
    private DateTime _dueDate = DateTime.Today.AddDays(14);

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPhoto))]
    private string _photoPath = string.Empty;

    public bool HasPhoto => !string.IsNullOrWhiteSpace(PhotoPath);
    public string FormTitle => IsEditing ? "Modifier la commande" : "Nouvelle commande";
    public string SaveButtonText => IsEditing ? "Enregistrer les modifications" : "Enregistrer la commande";
    public bool IsModelNameValid => !string.IsNullOrWhiteSpace(ModelName) && ModelName.Trim().Length <= 120;
    public bool IsTotalPriceValid => TotalPrice > 0;
    public bool IsAdvancePaymentValid => AdvancePayment >= 0 && AdvancePayment <= TotalPrice;
    public bool IsDueDateValid => IsEditing || DueDate.Date >= DateTime.Today;
    public bool CanSave => SelectedClient != null && IsModelNameValid && IsTotalPriceValid &&
                           IsAdvancePaymentValid && IsDueDateValid;
    public string ModelNameError => (_hasAttemptedSave || !string.IsNullOrWhiteSpace(ModelName)) && !IsModelNameValid
        ? "Le nom du modèle est obligatoire (120 caractères maximum)."
        : string.Empty;
    public string TotalPriceError => (_hasAttemptedSave || TotalPrice != 0) && !IsTotalPriceValid
        ? "Le prix doit être supérieur à zéro."
        : string.Empty;
    public string AdvancePaymentError => (_hasAttemptedSave || AdvancePayment != 0) && !IsAdvancePaymentValid
        ? "L'acompte doit être compris entre 0 et le prix total."
        : string.Empty;
    public string DueDateError => _hasAttemptedSave && !IsDueDateValid
        ? "L'échéance ne peut pas être dans le passé."
        : string.Empty;

    public ObservableCollection<Client> Clients { get; } = new();

    public NewOrderViewModel(IOrderService orderService, IClientService clientService)
    {
        _orderService = orderService;
        _clientService = clientService;
    }

    partial void OnIsEditingChanged(bool value)
    {
        OnPropertyChanged(nameof(FormTitle));
        OnPropertyChanged(nameof(SaveButtonText));
        RefreshValidation();
    }

    partial void OnSelectedClientChanged(Client? value) => RefreshValidation();
    partial void OnModelNameChanged(string value) => RefreshValidation();
    partial void OnTotalPriceChanged(decimal value) => RefreshValidation();
    partial void OnAdvancePaymentChanged(decimal value) => RefreshValidation();
    partial void OnDueDateChanged(DateTime value) => RefreshValidation();

    private void RefreshValidation()
    {
        OnPropertyChanged(nameof(IsModelNameValid));
        OnPropertyChanged(nameof(IsTotalPriceValid));
        OnPropertyChanged(nameof(IsAdvancePaymentValid));
        OnPropertyChanged(nameof(IsDueDateValid));
        OnPropertyChanged(nameof(CanSave));
        OnPropertyChanged(nameof(ModelNameError));
        OnPropertyChanged(nameof(TotalPriceError));
        OnPropertyChanged(nameof(AdvancePaymentError));
        OnPropertyChanged(nameof(DueDateError));
    }

    public async Task InitializeAsync(int orderId = 0, int preferredClientId = 0, string? suggestedModelName = null)
    {
        var clients = await _clientService.GetAllClientsAsync();
        Clients.Clear();
        foreach (var c in clients)
        {
            Clients.Add(c);
        }
        if (orderId > 0)
        {
            await LoadOrderAsync(orderId);
            return;
        }

        if (preferredClientId > 0)
        {
            SelectedClient = Clients.FirstOrDefault(client => client.Id == preferredClientId);
        }
        SelectedClient ??= Clients.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(suggestedModelName) && string.IsNullOrWhiteSpace(ModelName))
        {
            ModelName = suggestedModelName;
        }
    }

    public async Task LoadOrderAsync(int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null)
        {
            throw new InvalidOperationException("Commande introuvable.");
        }

        _editingOrder = order;
        IsEditing = true;
        SelectedClient = Clients.FirstOrDefault(client => client.Id == order.ClientId);
        ModelName = order.ModelName;
        Fabric = order.Fabric;
        TotalPrice = order.TotalPrice;
        AdvancePayment = order.AdvancePayment;
        DueDate = order.DueDate;
        PhotoPath = order.ImagePath;
    }

    [RelayCommand]
    public async Task<Order> CreateOrderAsync()
    {
        if (SelectedClient == null && Clients.Count == 0)
        {
            await InitializeAsync();
        }

        _hasAttemptedSave = true;
        RefreshValidation();

        if (SelectedClient == null)
        {
            throw new ArgumentException("Veuillez sélectionner un client.");
        }
        if (!IsModelNameValid)
        {
            throw new ArgumentException("Le nom du modèle est obligatoire (120 caractères maximum).");
        }
        if (!IsTotalPriceValid)
        {
            throw new ArgumentException("Le prix doit être supérieur à zéro.");
        }
        if (!IsAdvancePaymentValid)
        {
            throw new ArgumentException("L'acompte doit être compris entre 0 et le prix total.");
        }
        if (!IsDueDateValid)
        {
            throw new ArgumentException("L'échéance ne peut pas être dans le passé.");
        }

        var order = new Order
        {
            Id = _editingOrder?.Id ?? 0,
            OrderNumber = _editingOrder?.OrderNumber ?? string.Empty,
            ClientId = SelectedClient.Id,
            ClientName = SelectedClient.FullName,
            ClientPhone = SelectedClient.PhoneNumber,
            ModelName = ModelName,
            Fabric = Fabric,
            TotalPrice = TotalPrice,
            AdvancePayment = AdvancePayment,
            DueDate = DueDate,
            DueDateLabel = $"Pour le {DueDate.ToString("d MMM yyyy", new System.Globalization.CultureInfo("fr-FR"))}",
            Status = _editingOrder?.Status ?? OrderStatus.Recue,
            SvgAsset = _editingOrder?.SvgAsset ?? "dress",
            GradientClass = _editingOrder?.GradientClass ?? "g-ind",
            ImagePath = PhotoPath,
            RemoteImagePath = _editingOrder?.RemoteImagePath,
            CreatedAt = _editingOrder?.CreatedAt ?? DateTime.Now,
            Steps = _editingOrder?.Steps ?? new List<OrderStep>()
        };

        if (IsEditing)
        {
            if (!await _orderService.UpdateOrderAsync(order))
            {
                throw new InvalidOperationException("La commande à modifier n'existe plus.");
            }
            _editingOrder = order;
            ShowToast("Commande modifiée avec succès.", ToastType.Success);
            return order;
        }

        var created = await _orderService.CreateOrderAsync(order);
        ShowToast("Commande enregistrée avec succès !", ToastType.Success);
        return created;
    }
}
