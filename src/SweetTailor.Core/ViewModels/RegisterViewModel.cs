using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SweetTailor.Core.Services;

namespace SweetTailor.Core.ViewModels;

public partial class RegisterViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private static readonly Regex EmailPattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex DigitPattern = new(@"\d", RegexOptions.Compiled);

    public event EventHandler? RegistrationSucceeded;

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private bool _acceptTerms = false;

    public RegisterViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    public bool IsFullNameValid => !string.IsNullOrWhiteSpace(FullName) && FullName.Trim().Length >= 2;
    public bool IsPhoneValid => !string.IsNullOrWhiteSpace(Phone) && Phone.Trim().Length >= 9;
    
    public bool IsEmailValid => !string.IsNullOrWhiteSpace(Email) && EmailPattern.IsMatch(Email);
    public bool HasEmailError => !string.IsNullOrWhiteSpace(Email) && !IsEmailValid;
    public string EmailErrorMessage => "Adresse e-mail invalide. Exemple : nom@mail.com";

    public bool IsPasswordValid => !string.IsNullOrWhiteSpace(Password) && Password.Length >= 8 && DigitPattern.IsMatch(Password);
    public string PasswordHint => "8 caractères minimum, dont 1 chiffre";

    public bool IsConfirmPasswordValid => !string.IsNullOrWhiteSpace(ConfirmPassword) && ConfirmPassword == Password;
    public bool HasConfirmPasswordError => !string.IsNullOrWhiteSpace(ConfirmPassword) && ConfirmPassword != Password;
    public string ConfirmPasswordErrorMessage => "Les mots de passe ne correspondent pas";

    public string DisplayEmailError => HasEmailError ? "Adresse e-mail invalide. Exemple : nom@mail.com" : string.Empty;
    public string DisplayConfirmPasswordError => HasConfirmPasswordError ? "Les mots de passe ne correspondent pas" : string.Empty;

    public bool CanSubmit => IsFullNameValid && IsPhoneValid && IsEmailValid && IsPasswordValid && IsConfirmPasswordValid && AcceptTerms;

    partial void OnFullNameChanged(string value) => RefreshValidation();
    partial void OnPhoneChanged(string value) => RefreshValidation();
    partial void OnEmailChanged(string value) => RefreshValidation();
    partial void OnPasswordChanged(string value) => RefreshValidation();
    partial void OnConfirmPasswordChanged(string value) => RefreshValidation();
    partial void OnAcceptTermsChanged(bool value) => RefreshValidation();

    private void RefreshValidation()
    {
        OnPropertyChanged(nameof(IsFullNameValid));
        OnPropertyChanged(nameof(IsPhoneValid));
        OnPropertyChanged(nameof(IsEmailValid));
        OnPropertyChanged(nameof(HasEmailError));
        OnPropertyChanged(nameof(DisplayEmailError));
        OnPropertyChanged(nameof(IsPasswordValid));
        OnPropertyChanged(nameof(IsConfirmPasswordValid));
        OnPropertyChanged(nameof(HasConfirmPasswordError));
        OnPropertyChanged(nameof(DisplayConfirmPasswordError));
        OnPropertyChanged(nameof(CanSubmit));
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (!CanSubmit) return;

        IsBusy = true;
        try
        {
            var res = await _authService.RegisterAsync(FullName, Phone, Email, Password, ConfirmPassword, AcceptTerms);
            if (res.IsSuccess)
            {
                ShowToast("Compte créé avec succès !", ToastType.Success);
                await Task.Delay(400);
                RegistrationSucceeded?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ShowToast(res.ErrorMessage ?? "Erreur lors de la création du compte", ToastType.Error);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
