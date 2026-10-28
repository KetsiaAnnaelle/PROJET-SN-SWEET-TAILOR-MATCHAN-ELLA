using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SweetTailor.Core.Models;
using SweetTailor.Core.Services;

namespace SweetTailor.Core.ViewModels;

public enum ClientsViewState
{
    Normal,
    Empty,
    Loading,
    NetworkError
}

public partial class ClientsViewModel : BaseViewModel
{
    private readonly IClientService _clientService;
    private readonly IDemoModeService? _demoMode;

    [ObservableProperty]
    private ClientsViewState _viewState = ClientsViewState.Normal;

    public bool IsNormalState => ViewState == ClientsViewState.Normal;
    public bool IsEmptyState => ViewState == ClientsViewState.Empty;
    public bool IsLoadingState => ViewState == ClientsViewState.Loading;
    public bool IsErrorState => ViewState == ClientsViewState.NetworkError;

    partial void OnViewStateChanged(ClientsViewState value)
    {
        OnPropertyChanged(nameof(IsNormalState));
        OnPropertyChanged(nameof(IsEmptyState));
        OnPropertyChanged(nameof(IsLoadingState));
        OnPropertyChanged(nameof(IsErrorState));
    }

    [ObservableProperty]
    private string _workshopName = "Atelier de Rose";

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _selectedFilterChip = "Tous · 48";

    [ObservableProperty]
    private string _upcomingTitle = "Cette semaine";

    [ObservableProperty]
    private string _upcomingSubtitle = "3 livraisons à venir";

    public List<string> FilterChips { get; } = new()
    {
        "Tous ",
        "En cours ",
        "À livrer ",
        "Livrés"
    };

    public ObservableCollection<Client> Clients { get; } = new();

    public ClientsViewModel(IClientService clientService, IDemoModeService? demoMode = null)
    {
        _clientService = clientService;
        _demoMode = demoMode;
    }

    [RelayCommand]
    public async Task LoadClientsAsync()
    {
        ViewState = ClientsViewState.Loading;
        try
        {
            if (_demoMode is { CurrentState: not DemoListState.Normal })
            {
                await _demoMode.SimulateDelayAsync();
                if (_demoMode.CurrentState == DemoListState.Empty)
                {
                    Clients.Clear();
                    ViewState = ClientsViewState.Empty;
                    return;
                }
            }

            var list = await _clientService.SearchClientsAsync(SearchQuery, SelectedFilterChip);
            Clients.Clear();
            foreach (var client in list)
            {
                Clients.Add(client);
            }

            await RefreshFilterCountsAsync();
            ViewState = Clients.Count == 0 ? ClientsViewState.Empty : ClientsViewState.Normal;
        }
        catch (Exception)
        {
            ViewState = ClientsViewState.NetworkError;
            ShowToast("Impossible de charger la liste des clients.", ToastType.Error);
        }
    }

    [RelayCommand]
    public async Task SelectFilterChipAsync(string chip)
    {
        SelectedFilterChip = chip;
        await LoadClientsAsync();
    }

    [RelayCommand]
    public async Task SearchAsync(string query)
    {
        SearchQuery = query;
        await LoadClientsAsync();
    }

    [RelayCommand]
    public async Task RetryAsync()
    {
        HideToast();
        await LoadClientsAsync();
    }

    [RelayCommand]
    public async Task DeleteClientAsync(Client? client)
    {
        if (client == null) return;
        var success = await _clientService.DeleteClientAsync(client.Id);
        if (success)
        {
            Clients.Remove(client);
            if (Clients.Count == 0) ViewState = ClientsViewState.Empty;
            ShowToast($"Client {client.FullName} supprimé avec succès.", ToastType.Success);
        }
    }

    [RelayCommand]
    public async Task AddClientAsync(Client? client)
    {
        if (client == null) return;
        var created = await _clientService.AddClientAsync(client);
        Clients.Insert(0, created);
        ViewState = ClientsViewState.Normal;
        ShowToast($"Client {created.FullName} enregistré avec succès.", ToastType.Success);
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        HideToast();
        await LoadClientsAsync();
    }

    private async Task RefreshFilterCountsAsync()
    {
        var all = await _clientService.GetAllClientsAsync();
        var inProgress = all.Count(client => client.ActiveOrdersCount > 0);
        var toDeliver = all.Count(client => client.CurrentOrderStatus is OrderStatus.Prete or OrderStatus.Essayage);
        FilterChips[0] = $"Tous · {all.Count}";
        FilterChips[1] = $"En cours · {inProgress}";
        FilterChips[2] = $"À livrer · {toDeliver}";
        OnPropertyChanged(nameof(FilterChips));
    }

    // Interactive state previewers for UI showcasing
    [RelayCommand]
    public void SetNormalState() => ViewState = ClientsViewState.Normal;

    [RelayCommand]
    public void SetEmptyState() => ViewState = ClientsViewState.Empty;

    [RelayCommand]
    public void SetLoadingState() => ViewState = ClientsViewState.Loading;

    [RelayCommand]
    public void SetErrorState()
    {
        ViewState = ClientsViewState.NetworkError;
        ShowToast("Impossible de charger la liste des clients.", ToastType.Error);
    }
}
