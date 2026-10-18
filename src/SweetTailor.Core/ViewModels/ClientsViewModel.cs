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
        "Tous · 48",
        "En cours · 12",
        "À livrer · 5",
        "Livrés"
    };

    public ObservableCollection<Client> Clients { get; } = new();

    public ClientsViewModel(IClientService clientService)
    {
        _clientService = clientService;
    }

    [RelayCommand]
    public async Task LoadClientsAsync()
    {
        ViewState = ClientsViewState.Loading;
        try
        {
            var list = await _clientService.SearchClientsAsync(SearchQuery, SelectedFilterChip);
            Clients.Clear();
            foreach (var client in list)
            {
                Clients.Add(client);
            }

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
