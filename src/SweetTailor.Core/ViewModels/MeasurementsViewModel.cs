using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SweetTailor.Core.Models;
using SweetTailor.Core.Services;

namespace SweetTailor.Core.ViewModels;

public partial class MeasurementsViewModel : BaseViewModel
{
    private readonly IClientService _clientService;

    [ObservableProperty]
    private int _clientId;

    [ObservableProperty]
    private string _clientName = "Nadège";

    [ObservableProperty]
    private string _selectedCategory = "Haut du corps";

    public List<string> Categories { get; } = new()
    {
        "Haut du corps",
        "Bas",
        "Longueurs"
    };

    private List<MeasurementItem> _allMeasurements = new();
    public ObservableCollection<MeasurementItem> FilteredMeasurements { get; } = new();

    public MeasurementsViewModel(IClientService clientService)
    {
        _clientService = clientService;
    }

    public async Task LoadMeasurementsAsync(int clientId)
    {
        ClientId = clientId;
        var client = await _clientService.GetClientByIdAsync(clientId);
        if (client != null)
        {
            ClientName = client.FullName.Split(' ').FirstOrDefault() ?? client.FullName;
            _allMeasurements = client.Measurements;
            ApplyCategoryFilter();
        }
    }

    [RelayCommand]
    public void SelectCategory(string category)
    {
        SelectedCategory = category;
        ApplyCategoryFilter();
    }

    private void ApplyCategoryFilter()
    {
        FilteredMeasurements.Clear();
        var catEnum = SelectedCategory switch
        {
            "Haut du corps" => MeasurementCategory.HautDuCorps,
            "Bas" => MeasurementCategory.Bas,
            "Longueurs" => MeasurementCategory.Longueurs,
            _ => MeasurementCategory.HautDuCorps
        };

        foreach (var m in _allMeasurements.Where(m => m.Category == catEnum))
        {
            FilteredMeasurements.Add(m);
        }
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        await _clientService.UpdateMeasurementsAsync(ClientId, _allMeasurements);
        ShowToast("Mesures enregistrées avec succès.", ToastType.Success);
    }
}

public partial class ClientFormViewModel : BaseViewModel
{
    private readonly IClientService _clientService;

    [ObservableProperty]
    private string _fullName = "Nadège Mbarga";

    [ObservableProperty]
    private string _phoneNumber = "+237 6 77 00 00 00";

    [ObservableProperty]
    private string _address = "Bonamoussadi, Douala";

    [ObservableProperty]
    private ClientCategory _category = ClientCategory.Femme;

    [ObservableProperty]
    private string _note = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPhoto))]
    private string _photoPath = string.Empty;

    public bool HasPhoto => !string.IsNullOrWhiteSpace(PhotoPath);

    public ClientFormViewModel(IClientService clientService)
    {
        _clientService = clientService;
    }

    [RelayCommand]
    public void SelectCategory(ClientCategory category)
    {
        Category = category;
    }

    [RelayCommand]
    public async Task<Client> SaveAsync(bool thenAddMeasurements)
    {
        var client = new Client
        {
            FullName = FullName,
            PhoneNumber = PhoneNumber,
            Address = Address,
            Category = Category,
            NotesSummary = Note,
            AvatarColorClass = "a1",
            PhotoPath = PhotoPath
        };

        var created = await _clientService.AddClientAsync(client);
        ShowToast("Client enregistré avec succès.", ToastType.Success);
        return created;
    }
}
