using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SweetTailor.Core.Services;

namespace SweetTailor.Core.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    private readonly IDemoModeService _demoMode;
    private readonly IAuthService? _authService;
    private readonly IClientService? _clientService;
    private readonly IOrderService? _orderService;
    private readonly IWorkshopProfileService? _profileService;

    public event EventHandler? LogoutRequested;

    [ObservableProperty]
    private string _stylistName = "Rose Ndongo";

    [ObservableProperty]
    private string _shopName = "Atelier Rose d'Or";

    public string WorkshopDescription => string.IsNullOrWhiteSpace(Address) ? ShopName : $"{ShopName} · {Address}";

    [ObservableProperty]
    private string _address = "Bonamoussadi, Douala";

    [ObservableProperty]
    private string _email = "rose.ndongo@mail.com";

    [ObservableProperty]
    private string _phone = "+237 6 77 12 34 56";

    [ObservableProperty]
    private int _clientsCount = 28;

    [ObservableProperty]
    private int _ordersThisMonth = 14;

    [ObservableProperty]
    private string _onTimeRate = "98%";

    [ObservableProperty]
    private bool _autoBackup = true;

    [ObservableProperty]
    private bool _fittingReminders = true;

    [ObservableProperty]
    private DemoListState _currentDemoState;

    [ObservableProperty]
    private int _selectedDemoIndex = 0;

    public ProfileViewModel(IDemoModeService demoMode, IAuthService? authService = null,
        IClientService? clientService = null, IOrderService? orderService = null,
        IWorkshopProfileService? profileService = null)
    {
        _demoMode = demoMode;
        _authService = authService;
        _clientService = clientService;
        _orderService = orderService;
        _profileService = profileService;
        Title = "Profil & Atelier";
        UpdateDemoStateSelection();
    }

    partial void OnShopNameChanged(string value) => OnPropertyChanged(nameof(WorkshopDescription));
    partial void OnAddressChanged(string value) => OnPropertyChanged(nameof(WorkshopDescription));

    public ProfileViewModel() : this(new DemoModeService())
    {
    }

    private void UpdateDemoStateSelection()
    {
        CurrentDemoState = _demoMode.CurrentState;
        SelectedDemoIndex = _demoMode.CurrentState switch
        {
            DemoListState.Normal => 0,
            DemoListState.Empty => 1,
            DemoListState.Error => 2,
            DemoListState.Slow => 3,
            _ => 0
        };
    }

    public async Task LoadStatsAsync()
    {
        try
        {
            if (_profileService != null)
            {
                var profile = await _profileService.GetProfileAsync();
                StylistName = profile.FullName;
                ShopName = profile.WorkshopName;
                Address = profile.Address;
                Email = profile.Email;
                Phone = profile.Phone;

                var stats = await _profileService.GetStatsAsync();
                ClientsCount = stats.ClientCount;
                OrdersThisMonth = stats.OrdersThisMonth;
                OnTimeRate = $"{stats.OnTimeRate}%";
                return;
            }

            if (_clientService != null)
            {
                ClientsCount = await _clientService.GetClientsCountAsync();
            }
            if (_orderService != null)
            {
                OrdersThisMonth = await _orderService.GetActiveOrdersCountAsync();
            }
        }
        catch (Exception)
        {
            ShowToast("Impossible de charger les statistiques de l'atelier.", ToastType.Error);
        }
    }

    [RelayCommand]
    public void SetDemoState(string state)
    {
        _demoMode.CurrentState = state switch
        {
            "Empty" or "Vide" => DemoListState.Empty,
            "Error" or "Erreur" => DemoListState.Error,
            "Slow" or "Lent" => DemoListState.Slow,
            _ => DemoListState.Normal
        };
        UpdateDemoStateSelection();
        ShowToast($"Mode démo : {_demoMode.CurrentState}", ToastType.Info);
    }

    [RelayCommand]
    public void ResetDemoData()
    {
        _demoMode.CurrentState = DemoListState.Normal;
        UpdateDemoStateSelection();
        ShowToast("Données réinitialisées avec succès", ToastType.Success);
    }

    [RelayCommand]
    public async Task LogoutAsync()
    {
        try
        {
            if (_authService != null)
            {
                await _authService.LogoutAsync();
            }
        }
        catch (HttpRequestException ex)
        {
            ShowToast($"Déconnexion locale effectuée ; révocation serveur impossible : {ex.Message}", ToastType.Error);
        }
        catch (TaskCanceledException)
        {
            ShowToast("Déconnexion locale effectuée ; l’API n’a pas répondu à temps.", ToastType.Error);
        }
        finally
        {
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
