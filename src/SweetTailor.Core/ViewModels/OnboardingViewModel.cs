using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SweetTailor.Core.ViewModels;

public partial class SplashViewModel : BaseViewModel
{
    public string AppTitle => "Sweet Tailor";
    public string Tagline => "Chaque mesure compte.";
    public string VersionText => "Version 1.0";
}

public class OnboardingSlide
{
    public int Index { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Slogan { get; set; } = string.Empty;
    public string ButtonText { get; set; } = "Suivant";
    public bool IsLastSlide { get; set; }
    public bool IsSlide0 => Index == 0;
    public bool IsSlide1 => Index == 1;
    public bool IsSlide2 => Index == 2;
    public string BackgroundColorHex => Index switch
    {
        0 => "#FDEEF1",
        1 => "#EEF0FB",
        _ => "#FDF3D6"
    };
}

public partial class OnboardingViewModel : BaseViewModel
{
    [ObservableProperty]
    private int _currentSlideIndex = 0;

    public List<OnboardingSlide> Slides { get; } = new()
    {
        new()
        {
            Index = 0,
            Title = "Fini les cahiers, tout est dans votre poche",
            Description = "Notez les mesures de chaque client une seule fois. Retrouvez-les en un geste, même des mois plus tard.",
            ButtonText = "Suivant",
            IsLastSlide = false
        },
        new()
        {
            Index = 1,
            Title = "Chaque commande, livrée à temps",
            Description = "Suivez l'échéance et le statut de chaque vêtement, de la coupe à l'essayage jusqu'à la livraison.",
            ButtonText = "Suivant",
            IsLastSlide = false
        },
        new()
        {
            Index = 2,
            Title = "Inspirez-vous, puis passez à la création",
            Description = "Parcourez un catalogue de modèles, enregistrez vos coups de cœur et liez-les à une commande.",
            ButtonText = "Commencer",
            IsLastSlide = true
        }
    };

    public OnboardingSlide CurrentSlide => Slides[Math.Clamp(CurrentSlideIndex, 0, Slides.Count - 1)];

    [RelayCommand]
    private void Next()
    {
        if (CurrentSlideIndex < Slides.Count - 1)
        {
            CurrentSlideIndex++;
            OnPropertyChanged(nameof(CurrentSlide));
        }
    }

    [RelayCommand]
    private void Skip()
    {
        CurrentSlideIndex = Slides.Count - 1;
        OnPropertyChanged(nameof(CurrentSlide));
    }
}
