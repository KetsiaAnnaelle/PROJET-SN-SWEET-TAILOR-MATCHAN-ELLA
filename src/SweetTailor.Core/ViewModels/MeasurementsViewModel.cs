using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SweetTailor.Core.Models;
using SweetTailor.Core.Services;
using System.Text.RegularExpressions;
using System.Net.Http;

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
        try
        {
            if (ClientId <= 0)
            {
                throw new InvalidOperationException("Aucun client n’est associé à ces mesures.");
            }

            if (_allMeasurements.Any(measurement =>
                    !double.IsFinite(measurement.Value) || measurement.Value is < 0 or > 500))
            {
                ShowToast("Chaque mesure doit être comprise entre 0 et 500 cm.", ToastType.Error);
                return;
            }

            if (!await _clientService.UpdateMeasurementsAsync(ClientId, _allMeasurements))
            {
                ShowToast("Impossible d’enregistrer les mesures : client introuvable.", ToastType.Error);
                return;
            }

            ShowToast("Mesures enregistrées avec succès.", ToastType.Success);
        }
        catch (HttpRequestException ex)
        {
            ShowToast($"Impossible d’enregistrer les mesures : {ex.Message}", ToastType.Error);
        }
        catch (InvalidOperationException ex)
        {
            ShowToast(ex.Message, ToastType.Error);
        }
    }
}

public partial class ClientFormViewModel : BaseViewModel
{
    private static readonly Regex PhonePattern = new(@"^[+\d\s().-]+$", RegexOptions.Compiled);
    private readonly IClientService _clientService;
    private int _clientId;
    private bool _hasAttemptedSave;

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _phoneNumber = string.Empty;

    [ObservableProperty]
    private string _address = string.Empty;

    [ObservableProperty]
    private ClientCategory _category = ClientCategory.Femme;

    [ObservableProperty]
    private string _categoryTab = "Femme";

    [ObservableProperty]
    private string _note = string.Empty;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPhoto))]
    private string _photoPath = string.Empty;

    public bool HasPhoto => !string.IsNullOrWhiteSpace(PhotoPath);
    public string FormTitle => IsEditing ? "Modifier le client" : "Nouveau client";
    public string SaveButtonText => IsEditing ? "Enregistrer les modifications" : "Enregistrer et saisir les mesures";
    public bool IsNewClient => !IsEditing;
    public bool IsFullNameValid => !string.IsNullOrWhiteSpace(FullName) && FullName.Trim().Length >= 2;
    public bool IsPhoneNumberValid => IsValidPhone(PhoneNumber);
    public string FullNameError => (_hasAttemptedSave || !string.IsNullOrWhiteSpace(FullName)) && !IsFullNameValid
        ? "Le nom doit contenir au moins 2 caractères."
        : string.Empty;
    public string PhoneNumberError => (_hasAttemptedSave || !string.IsNullOrWhiteSpace(PhoneNumber)) && !IsPhoneNumberValid
        ? "Entrez un numéro valide contenant entre 9 et 15 chiffres."
        : string.Empty;

    public ClientFormViewModel(IClientService clientService)
    {
        _clientService = clientService;
    }

    partial void OnFullNameChanged(string value) => RefreshValidation();
    partial void OnPhoneNumberChanged(string value) => RefreshValidation();

    private static bool IsValidPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone) || !PhonePattern.IsMatch(phone.Trim()))
        {
            return false;
        }

        var digitCount = phone.Count(char.IsDigit);
        return digitCount is >= 9 and <= 15;
    }

    private void RefreshValidation()
    {
        OnPropertyChanged(nameof(IsFullNameValid));
        OnPropertyChanged(nameof(IsPhoneNumberValid));
        OnPropertyChanged(nameof(FullNameError));
        OnPropertyChanged(nameof(PhoneNumberError));
    }

    [RelayCommand]
    public void SelectCategory(ClientCategory category)
    {
        Category = category;
    }

    partial void OnCategoryChanged(ClientCategory value)
    {
        CategoryTab = value switch
        {
            ClientCategory.Homme => "Homme",
            ClientCategory.Enfant => "Enfant",
            _ => "Femme"
        };
    }

    partial void OnIsEditingChanged(bool value)
    {
        OnPropertyChanged(nameof(FormTitle));
        OnPropertyChanged(nameof(SaveButtonText));
        OnPropertyChanged(nameof(IsNewClient));
    }

    public async Task LoadClientAsync(int clientId)
    {
        var client = await _clientService.GetClientByIdAsync(clientId);
        if (client == null)
        {
            throw new InvalidOperationException("Client introuvable.");
        }

        _clientId = client.Id;
        FullName = client.FullName;
        PhoneNumber = client.PhoneNumber;
        Address = client.Address;
        Category = client.Category;
        Note = client.NotesSummary;
        PhotoPath = client.PhotoPath;
        IsEditing = true;
    }

    public async Task<Client> SaveAsync(bool thenAddMeasurements)
    {
        _hasAttemptedSave = true;
        RefreshValidation();
        if (!IsFullNameValid)
        {
            throw new ArgumentException("Le nom doit contenir au moins 2 caractères.");
        }
        if (!IsPhoneNumberValid)
        {
            throw new ArgumentException("Entrez un numéro valide contenant entre 9 et 15 chiffres.");
        }

        var client = new Client
        {
            Id = _clientId,
            FullName = FullName.Trim(),
            PhoneNumber = PhoneNumber.Trim(),
            Address = Address.Trim(),
            Category = Category,
            NotesSummary = Note.Trim(),
            AvatarColorClass = "a1",
            PhotoPath = PhotoPath
        };

        if (IsEditing)
        {
            var existing = await _clientService.GetClientByIdAsync(_clientId);
            if (existing != null)
            {
                client.Measurements = existing.Measurements;
                client.Notes = existing.Notes;
                client.CreatedAt = existing.CreatedAt;
                client.AvatarColorClass = existing.AvatarColorClass;
            }

            if (!await _clientService.UpdateClientAsync(client))
            {
                throw new InvalidOperationException("Le client à modifier n'existe plus.");
            }

            ShowToast("Client modifié avec succès.", ToastType.Success);
            return client;
        }

        var created = await _clientService.AddClientAsync(client);
        _clientId = created.Id;
        ShowToast("Client enregistré avec succès.", ToastType.Success);
        return created;
    }

    public async Task<bool> DeleteAsync()
    {
        if (!IsEditing)
        {
            return false;
        }

        var deleted = await _clientService.DeleteClientAsync(_clientId);
        if (deleted)
        {
            ShowToast("Client supprimé.", ToastType.Success);
        }
        return deleted;
    }
}
