using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SweetTailor.Core.Models;
using SweetTailor.Core.Services;

namespace SweetTailor.Core.ViewModels;

public partial class ClientDetailViewModel : BaseViewModel
{
    private readonly IClientService _clientService;
    private readonly IOrderService _orderService;

    [ObservableProperty]
    private Client? _client;

    [ObservableProperty]
    private string _selectedTab = "Mesures"; // "Mesures", "Commandes", "Notes"

    [ObservableProperty]
    private string _newNoteContent = string.Empty;

    public ObservableCollection<Order> Orders { get; } = new();
    public ObservableCollection<ClientNote> Notes { get; } = new();
    public ObservableCollection<MeasurementItem> KeyMeasurements { get; } = new();
    public bool HasOrders => Orders.Count > 0;
    public bool HasNotes => Notes.Count > 0;

    public ClientDetailViewModel(IClientService clientService, IOrderService orderService)
    {
        _clientService = clientService;
        _orderService = orderService;
        Orders.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasOrders));
        Notes.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNotes));
    }

    public async Task LoadClientAsync(int clientId)
    {
        Client = await _clientService.GetClientByIdAsync(clientId);
        if (Client == null) return;

        // Key measurements (Chest, Waist, Hips, Dress length or first measurements)
        KeyMeasurements.Clear();
        var keyList = Client.Measurements.Where(m => m.Name is "Tour de poitrine" or "Tour de taille" or "Tour de hanches" or "Longueur robe" or "Carrure dos" or "Longueur pantalon").ToList();
        if (keyList.Count == 0 && Client.Measurements.Count > 0)
        {
            keyList = Client.Measurements.Take(4).ToList();
        }
        foreach (var m in keyList)
        {
            KeyMeasurements.Add(m);
        }

        // Orders
        Orders.Clear();
        var clientOrders = await _orderService.GetOrdersForClientAsync(clientId);
        foreach (var order in clientOrders)
        {
            Orders.Add(order);
        }

        // Notes
        Notes.Clear();
        foreach (var note in Client.Notes)
        {
            Notes.Add(note);
        }
    }

    [RelayCommand]
    private void SelectTab(string tab)
    {
        SelectedTab = tab;
    }

    [RelayCommand]
    private async Task AddNoteAsync()
    {
        if (Client == null || string.IsNullOrWhiteSpace(NewNoteContent)) return;

        var note = await _clientService.AddNoteAsync(Client.Id, NewNoteContent.Trim());
        Notes.Insert(0, note);
        NewNoteContent = string.Empty;
        ShowToast("Note ajoutée !", ToastType.Success);
    }

    [RelayCommand]
    public async Task<bool> DeleteClientAsync()
    {
        if (Client == null) return false;
        var success = await _clientService.DeleteClientAsync(Client.Id);
        if (success)
        {
            ShowToast($"Client {Client.FullName} supprimé.", ToastType.Success);
        }
        return success;
    }

    [RelayCommand]
    public async Task<bool> DeleteNoteAsync(ClientNote? note)
    {
        if (Client == null || note == null) return false;
        var success = await _clientService.DeleteNoteAsync(Client.Id, note.Id);
        if (success)
        {
            Notes.Remove(note);
            ShowToast("Note supprimée.", ToastType.Success);
        }
        return success;
    }

    [RelayCommand]
    public async Task<bool> DeleteOrderAsync(Order? order)
    {
        if (order == null) return false;
        var success = await _orderService.DeleteOrderAsync(order.Id);
        if (success)
        {
            Orders.Remove(order);
            ShowToast($"Commande {order.OrderNumber} supprimée.", ToastType.Success);
        }
        return success;
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        if (Client != null)
        {
            await LoadClientAsync(Client.Id);
        }
    }
}
