using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SweetTailor.Core.Services;

namespace SweetTailor.Core.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    private readonly IDemoModeService _demoMode;

    public event EventHandler? LogoutRequested;

    [ObservableProperty]
    private string _stylistName = "Rose Ndongo";

    [ObservableProperty]
    private string _shopName = "Atelier Rose d'Or";

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

    public ProfileViewModel(IDemoModeService demoMode)
    {
        _demoMode = demoMode;
        Title = "Profil & Atelier";
        UpdateDemoStateSelection();
    }

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
        ShowToast("Déconnexion en cours...", ToastType.Info);
        await Task.Delay(400);
        LogoutRequested?.Invoke(this, EventArgs.Empty);
    }
}
