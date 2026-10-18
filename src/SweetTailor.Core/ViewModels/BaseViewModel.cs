using CommunityToolkit.Mvvm.ComponentModel;

namespace SweetTailor.Core.ViewModels;

public enum ToastType
{
    None,
    Success,
    Error,
    Info
}

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _toastMessage = string.Empty;

    [ObservableProperty]
    private ToastType _toastType = ToastType.None;

    [ObservableProperty]
    private bool _isToastVisible;

    public void ShowToast(string message, ToastType type = ToastType.Info)
    {
        ToastMessage = message;
        ToastType = type;
        IsToastVisible = true;
    }

    public void HideToast()
    {
        IsToastVisible = false;
        ToastMessage = string.Empty;
        ToastType = ToastType.None;
    }
}
