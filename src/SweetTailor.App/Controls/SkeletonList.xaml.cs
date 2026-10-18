namespace SweetTailor.App.Controls;

public partial class SkeletonList : ContentView
{
    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(SkeletonList), "Chargement de vos clients…", propertyChanged: (b, o, n) => ((SkeletonList)b).LoadingMessageLabel.Text = (string)n);

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public SkeletonList()
    {
        InitializeComponent();
    }
}
