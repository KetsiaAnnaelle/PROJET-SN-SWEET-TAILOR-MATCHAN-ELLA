using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SweetTailor.Core.Services;

namespace SweetTailor.Core.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    public event EventHandler? LoginSucceeded;

    [ObservableProperty]
    private string _identifier = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isPasswordHidden = true;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private void TogglePasswordVisibility()
    {
        IsPasswordHidden = !IsPasswordHidden;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        IsBusy = true;
        try
        {
            var result = await _authService.LoginAsync(Identifier, Password);
            if (!result.IsSuccess)
            {
                ShowToast(result.ErrorMessage ?? "Erreur de connexion", ToastType.Error);
            }
            else
            {
                ShowToast("Connexion réussie !", ToastType.Success);
                await Task.Delay(300);
                LoginSucceeded?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (HttpRequestException ex)
        {
            ShowToast($"API indisponible : {ex.Message}", ToastType.Error);
        }
        catch (TaskCanceledException)
        {
            ShowToast("La requête a expiré. Vérifiez que l'API est démarrée.", ToastType.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public partial class ForgotPasswordViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _identifier = string.Empty;

    [ObservableProperty]
    private bool _codeSent;

    public ForgotPasswordViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task SendCodeAsync()
    {
        IsBusy = true;
        try
        {
            var res = await _authService.SendPasswordResetCodeAsync(Identifier);
            if (res.IsSuccess)
            {
                CodeSent = true;
                ShowToast("Code de réinitialisation envoyé par SMS/E-mail !", ToastType.Success);
            }
            else
            {
                ShowToast(res.ErrorMessage ?? "Erreur lors de l'envoi du code", ToastType.Error);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
