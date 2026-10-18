using System.Windows.Input;
using SweetTailor.Core.Models;

namespace SweetTailor.App.Controls;

public partial class ModelCard : ContentView
{
    public static readonly BindableProperty ModelProperty =
        BindableProperty.Create(nameof(Model), typeof(CatalogModel), typeof(ModelCard), null, propertyChanged: (b, o, n) => ((ModelCard)b).UpdateModel((CatalogModel?)n));

    public static readonly BindableProperty ImageHeightProperty =
        BindableProperty.Create(nameof(ImageHeight), typeof(double), typeof(ModelCard), 160.0);

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ModelCard), null);

    public static readonly BindableProperty ToggleFavoriteCommandProperty =
        BindableProperty.Create(nameof(ToggleFavoriteCommand), typeof(ICommand), typeof(ModelCard), null);

    public event EventHandler? Tapped;
    public event EventHandler? FavoriteToggled;

    public CatalogModel? Model
    {
        get => (CatalogModel?)GetValue(ModelProperty);
        set => SetValue(ModelProperty, value);
    }

    public double ImageHeight
    {
        get => (double)GetValue(ImageHeightProperty);
        set => SetValue(ImageHeightProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public ICommand? ToggleFavoriteCommand
    {
        get => (ICommand?)GetValue(ToggleFavoriteCommandProperty);
        set => SetValue(ToggleFavoriteCommandProperty, value);
    }

    public ModelCard()
    {
        InitializeComponent();

        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) =>
        {
            Tapped?.Invoke(this, EventArgs.Empty);
            if (Command?.CanExecute(Model) == true)
            {
                Command.Execute(Model);
            }
        };
        GestureRecognizers.Add(tap);
    }

    private void UpdateModel(CatalogModel? model)
    {
        if (model == null) return;

        CategoryLabel.Text = model.Category.ToUpperInvariant();
        TitleLabel.Text = model.Title;
        HeartIcon.Fill = model.IsFavorite ? Color.FromArgb("#E8607F") : Colors.Transparent;
        HeartIcon.Stroke = model.IsFavorite ? Color.FromArgb("#E8607F") : Color.FromArgb("#6B7094");

        var geometryKey = model.SvgAsset.ToLowerInvariant() switch
        {
            "gown" => "GeoGown",
            "kaba" => "GeoKaba",
            "suit" => "GeoSuit",
            "skirt" => "GeoSkirt",
            _ => "GeoDress"
        };
        GarmentIcon.Data = Application.Current?.Resources[geometryKey] as Microsoft.Maui.Controls.Shapes.Geometry;
        GarmentIcon.Fill = Color.FromArgb(model.SvgFillColor);

        if (!string.IsNullOrEmpty(model.ImagePath))
        {
            GarmentPhoto.Source = model.ImagePath;
            GarmentPhoto.IsVisible = true;
            GarmentIcon.IsVisible = false;
        }
        else
        {
            GarmentPhoto.IsVisible = false;
            GarmentIcon.IsVisible = true;
        }

        ImagePlaceholder.BackgroundColor = model.Category.ToLowerInvariant() switch
        {
            "robes" or "mariage" or "soirée" => Color.FromArgb("#FCE4EA"),
            "pagne" => Color.FromArgb("#FDF3D6"),
            "homme" or "costumes" => Color.FromArgb("#E4E5F2"),
            _ => Color.FromArgb("#F6F5FA")
        };
    }

    public void RefreshModel() => UpdateModel(Model);

    private void OnHeartTapped(object? sender, EventArgs e)
    {
        if (Model != null)
        {
            ToggleFavoriteCommand?.Execute(Model);
            FavoriteToggled?.Invoke(this, EventArgs.Empty);
        }
    }
}
