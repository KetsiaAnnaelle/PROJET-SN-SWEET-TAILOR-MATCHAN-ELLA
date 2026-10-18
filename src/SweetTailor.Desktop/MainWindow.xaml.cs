using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using SweetTailor.Core.Models;
using SweetTailor.Core.Services;
using SweetTailor.Core.ViewModels;

namespace SweetTailor.Desktop;

public partial class MainWindow : Window
{
    private readonly ClientService _clientService = new();
    private readonly OrderService _orderService = new();
    private readonly CatalogService _catalogService = new();

    private string _currentScreen = "Splash";
    private bool _isLoggedIn = false;
    private DispatcherTimer? _splashTimer;
    private Client? _selectedClient;
    private Order? _selectedOrder;

    public MainWindow()
    {
        InitializeComponent();
        _selectedClient = _clientService.GetClientsAsync().Result.FirstOrDefault();
        _selectedOrder = _orderService.GetAllOrdersAsync().Result.FirstOrDefault();
        NavigateTo("Splash"); // Default faithfully to Splash Screen (Page 1)
    }

    private void OnSelectScreen(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string screenKey)
        {
            NavigateTo(screenKey);
        }
    }

    private void OnNavClients(object sender, RoutedEventArgs e) => NavigateTo("Clients");
    private void OnNavOrders(object sender, RoutedEventArgs e) => NavigateTo("Orders");
    private void OnNavCatalog(object sender, RoutedEventArgs e) => NavigateTo("Catalog");
    private void OnNavProfile(object sender, RoutedEventArgs e) => NavigateTo(_isLoggedIn ? "Profile" : "Login");

    public void NavigateTo(string screenKey)
    {
        if (screenKey != "Splash")
        {
            _splashTimer?.Stop();
        }

        _currentScreen = screenKey;
        UpdateBottomNav(screenKey);
        HighlightSidebarButton(screenKey);
        ScreenHost.Content = BuildScreenContent(screenKey);
    }

    private void UpdateBottomNav(string screenKey)
    {
        var activeBrush = (Brush)FindResource("BrushIndigo");
        var inactiveBrush = (Brush)new BrushConverter().ConvertFrom("#9A9EBD")!;

        bool isClients = screenKey is "Clients" or "ClientsEmpty" or "ClientsLoading" or "ClientsError";
        bool isOrders = screenKey is "Orders";
        bool isCatalog = screenKey is "Catalog";
        bool isProfile = screenKey is "Profile";

        NavClientsIcon.Stroke = isClients ? activeBrush : inactiveBrush;
        NavClientsLabel.Foreground = isClients ? activeBrush : inactiveBrush;
        NavClientsLabel.FontWeight = isClients ? FontWeights.Bold : FontWeights.SemiBold;

        NavOrdersIcon.Stroke = isOrders ? activeBrush : inactiveBrush;
        NavOrdersLabel.Foreground = isOrders ? activeBrush : inactiveBrush;
        NavOrdersLabel.FontWeight = isOrders ? FontWeights.Bold : FontWeights.SemiBold;

        NavCatalogIcon.Stroke = isCatalog ? activeBrush : inactiveBrush;
        NavCatalogLabel.Foreground = isCatalog ? activeBrush : inactiveBrush;
        NavCatalogLabel.FontWeight = isCatalog ? FontWeights.Bold : FontWeights.SemiBold;

        NavProfileIcon.Stroke = isProfile ? activeBrush : inactiveBrush;
        NavProfileLabel.Foreground = isProfile ? activeBrush : inactiveBrush;
        NavProfileLabel.FontWeight = isProfile ? FontWeights.Bold : FontWeights.SemiBold;

        // Bottom navbar is ONLY shown when the user is logged in AND on a main tabbed screen!
        bool isTabbedScreen = screenKey is "Clients" or "ClientsEmpty" or "ClientsLoading" or "ClientsError" or "Orders" or "Catalog" or "Profile";
        bool showNav = _isLoggedIn && isTabbedScreen;
        BottomNavBar.Visibility = showNav ? Visibility.Visible : Visibility.Collapsed;
        BottomNavBar.IsHitTestVisible = showNav;
    }

    private void HighlightSidebarButton(string screenKey)
    {
        if (ScreenButtonsList == null) return;
        foreach (var child in ScreenButtonsList.Children)
        {
            if (child is Button btn)
            {
                bool isCurrent = (string)btn.Tag == screenKey;
                btn.Background = isCurrent ? (Brush)FindResource("BrushMist") : (Brush)FindResource("BrushChalk");
                btn.Foreground = isCurrent ? (Brush)FindResource("BrushIndigo") : (Brush)FindResource("BrushInk");
                btn.FontWeight = isCurrent ? FontWeights.Bold : FontWeights.SemiBold;
            }
        }
    }

    private FrameworkElement BuildScreenContent(string key)
    {
        return key switch
        {
            "Splash" => BuildSplashScreen(),
            "Onboarding1" => BuildOnboarding1Screen(),
            "Onboarding2" => BuildOnboarding2Screen(),
            "Onboarding3" => BuildOnboarding3Screen(),
            "Login" => BuildLoginScreen(),
            "ForgotPassword" => BuildForgotPasswordScreen(),
            "Register" => BuildRegisterScreen(),
            "Clients" => BuildClientsScreen(),
            "ClientsEmpty" => BuildClientsEmptyScreen(),
            "ClientsLoading" => BuildClientsLoadingScreen(),
            "ClientsError" => BuildClientsErrorScreen(),
            "ClientDetail" => BuildClientDetailScreen("Mesures"),
            "ClientOrders" => BuildClientDetailScreen("Commandes"),
            "ClientNotes" => BuildClientDetailScreen("Notes"),
            "Measurements" => BuildMeasurementsScreen(),
            "ClientForm" => BuildClientFormScreen(),
            "Orders" => BuildOrdersScreen(),
            "OrderDetail" => BuildOrderDetailScreen(),
            "NewOrder" => BuildNewOrderScreen(),
            "Search" => BuildSearchScreen(),
            "SearchEmpty" => BuildSearchEmptyScreen(),
            "Catalog" => BuildCatalogScreen(),
            "CatalogDetail" => BuildCatalogDetailScreen(),
            "Components" => BuildComponentsCharterScreen(),
            "Profile" => BuildProfileScreen(),
            _ => BuildClientsScreen()
        };
    }

    // =========================================================================
    // VECTOR ICON ENGINE (Exact Figma SVG Paths)
    // =========================================================================

    private Path CreateIcon(string name, Brush? stroke = null, Brush? fill = null, double size = 20, double strokeThickness = 1.8)
    {
        string pathData = name.ToLowerInvariant() switch
        {
            "users" => "M9,4.5 C10.93,4.5 12.5,6.07 12.5,8 C12.5,9.93 10.93,11.5 9,11.5 C7.07,11.5 5.5,9.93 5.5,8 C5.5,6.07 7.07,4.5 9,4.5 Z M2.5,20 C3.1,16.4 5.7,14.5 9,14.5 C12.3,14.5 14.9,16.4 15.5,20 M16,4.8 C17.3,5.5 18.2,6.9 18.2,8.5 C18.2,10.1 17.3,11.5 16,12.2 M18,14.8 C19.8,15.5 21,17.2 21.5,20",
            "bag" => "M5,8 L19,8 L18,20 L6,20 Z M9,8 L9,6.5 C9,4.84 10.34,3.5 12,3.5 C13.66,3.5 15,4.84 15,6.5 L15,8",
            "catalog" or "img" => "M3.5,4.5 L20.5,4.5 C21.6,4.5 22.5,5.4 22.5,6.5 L22.5,17.5 C22.5,18.6 21.6,19.5 20.5,19.5 L3.5,19.5 C2.4,19.5 1.5,18.6 1.5,17.5 L1.5,6.5 C1.5,5.4 2.4,4.5 3.5,4.5 Z M9,8.4 C9.88,8.4 10.6,9.12 10.6,10 C10.6,10.88 9.88,11.6 9,11.6 C8.12,11.6 7.4,10.88 7.4,10 C7.4,9.12 8.12,8.4 9,8.4 Z M4,17 L9,12.5 L13,16 L16,13.5 L20,17",
            "user" => "M12,4.5 C14.2,4.5 16,6.3 16,8.5 C16,10.7 14.2,12.5 12,12.5 C9.8,12.5 8,10.7 8,8.5 C8,6.3 9.8,4.5 12,4.5 Z M4.5,20 C5.2,16 8.2,14 12,14 C15.8,14 18.8,16 19.5,20",
            "search" => "M11,4 C14.86,4 18,7.14 18,11 C18,14.86 14.86,18 11,18 C7.14,18 4,14.86 4,11 C4,7.14 7.14,4 11,4 Z M20,20 L16.5,16.5",
            "plus" => "M12,5 L12,19 M5,12 L19,12",
            "calendar" or "cal" => "M3.5,5 L20.5,5 C21.6,5 22.5,5.9 22.5,7 L22.5,19 C22.5,20.1 21.6,21 20.5,21 L3.5,21 C2.4,21 1.5,20.1 1.5,19 L1.5,7 C1.5,5.9 2.4,5 3.5,5 Z M3.5,10 L20.5,10 M8,3 L8,7 M16,3 L16,7",
            "filter" => "M4,6 L20,6 M7,12 L17,12 M10,18 L14,18",
            "phone" => "M5,4 L9,4 L11,9 L8.5,10.5 C9.7,13 11.5,14.8 14,16 L15.5,13.5 L20,15.5 L20,19.5 C20,20.6 19.1,21.5 18,21.5 C9.7,21.5 3,14.8 3,6.5 C3,5.4 3.9,4.5 5,4.5 Z",
            "chat" => "M4,5 L20,5 L20,16 L9,16 L4,20 Z",
            "camera" or "cam" => "M4,8 L7,8 L8.5,6 L15.5,6 L17,8 L20,8 L20,19 L4,19 Z M12,9.5 C13.93,9.5 15.5,11.07 15.5,13 C15.5,14.93 13.93,16.5 12,16.5 C10.07,16.5 8.5,14.93 8.5,13 C8.5,11.07 10.07,9.5 12,9.5 Z",
            "check" => "M5,12.5 L9.5,17 L19,7.5",
            "chevron" or "chev" => "M9,5 L16,12 L9,19",
            "back" => "M15,5 L8,12 L15,19",
            "edit" => "M4,20 L8,20 L19,9 L15,5 L4,16 Z",
            "clock" or "hist" => "M12,3.5 C16.69,3.5 20.5,7.31 20.5,12 C20.5,16.69 16.69,20.5 12,20.5 C7.31,20.5 3.5,16.69 3.5,12 C3.5,7.31 7.31,3.5 12,3.5 Z M12,7.5 L12,12 L15,14",
            "close" or "x" => "M6,6 L18,18 M18,6 L6,18",
            "alert" => "M12,3 C17,3 21,7 21,12 C21,17 17,21 12,21 C7,21 3,17 3,12 C3,7 7,3 12,3 Z M12,7.5 L12,13 M12,16.3 L12,16.5",
            "lock" => "M5,10.5 L19,10.5 C20.1,10.5 21,11.4 21,12.5 L21,19.5 C21,20.6 20.1,21.5 19,21.5 L5,21.5 C3.9,21.5 3,20.6 3,19.5 L3,12.5 C3,11.4 3.9,10.5 5,10.5 Z M8,10.5 L8,8 C8,5.8 9.8,4 12,4 C14.2,4 16,5.8 16,8 L16,10.5",
            "mail" => "M3,5.5 L21,5.5 C22.1,5.5 23,6.4 23,7.5 L23,17.5 C23,18.6 22.1,19.5 21,19.5 L3,19.5 C1.9,19.5 1,18.6 1,17.5 L1,7.5 C1,6.4 1.9,5.5 3,5.5 Z M4,8 L12,13.5 L20,8",
            "eye" => "M2.5,12 C4.5,7 8,5.5 12,5.5 C16,5.5 19.5,7 21.5,12 C19.5,17 16,18.5 12,18.5 C8,18.5 4.5,17 2.5,12 Z M12,9 C13.66,9 15,10.34 15,12 C15,13.66 13.66,15 12,15 C10.34,15 9,13.66 9,12 C9,10.34 10.34,9 12,9 Z",
            "heart" => "M12,20 C12,20 4.5,15.4 4.5,10 C4.5,7.6 6.4,5.7 8.8,5.7 C10.1,5.7 11.3,6.3 12,7.3 C12.7,6.3 13.9,5.7 15.2,5.7 C17.6,5.7 19.5,7.6 19.5,10 C19.5,15.4 12,20 12,20 Z",
            "share" => "M12,15 L12,4 M8,8 L12,4 L16,8 M5,13 L5,19 L19,19 L19,13",
            "more" => "M4,12 C4,12.6 3.6,13 3,13 C2.4,13 2,12.6 2,12 C2,11.4 2.4,11 3,11 C3.6,11 4,11.4 4,12 Z M13,12 C13,12.6 12.6,13 12,13 C11.4,13 11,12.6 11,12 C11,11.4 11.4,11 12,11 C12.6,11 13,11.4 13,12 Z M22,12 C22,12.6 21.6,13 21,13 C20.4,13 20,12.6 20,12 C20,11.4 20.4,11 21,11 C21.6,11 22,11.4 22,12 Z",
            "wifioff" => "M3,3 L21,21 M5.2,10.3 C6.3,9.4 7.6,8.8 9,8.3 M19,10.3 C17.3,9 15.2,8.2 13.7,7.4 M8.6,14.3 C9.6,13.5 10.8,12.9 12,12.9 M15.6,14.4 C15.1,14.1 14.6,13.8 14.6,13.8 M12,18 C12.5,18 12.9,17.6 12.9,17.1 C12.9,16.6 12.5,16.2 12,16.2 C11.5,16.2 11.1,16.6 11.1,17.1 C11.1,17.6 11.5,18 12,18 Z",
            "gown" => "M48,10 Q60,22 72,10 L80,40 L70,62 C84,100 98,140 104,172 L16,172 C22,140 36,100 50,62 L40,40 Z",
            "suit" => "M44,10 L60,34 L76,10 L104,24 L112,70 L98,74 L96,168 L24,168 L22,74 L8,70 L16,24 Z",
            "kaba" => "M40,14 Q60,30 80,14 L108,36 L96,62 L84,54 L88,160 L32,160 L36,54 L24,62 L12,36 Z",
            "skirt" => "M38,20 L82,20 L108,168 L12,168 Z",
            _ => "M12,5 L12,19 M5,12 L19,12"
        };

        return new Path
        {
            Data = Geometry.Parse(pathData),
            Stroke = stroke,
            Fill = fill ?? Brushes.Transparent,
            Width = size,
            Height = size,
            Stretch = Stretch.Uniform,
            StrokeThickness = strokeThickness,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeLineJoin = PenLineJoin.Round,
            SnapsToDevicePixels = true
        };
    }

    private FrameworkElement CreateLogoMark(double size = 90)
    {
        var vb = new Viewbox { Width = size, Height = size, Stretch = Stretch.Uniform };
        var canvas = new Canvas { Width = 100, Height = 100 };

        // Continuous S-Thread in Tape Gold
        var thread = new Path
        {
            Data = Geometry.Parse("M68,28 C56,18 32,22 32,36 C32,52 68,48 68,64 C68,78 44,82 32,72"),
            Stroke = (Brush)FindResource("BrushTape"),
            StrokeThickness = 11,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeLineJoin = PenLineJoin.Round,
            Fill = Brushes.Transparent,
            SnapsToDevicePixels = true
        };
        canvas.Children.Add(thread);

        // Needle Eye Ring in Rose
        var eye = new Ellipse
        {
            Width = 16,
            Height = 16,
            Stroke = (Brush)FindResource("BrushRose"),
            StrokeThickness = 4,
            Fill = Brushes.Transparent,
            SnapsToDevicePixels = true
        };
        Canvas.SetLeft(eye, 60);
        Canvas.SetTop(eye, 20);
        canvas.Children.Add(eye);

        // Knot Point in Rose
        var knot = new Ellipse
        {
            Width = 14,
            Height = 14,
            Fill = (Brush)FindResource("BrushRose"),
            SnapsToDevicePixels = true
        };
        Canvas.SetLeft(knot, 25);
        Canvas.SetTop(knot, 65);
        canvas.Children.Add(knot);

        vb.Child = canvas;
        return vb;
    }

    private FrameworkElement CreateGarmentImage(string fileName, double width, double height, CornerRadius cornerRadius, string fallbackIcon = "gown")
    {
        var border = new Border
        {
            Width = width,
            Height = height,
            CornerRadius = cornerRadius,
            ClipToBounds = true,
            Background = (Brush)FindResource("BrushChalk"),
            SnapsToDevicePixels = true
        };

        try
        {
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                string localPath = fileName;
                if (!System.IO.File.Exists(localPath))
                {
                    var cleanName = System.IO.Path.GetFileName(fileName);
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    localPath = System.IO.Path.Combine(baseDir, "Assets", "Images", cleanName);
                    if (!System.IO.File.Exists(localPath))
                    {
                        var devPath = System.IO.Path.Combine(baseDir, "..", "..", "..", "Assets", "Images", cleanName);
                        if (System.IO.File.Exists(devPath)) localPath = devPath;
                    }
                }

                if (System.IO.File.Exists(localPath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(System.IO.Path.GetFullPath(localPath), UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    var image = new Image
                    {
                        Source = bitmap,
                        Stretch = Stretch.UniformToFill,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
                    border.Child = image;
                    return border;
                }
            }
        }
        catch { }

        border.Child = CreateGarmentVisual(fallbackIcon, size: Math.Min(width, height) * 0.7);
        return border;
    }

    private FrameworkElement CreateGarmentVisual(string garment, double size = 80, Brush? fill = null)
    {
        fill ??= (Brush)FindResource("BrushIndigo");
        var gKey = garment.ToLowerInvariant();

        var vb = new Viewbox { Width = size, Height = size * 1.35, Stretch = Stretch.Uniform };
        var canvas = new Canvas { Width = 120, Height = 180, ClipToBounds = true };

        if (gKey is "gown" or "robe")
        {
            // Couture Evening Gown with Halter sweetheart neckline, golden sash & flared mermaid silhouette
            var gownBase = new Path
            {
                Data = Geometry.Parse("M48,10 Q60,22 72,10 L80,40 L70,62 C84,100 98,140 104,172 L16,172 C22,140 36,100 50,62 L40,40 Z"),
                Fill = fill,
                SnapsToDevicePixels = true
            };
            canvas.Children.Add(gownBase);

            // Sweetheart neckline inner shade
            var neck = new Path
            {
                Data = Geometry.Parse("M48,10 Q60,24 72,10 Q60,16 48,10 Z"),
                Fill = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255))
            };
            canvas.Children.Add(neck);

            // Golden Waist Sash
            var sash = new Path
            {
                Data = Geometry.Parse("M49,58 Q60,62 71,58 L72,66 Q60,70 48,66 Z"),
                Fill = (Brush)FindResource("BrushTape"),
                SnapsToDevicePixels = true
            };
            canvas.Children.Add(sash);

            // Ribbon knot
            var bow = new Ellipse { Width = 8, Height = 8, Fill = (Brush)FindResource("BrushRose") };
            Canvas.SetLeft(bow, 56);
            Canvas.SetTop(bow, 60);
            canvas.Children.Add(bow);

            // Drape Seams
            var drape1 = new Path
            {
                Data = Geometry.Parse("M54,66 Q52,110 42,172"),
                Stroke = new SolidColorBrush(Color.FromArgb(45, 255, 255, 255)),
                StrokeThickness = 1.2
            };
            var drape2 = new Path
            {
                Data = Geometry.Parse("M66,66 Q68,110 78,172"),
                Stroke = new SolidColorBrush(Color.FromArgb(45, 255, 255, 255)),
                StrokeThickness = 1.2
            };
            canvas.Children.Add(drape1);
            canvas.Children.Add(drape2);
        }
        else if (gKey is "suit" or "costume")
        {
            // Tailored 2-piece Blazer with crisp peaked lapels, shirt collar, necktie & pocket square
            var suitBase = new Path
            {
                Data = Geometry.Parse("M44,10 L60,34 L76,10 L104,24 L112,70 L98,74 L96,168 L24,168 L22,74 L8,70 L16,24 Z"),
                Fill = fill,
                SnapsToDevicePixels = true
            };
            canvas.Children.Add(suitBase);

            // Inner Crisp White Dress Shirt
            var shirt = new Path
            {
                Data = Geometry.Parse("M46,10 L60,34 L74,10 Z"),
                Fill = Brushes.White,
                SnapsToDevicePixels = true
            };
            canvas.Children.Add(shirt);

            // Silk Necktie (Rose)
            var tie = new Path
            {
                Data = Geometry.Parse("M57,18 L63,18 L64,48 L60,56 L56,48 Z"),
                Fill = (Brush)FindResource("BrushRose"),
                SnapsToDevicePixels = true
            };
            canvas.Children.Add(tie);

            // Lapel Crease Lines
            var lapel1 = new Path { Data = Geometry.Parse("M44,10 L60,44"), Stroke = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)), StrokeThickness = 1.5 };
            var lapel2 = new Path { Data = Geometry.Parse("M76,10 L60,44"), Stroke = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)), StrokeThickness = 1.5 };
            canvas.Children.Add(lapel1);
            canvas.Children.Add(lapel2);

            // Pocket Square (White fold in chest pocket)
            var pocket = new Path
            {
                Data = Geometry.Parse("M30,60 L44,60 M32,60 L36,54 L40,60"),
                Stroke = Brushes.White,
                StrokeThickness = 2,
                Fill = Brushes.White
            };
            canvas.Children.Add(pocket);

            // Golden Horn Buttons
            var btn1 = new Ellipse { Width = 5, Height = 5, Fill = (Brush)FindResource("BrushTape") };
            Canvas.SetLeft(btn1, 57.5);
            Canvas.SetTop(btn1, 72);
            var btn2 = new Ellipse { Width = 5, Height = 5, Fill = (Brush)FindResource("BrushTape") };
            Canvas.SetLeft(btn2, 57.5);
            Canvas.SetTop(btn2, 92);
            canvas.Children.Add(btn1);
            canvas.Children.Add(btn2);
        }
        else if (gKey is "kaba" or "pagne")
        {
            // Traditional African Boubou / Kaba with ornate golden embroidered collar and cuff borders
            var kabaBase = new Path
            {
                Data = Geometry.Parse("M40,14 Q60,30 80,14 L108,36 L96,62 L84,54 L88,160 L32,160 L36,54 L24,62 L12,36 Z"),
                Fill = fill,
                SnapsToDevicePixels = true
            };
            canvas.Children.Add(kabaBase);

            // Golden Embroidered Neck Collar
            var collar = new Path
            {
                Data = Geometry.Parse("M42,16 Q60,38 78,16 L76,46 Q60,60 44,46 Z"),
                Fill = (Brush)FindResource("BrushTape"),
                SnapsToDevicePixels = true
            };
            canvas.Children.Add(collar);

            // Inner delicate neckline cutout
            var innerNeck = new Path
            {
                Data = Geometry.Parse("M47,20 Q60,32 73,20 Z"),
                Fill = new SolidColorBrush(Color.FromArgb(80, 20, 23, 58))
            };
            canvas.Children.Add(innerNeck);

            // Sleeve cuff gold accents
            var cuff1 = new Path { Data = Geometry.Parse("M12,36 L24,62"), Stroke = (Brush)FindResource("BrushTape"), StrokeThickness = 3 };
            var cuff2 = new Path { Data = Geometry.Parse("M108,36 L96,62"), Stroke = (Brush)FindResource("BrushTape"), StrokeThickness = 3 };
            canvas.Children.Add(cuff1);
            canvas.Children.Add(cuff2);
        }
        else // "skirt" or "jupe"
        {
            // High-waisted structured pleated midi skirt
            var skirtBase = new Path
            {
                Data = Geometry.Parse("M38,20 L82,20 L108,168 L12,168 Z"),
                Fill = (Brush)FindResource("BrushGreenDark"),
                SnapsToDevicePixels = true
            };
            canvas.Children.Add(skirtBase);

            // Waistband
            var wb = new Path
            {
                Data = Geometry.Parse("M36,20 L84,20 L84,32 L36,32 Z"),
                Fill = (Brush)FindResource("BrushInk"),
                SnapsToDevicePixels = true
            };
            canvas.Children.Add(wb);

            // Gold buckle
            var buckle = new Border { Width = 12, Height = 10, CornerRadius = new CornerRadius(2), BorderBrush = (Brush)FindResource("BrushTape"), BorderThickness = new Thickness(2) };
            Canvas.SetLeft(buckle, 54);
            Canvas.SetTop(buckle, 21);
            canvas.Children.Add(buckle);

            // Pleat Lines
            for (int p = 30; p <= 90; p += 15)
            {
                var pl = new Path
                {
                    Data = Geometry.Parse($"M{p},32 L{(int)(60 + (p - 60) * 1.5)},168"),
                    Stroke = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
                    StrokeThickness = 1.2
                };
                canvas.Children.Add(pl);
            }
        }

        vb.Child = canvas;
        return vb;
    }

    private Border BuildIconBtn(string iconKey, Action onClick)
    {
        var b = new Border
        {
            Width = 42,
            Height = 42,
            CornerRadius = new CornerRadius(14),
            Background = Brushes.White,
            BorderBrush = (Brush)FindResource("BrushLine"),
            BorderThickness = new Thickness(1),
            Cursor = Cursors.Hand,
            SnapsToDevicePixels = true
        };

        FrameworkElement child;
        if (iconKey is "←" or "back")
            child = CreateIcon("back", (Brush)FindResource("BrushInk"), size: 16);
        else if (iconKey is "✕" or "x" or "close")
            child = CreateIcon("close", (Brush)FindResource("BrushInk"), size: 16);
        else if (iconKey is "✓" or "check")
            child = CreateIcon("check", (Brush)FindResource("BrushGreen"), size: 16, strokeThickness: 2.2);
        else if (iconKey is "✏️" or "edit")
            child = CreateIcon("edit", (Brush)FindResource("BrushInk"), size: 16);
        else if (iconKey is "⋯" or "more")
            child = CreateIcon("more", (Brush)FindResource("BrushInk"), size: 16);
        else if (iconKey is "↗" or "share")
            child = CreateIcon("share", (Brush)FindResource("BrushInk"), size: 16);
        else if (iconKey is "❤️" or "heart")
            child = CreateIcon("heart", (Brush)FindResource("BrushRose"), fill: (Brush)FindResource("BrushRose"), size: 18);
        else if (iconKey is "filter" or "⚙️")
            child = CreateIcon("filter", (Brush)FindResource("BrushInk"), size: 18);
        else
            child = new TextBlock { Text = iconKey, FontSize = 16, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

        child.VerticalAlignment = VerticalAlignment.Center;
        child.HorizontalAlignment = HorizontalAlignment.Center;
        b.Child = child;

        b.MouseEnter += (s, e) => b.Background = (Brush)new BrushConverter().ConvertFrom("#F0F1FA")!;
        b.MouseLeave += (s, e) => b.Background = Brushes.White;
        b.MouseLeftButtonDown += (s, e) => onClick();
        return b;
    }

    private FrameworkElement CreateTape(bool isSmall = false)
    {
        var height = isSmall ? 16 : 22;
        var border = new Border
        {
            Height = height,
            Background = (Brush)FindResource("BrushTape"),
            CornerRadius = new CornerRadius(isSmall ? 4 : 0),
            ClipToBounds = true,
            SnapsToDevicePixels = true
        };

        var stack = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Bottom };
        for (int i = 0; i < 55; i++)
        {
            var isLong = i % 5 == 0;
            stack.Children.Add(new Border
            {
                Width = isLong ? 1.5 : 1,
                Height = isLong ? (isSmall ? 9 : 13) : (isSmall ? 5 : 7),
                Background = (Brush)FindResource("BrushInk"),
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 5, 0),
                SnapsToDevicePixels = true
            });
        }
        border.Child = stack;
        return border;
    }

    private FrameworkElement CreateStatusBadge(OrderStatus status)
    {
        var (text, bg, fg) = status switch
        {
            OrderStatus.Recue => ("Reçue", "#E4E5F2", "#26306B"),
            OrderStatus.Couture => ("Couture", "#FDF3D6", "#8A6A10"),
            OrderStatus.Essayage => ("Essayage", "#FCE4EA", "#B8325A"),
            OrderStatus.Prete => ("Prête", "#DDF3E8", "#1E7A50"),
            OrderStatus.Livree => ("Livrée", "#ECECF2", "#6B7094"),
            _ => ("Reçue", "#E4E5F2", "#26306B")
        };

        var border = new Border
        {
            CornerRadius = new CornerRadius(999),
            Background = (Brush)new BrushConverter().ConvertFrom(bg)!,
            Padding = new Thickness(10, 4, 10, 4),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            SnapsToDevicePixels = true
        };
        border.Child = new TextBlock
        {
            Text = text,
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Foreground = (Brush)new BrushConverter().ConvertFrom(fg)!
        };
        return border;
    }

    private FrameworkElement CreateAvatar(string initials, string colorClass, double size = 46)
    {
        var (bg, fg) = colorClass switch
        {
            "a1" => ("#FCE4EA", "#B8325A"),
            "a2" => ("#FDF3D6", "#8A6A10"),
            "a3" => ("#E4E5F2", "#26306B"),
            "a4" => ("#DDF3E8", "#1E7A50"),
            _ => ("#FCE4EA", "#B8325A")
        };

        var border = new Border
        {
            Width = size,
            Height = size,
            CornerRadius = new CornerRadius(size / 2),
            Background = (Brush)new BrushConverter().ConvertFrom(bg)!,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            SnapsToDevicePixels = true
        };
        border.Child = new TextBlock
        {
            Text = initials,
            FontSize = size >= 70 ? 28 : (size >= 50 ? 18 : 15),
            FontWeight = FontWeights.Bold,
            Foreground = (Brush)new BrushConverter().ConvertFrom(fg)!,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        return border;
    }

    // =========================================================================
    // PROFESSIONAL REAL INTERACTIVE INPUT CONTROL
    // =========================================================================

    private FrameworkElement BuildInputCard(string label, string val, bool isPass, string icon, string helper = "", bool isError = false)
    {
        var container = new StackPanel();

        var border = new Border
        {
            CornerRadius = new CornerRadius(14),
            Background = isError ? (Brush)FindResource("BrushErrorSoft") : Brushes.White,
            BorderBrush = isError ? (Brush)FindResource("BrushError") : (Brush)FindResource("BrushLine"),
            BorderThickness = new Thickness(isError ? 1.5 : 1),
            Padding = new Thickness(14, 8, 14, 8),
            MinHeight = 58,
            Cursor = Cursors.IBeam,
            SnapsToDevicePixels = true
        };

        var contentStack = new StackPanel();

        // Label
        var lbl = new TextBlock
        {
            Text = label,
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Foreground = isError ? (Brush)FindResource("BrushError") : (Brush)FindResource("BrushMuted")
        };
        contentStack.Children.Add(lbl);

        // Input row
        var dock = new DockPanel { Margin = new Thickness(0, 2, 0, 0) };

        // Password / Trailing action
        TextBox? regularTextBox = null;
        PasswordBox? passwordBox = null;
        bool passwordShown = false;

        if (!string.IsNullOrEmpty(icon))
        {
            FrameworkElement trailingElement;
            if (icon == "👁️" || isPass)
            {
                var eyeBtn = new Border
                {
                    Width = 28,
                    Height = 28,
                    CornerRadius = new CornerRadius(6),
                    Background = Brushes.Transparent,
                    Cursor = Cursors.Hand,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(6, 0, 0, 0)
                };
                var eyePath = CreateIcon("eye", (Brush)FindResource("BrushMuted"), size: 16);
                eyeBtn.Child = eyePath;

                eyeBtn.MouseEnter += (s, e) => eyeBtn.Background = (Brush)new BrushConverter().ConvertFrom("#F0F1FA")!;
                eyeBtn.MouseLeave += (s, e) => eyeBtn.Background = Brushes.Transparent;
                trailingElement = eyeBtn;
            }
            else if (icon == "✓")
            {
                trailingElement = CreateIcon("check", (Brush)FindResource("BrushGreen"), size: 16, strokeThickness: 2.2);
                trailingElement.VerticalAlignment = VerticalAlignment.Center;
                trailingElement.Margin = new Thickness(6, 0, 0, 0);
            }
            else if (icon == "⚠️")
            {
                trailingElement = CreateIcon("alert", (Brush)FindResource("BrushError"), size: 16, strokeThickness: 2.0);
                trailingElement.VerticalAlignment = VerticalAlignment.Center;
                trailingElement.Margin = new Thickness(6, 0, 0, 0);
            }
            else if (icon == "✉️")
            {
                trailingElement = CreateIcon("mail", (Brush)FindResource("BrushMuted"), size: 16);
                trailingElement.VerticalAlignment = VerticalAlignment.Center;
                trailingElement.Margin = new Thickness(6, 0, 0, 0);
            }
            else if (icon == "📅")
            {
                trailingElement = CreateIcon("calendar", (Brush)FindResource("BrushMuted"), size: 16);
                trailingElement.VerticalAlignment = VerticalAlignment.Center;
                trailingElement.Margin = new Thickness(6, 0, 0, 0);
            }
            else if (icon == "▼")
            {
                var chev = CreateIcon("chevron", (Brush)FindResource("BrushMuted"), size: 14);
                chev.VerticalAlignment = VerticalAlignment.Center;
                chev.Margin = new Thickness(6, 0, 0, 0);
                chev.RenderTransform = new RotateTransform(90, 7, 7);
                trailingElement = chev;
            }
            else
            {
                trailingElement = new TextBlock
                {
                    Text = icon,
                    FontSize = 14,
                    Foreground = (Brush)FindResource("BrushMuted"),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(6, 0, 0, 0)
                };
            }

            DockPanel.SetDock(trailingElement, Dock.Right);
            dock.Children.Add(trailingElement);

            // Toggle password visibility handler
            if ((icon == "👁️" || isPass) && trailingElement is Border eyeBorder)
            {
                eyeBorder.MouseLeftButtonDown += (s, e) =>
                {
                    passwordShown = !passwordShown;
                    if (regularTextBox != null && passwordBox != null)
                    {
                        if (passwordShown)
                        {
                            regularTextBox.Text = passwordBox.Password;
                            passwordBox.Visibility = Visibility.Collapsed;
                            regularTextBox.Visibility = Visibility.Visible;
                            regularTextBox.Focus();
                            regularTextBox.CaretIndex = regularTextBox.Text.Length;
                        }
                        else
                        {
                            passwordBox.Password = regularTextBox.Text;
                            regularTextBox.Visibility = Visibility.Collapsed;
                            passwordBox.Visibility = Visibility.Visible;
                            passwordBox.Focus();
                        }
                    }
                };
            }
        }

        // Real input field
        if (isPass)
        {
            passwordBox = new PasswordBox
            {
                Password = val.Replace("•", "1"),
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("BrushInk"),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                CaretBrush = (Brush)FindResource("BrushIndigo"),
                Padding = new Thickness(0, 1, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            regularTextBox = new TextBox
            {
                Text = val.Replace("•", "1"),
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("BrushInk"),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                CaretBrush = (Brush)FindResource("BrushIndigo"),
                Padding = new Thickness(0, 1, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = Visibility.Collapsed
            };

            dock.Children.Add(passwordBox);
            dock.Children.Add(regularTextBox);

            passwordBox.GotFocus += (s, e) =>
            {
                if (!isError)
                {
                    border.BorderBrush = (Brush)FindResource("BrushIndigo");
                    border.BorderThickness = new Thickness(1.8);
                }
            };
            passwordBox.LostFocus += (s, e) =>
            {
                if (!isError)
                {
                    border.BorderBrush = (Brush)FindResource("BrushLine");
                    border.BorderThickness = new Thickness(1);
                }
            };

            regularTextBox.GotFocus += (s, e) =>
            {
                if (!isError)
                {
                    border.BorderBrush = (Brush)FindResource("BrushIndigo");
                    border.BorderThickness = new Thickness(1.8);
                }
            };
            regularTextBox.LostFocus += (s, e) =>
            {
                if (!isError)
                {
                    border.BorderBrush = (Brush)FindResource("BrushLine");
                    border.BorderThickness = new Thickness(1);
                }
            };
        }
        else
        {
            regularTextBox = new TextBox
            {
                Text = val,
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("BrushInk"),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                CaretBrush = (Brush)FindResource("BrushIndigo"),
                Padding = new Thickness(0, 1, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                IsReadOnly = false,
                Focusable = true,
                IsEnabled = true,
                IsTabStop = true
            };

            dock.Children.Add(regularTextBox);

            regularTextBox.GotFocus += (s, e) =>
            {
                if (!isError)
                {
                    border.BorderBrush = (Brush)FindResource("BrushIndigo");
                    border.BorderThickness = new Thickness(1.8);
                }
            };
            regularTextBox.LostFocus += (s, e) =>
            {
                if (!isError)
                {
                    border.BorderBrush = (Brush)FindResource("BrushLine");
                    border.BorderThickness = new Thickness(1);
                }
            };
        }

        contentStack.Children.Add(dock);
        border.Child = contentStack;

        border.MouseLeftButtonDown += (s, e) =>
        {
            if (passwordBox != null && passwordBox.Visibility == Visibility.Visible)
            {
                passwordBox.Focus();
            }
            else if (regularTextBox != null)
            {
                regularTextBox.Focus();
                regularTextBox.CaretIndex = regularTextBox.Text.Length;
            }
        };

        container.Children.Add(border);

        // Helper / Error Message below input
        if (!string.IsNullOrEmpty(helper))
        {
            var msgStack = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(4, 5, 4, 0) };
            if (isError)
            {
                var warnIcon = CreateIcon("alert", (Brush)FindResource("BrushError"), size: 13, strokeThickness: 1.8);
                warnIcon.Margin = new Thickness(0, 0, 4, 0);
                warnIcon.VerticalAlignment = VerticalAlignment.Center;
                msgStack.Children.Add(warnIcon);
            }
            var hText = new TextBlock
            {
                Text = helper,
                FontSize = 11.5,
                Foreground = isError ? (Brush)FindResource("BrushError") : (Brush)FindResource("BrushMuted"),
                VerticalAlignment = VerticalAlignment.Center
            };
            msgStack.Children.Add(hText);
            container.Children.Add(msgStack);
        }

        return container;
    }

    private FrameworkElement BuildChip(string text, bool isActive, Action? onClick = null)
    {
        var b = new Border
        {
            Height = 34,
            CornerRadius = new CornerRadius(999),
            Background = isActive ? (Brush)FindResource("BrushIndigo") : Brushes.White,
            BorderBrush = isActive ? (Brush)FindResource("BrushIndigo") : (Brush)FindResource("BrushLine"),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(14, 0, 14, 0),
            Margin = new Thickness(0, 0, 8, 0),
            Cursor = Cursors.Hand,
            SnapsToDevicePixels = true
        };
        b.Child = new TextBlock
        {
            Text = text,
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            Foreground = isActive ? Brushes.White : (Brush)FindResource("BrushMuted"),
            VerticalAlignment = VerticalAlignment.Center
        };

        if (!isActive)
        {
            b.MouseEnter += (s, e) => b.Background = (Brush)new BrushConverter().ConvertFrom("#F0F1FA")!;
            b.MouseLeave += (s, e) => b.Background = Brushes.White;
        }

        b.MouseLeftButtonDown += (s, e) => onClick?.Invoke();
        return b;
    }

    // =========================================================================
    // 1. SPLASH SCREEN (Page 1)
    // =========================================================================
    private FrameworkElement BuildSplashScreen()
    {
        var grid = new Grid { Background = (Brush)FindResource("BrushIndigo"), ClipToBounds = true };

        // 1. Top-center Radial Glow (#3D4CA6)
        var glowTop = new Border
        {
            Background = new RadialGradientBrush
            {
                Center = new Point(0.5, 0.0),
                GradientOrigin = new Point(0.5, 0.0),
                RadiusX = 1.2,
                RadiusY = 0.55,
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb(0x3D, 0x4C, 0xA6), 0.0),
                    new GradientStop(Color.FromArgb(0, 0x3D, 0x4C, 0xA6), 0.62)
                }
            }
        };
        grid.Children.Add(glowTop);

        // 2. Bottom-right Radial Glow (rgba(232,96,127, 0.38))
        var glowBot = new Border
        {
            Background = new RadialGradientBrush
            {
                Center = new Point(1.0, 1.0),
                GradientOrigin = new Point(1.0, 1.0),
                RadiusX = 0.9,
                RadiusY = 0.5,
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(97, 0xE8, 0x60, 0x7F), 0.0),
                    new GradientStop(Color.FromArgb(0, 0xE8, 0x60, 0x7F), 0.62)
                }
            }
        };
        grid.Children.Add(glowBot);

        // 3. Suture Curves (Figma: M-10 560 and M-10 90)
        var sutureCanvas = new Canvas { IsHitTestVisible = false };

        var goldSuture = new Path
        {
            Data = Geometry.Parse("M-10 560C70 470 130 640 210 560S330 440 360 500"),
            Stroke = new SolidColorBrush(Color.FromArgb(97, 0xF4, 0xC9, 0x5D)),
            StrokeThickness = 2,
            StrokeDashArray = new DoubleCollection { 7, 9 },
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
            SnapsToDevicePixels = true
        };
        sutureCanvas.Children.Add(goldSuture);

        var whiteSuture = new Path
        {
            Data = Geometry.Parse("M-10 90C60 150 120 40 200 100S320 60 360 120"),
            Stroke = new SolidColorBrush(Color.FromArgb(36, 0xFF, 0xFF, 0xFF)),
            StrokeThickness = 1.5,
            StrokeDashArray = new DoubleCollection { 5, 9 },
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
            SnapsToDevicePixels = true
        };
        sutureCanvas.Children.Add(whiteSuture);
        grid.Children.Add(sutureCanvas);

        // 4. Center Content (Logo, Concentric Circles, Titles, CTA)
        var centerStack = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(20, 20, 20, 40)
        };

        // Concentric Circles + Logo squircle
        var logoContainer = new Grid { Width = 230, Height = 230, HorizontalAlignment = HorizontalAlignment.Center };

        // Outer subtle 1px ring (diameter 230)
        var outerRing = new Ellipse
        {
            Width = 230,
            Height = 230,
            Stroke = new SolidColorBrush(Color.FromArgb(31, 0xFF, 0xFF, 0xFF)),
            StrokeThickness = 1,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        logoContainer.Children.Add(outerRing);

        // Inner dashed golden ring (diameter 180)
        var dashedRing = new Ellipse
        {
            Width = 180,
            Height = 180,
            Stroke = new SolidColorBrush(Color.FromArgb(192, 0xF4, 0xC9, 0x5D)),
            StrokeThickness = 2,
            StrokeDashArray = new DoubleCollection { 6, 4 },
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        logoContainer.Children.Add(dashedRing);

        // Logo Squircle Tile (128x128)
        var squircle = new Border
        {
            Width = 128,
            Height = 128,
            CornerRadius = new CornerRadius(36),
            Background = (Brush)FindResource("GradIndigo"),
            BorderBrush = new SolidColorBrush(Color.FromArgb(36, 0xFF, 0xFF, 0xFF)),
            BorderThickness = new Thickness(1),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            SnapsToDevicePixels = true
        };
        squircle.Child = CreateLogoMark(86);
        logoContainer.Children.Add(squircle);

        centerStack.Children.Add(logoContainer);

        // Brand Name
        centerStack.Children.Add(new TextBlock
        {
            Text = "Sweet Tailor",
            FontSize = 42,
            FontWeight = FontWeights.ExtraBold,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 32, 0, 8),
            TextAlignment = TextAlignment.Center
        });

        // Tagline
        centerStack.Children.Add(new TextBlock
        {
            Text = "Chaque mesure compte.",
            FontSize = 16,
            FontWeight = FontWeights.Medium,
            Foreground = (Brush)new BrushConverter().ConvertFrom("#C9CCEA")!,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        grid.Children.Add(centerStack);

        // 5. Bottom Tape Measure & Version
        var botStack = new StackPanel { VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Stretch };
        botStack.Children.Add(new TextBlock
        {
            Text = "Version 1.0",
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            Foreground = (Brush)new BrushConverter().ConvertFrom("#9EA4D6")!,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 14)
        });
        botStack.Children.Add(CreateTape(false));
        grid.Children.Add(botStack);

        // Clicking anywhere on splash navigates to Onboarding 1/3
        grid.Cursor = Cursors.Hand;
        grid.MouseLeftButtonDown += (s, e) => NavigateTo("Onboarding1");

        _splashTimer?.Stop();
        _splashTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1800) };
        _splashTimer.Tick += (_, _) =>
        {
            _splashTimer?.Stop();
            if (_currentScreen == "Splash")
                NavigateTo("Onboarding1");
        };
        _splashTimer.Start();

        return grid;
    }

    // =========================================================================
    // 2. ONBOARDING 1 (Page 2)
    // =========================================================================
    private FrameworkElement BuildOnboarding1Screen()
    {
        var grid = new Grid { Background = (Brush)FindResource("BrushChalk") };
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(390) });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80) });

        var art = new Border { Background = (Brush)FindResource("GradRose"), CornerRadius = new CornerRadius(0, 0, 40, 40) };
        var artCanvas = new Canvas();

        var skipBtn = new Border
        {
            CornerRadius = new CornerRadius(999),
            Background = new SolidColorBrush(Color.FromArgb(0xD9, 0xFF, 0xFF, 0xFF)),
            Padding = new Thickness(14, 6, 14, 6),
            Cursor = Cursors.Hand
        };
        skipBtn.Child = new TextBlock { Text = "Passer", FontSize = 12.5, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushIndigo") };
        skipBtn.MouseLeftButtonDown += (s, e) => NavigateTo("Login");
        Canvas.SetRight(skipBtn, 18);
        Canvas.SetTop(skipBtn, 10);
        artCanvas.Children.Add(skipBtn);

        // Floating "Taille 74 cm" dark indigo badge
        var tailleBadge = new Border
        {
            CornerRadius = new CornerRadius(14),
            Background = (Brush)FindResource("BrushIndigo"),
            Padding = new Thickness(13, 8, 13, 8),
            RenderTransform = new RotateTransform(-5),
            SnapsToDevicePixels = true
        };
        tailleBadge.Child = new TextBlock { Text = "Taille 74 cm", FontSize = 12.5, FontWeight = FontWeights.Bold, Foreground = Brushes.White };
        Canvas.SetLeft(tailleBadge, 18);
        Canvas.SetTop(tailleBadge, 58);
        artCanvas.Children.Add(tailleBadge);

        // Floating client card
        var card = new Border
        {
            Width = 230,
            Height = 220,
            CornerRadius = new CornerRadius(20),
            Background = Brushes.White,
            BorderBrush = (Brush)FindResource("BrushLine"),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(14, 12, 14, 0),
            RenderTransform = new RotateTransform(-5),
            SnapsToDevicePixels = true
        };
        var cardStack = new StackPanel();
        var cardHeader = new StackPanel { Orientation = Orientation.Horizontal };
        cardHeader.Children.Add(CreateAvatar("NM", "a1", 36));
        var cardHeaderTxt = new StackPanel { Margin = new Thickness(10, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
        cardHeaderTxt.Children.Add(new TextBlock { Text = "Nadège Mbarga", FontSize = 13, FontWeight = FontWeights.Bold });
        cardHeaderTxt.Children.Add(new TextBlock { Text = "Mesures du 12 sept.", FontSize = 10.5, Foreground = (Brush)FindResource("BrushMuted") });
        cardHeader.Children.Add(cardHeaderTxt);
        cardStack.Children.Add(cardHeader);

        string[] labels = { "Poitrine", "Taille", "Hanches", "Longueur robe" };
        string[] vals = { "92 cm", "74 cm", "98 cm", "112 cm" };
        for (int i = 0; i < labels.Length; i++)
        {
            var r = new DockPanel { Margin = new Thickness(0, 4, 0, 4) };
            r.Children.Add(new TextBlock { Text = labels[i], FontSize = 11.5, VerticalAlignment = VerticalAlignment.Center });
            var vb = new Border { CornerRadius = new CornerRadius(6), Background = (Brush)FindResource("BrushChalk"), Padding = new Thickness(6, 2, 6, 2) };
            vb.Child = new TextBlock { Text = vals[i], FontSize = 11.5, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushIndigo") };
            DockPanel.SetDock(vb, Dock.Right);
            r.Children.Add(vb);
            cardStack.Children.Add(r);
        }
        cardStack.Children.Add(CreateTape(true));
        card.Child = cardStack;

        Canvas.SetLeft(card, 50);
        Canvas.SetTop(card, 60);
        artCanvas.Children.Add(card);

        // Floating rotated tape measure at bottom of art
        var tapeFloat = new Border
        {
            Width = 260,
            Height = 20,
            CornerRadius = new CornerRadius(4),
            ClipToBounds = true,
            RenderTransform = new RotateTransform(-14),
            SnapsToDevicePixels = true
        };
        tapeFloat.Child = CreateTape(true);
        Canvas.SetLeft(tapeFloat, 20);
        Canvas.SetTop(tapeFloat, 320);
        artCanvas.Children.Add(tapeFloat);

        // Floating check circle badge in Rose
        var checkCirc = new Border
        {
            Width = 50,
            Height = 50,
            CornerRadius = new CornerRadius(25),
            Background = (Brush)FindResource("BrushRose"),
            SnapsToDevicePixels = true
        };
        checkCirc.Child = CreateIcon("check", Brushes.White, size: 24, strokeThickness: 2.4);
        Canvas.SetRight(checkCirc, 22);
        Canvas.SetTop(checkCirc, 305);
        artCanvas.Children.Add(checkCirc);

        art.Child = artCanvas;
        Grid.SetRow(art, 0);
        grid.Children.Add(art);

        // Text
        var textStack = new StackPanel { Margin = new Thickness(24, 20, 24, 0) };
        textStack.Children.Add(new TextBlock
        {
            Text = "Fini les cahiers, tout est dans votre poche",
            FontSize = 24,
            FontWeight = FontWeights.Bold,
            Foreground = (Brush)FindResource("BrushInk"),
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 28
        });
        textStack.Children.Add(new TextBlock
        {
            Text = "Notez les mesures de chaque client une seule fois. Retrouvez-les en un geste, même des mois plus tard.",
            FontSize = 13.5,
            Foreground = (Brush)FindResource("BrushMuted"),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 8, 0, 0),
            LineHeight = 18
        });
        Grid.SetRow(textStack, 1);
        grid.Children.Add(textStack);

        // Footer buttons
        var foot = new DockPanel { Margin = new Thickness(24, 0, 24, 20), VerticalAlignment = VerticalAlignment.Center };
        var dots = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        dots.Children.Add(new Border { Width = 26, Height = 8, CornerRadius = new CornerRadius(4), Background = (Brush)FindResource("BrushIndigo"), Margin = new Thickness(0, 0, 5, 0) });
        dots.Children.Add(new Border { Width = 8, Height = 8, CornerRadius = new CornerRadius(4), Background = (Brush)FindResource("BrushMist"), Margin = new Thickness(0, 0, 5, 0) });
        dots.Children.Add(new Border { Width = 8, Height = 8, CornerRadius = new CornerRadius(4), Background = (Brush)FindResource("BrushMist") });
        foot.Children.Add(dots);

        var nextBtn = new Button
        {
            Content = "Suivant →",
            Width = 120,
            Height = 48,
            Background = (Brush)FindResource("BrushIndigo"),
            Foreground = Brushes.White,
            FontWeight = FontWeights.Bold,
            FontSize = 14,
            Cursor = Cursors.Hand
        };
        nextBtn.Click += (s, e) => NavigateTo("Onboarding2");
        DockPanel.SetDock(nextBtn, Dock.Right);
        foot.Children.Add(nextBtn);

        Grid.SetRow(foot, 2);
        grid.Children.Add(foot);
        return grid;
    }

    // =========================================================================
    // 3. ONBOARDING 2 (Page 3)
    // =========================================================================
    private FrameworkElement BuildOnboarding2Screen()
    {
        var grid = new Grid { Background = (Brush)FindResource("BrushChalk") };
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(390) });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80) });

        var art = new Border { Background = (Brush)FindResource("GradTape"), CornerRadius = new CornerRadius(0, 0, 40, 40) };
        var artCanvas = new Canvas();

        var skipBtn = new Border
        {
            CornerRadius = new CornerRadius(999),
            Background = new SolidColorBrush(Color.FromArgb(0xD9, 0xFF, 0xFF, 0xFF)),
            Padding = new Thickness(14, 6, 14, 6),
            Cursor = Cursors.Hand
        };
        skipBtn.Child = new TextBlock { Text = "Passer", FontSize = 12.5, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushIndigo") };
        skipBtn.MouseLeftButtonDown += (s, e) => NavigateTo("Login");
        Canvas.SetRight(skipBtn, 18);
        Canvas.SetTop(skipBtn, 10);
        artCanvas.Children.Add(skipBtn);

        var soonBadge = new Border
        {
            CornerRadius = new CornerRadius(99),
            Background = (Brush)FindResource("BrushRose"),
            Padding = new Thickness(12, 7, 12, 7),
            SnapsToDevicePixels = true
        };
        var soonStack = new StackPanel { Orientation = Orientation.Horizontal };
        soonStack.Children.Add(CreateIcon("clock", Brushes.White, size: 14));
        soonStack.Children.Add(new TextBlock { Text = "  Dans 5 jours", FontSize = 12, FontWeight = FontWeights.Bold, Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center });
        soonBadge.Child = soonStack;
        Canvas.SetLeft(soonBadge, 24);
        Canvas.SetTop(soonBadge, 52);
        artCanvas.Children.Add(soonBadge);

        var r1 = BuildFloatingOrderRow("08", "OCT.", "Robe de soirée", "Nadège Mbarga", OrderStatus.Essayage, true);
        r1.RenderTransform = new RotateTransform(1.5);
        Canvas.SetLeft(r1, 30); Canvas.SetTop(r1, 96); artCanvas.Children.Add(r1);

        var r2 = BuildFloatingOrderRow("11", "OCT.", "Ensemble pagne", "Christelle Tchoua", OrderStatus.Couture, false);
        r2.RenderTransform = new RotateTransform(-2);
        Canvas.SetLeft(r2, 20); Canvas.SetTop(r2, 176); artCanvas.Children.Add(r2);

        var r3 = BuildFloatingOrderRow("17", "OCT.", "Robe de mariée", "Sandrine Kamga", OrderStatus.Couture, false);
        r3.RenderTransform = new RotateTransform(3);
        Canvas.SetLeft(r3, 34); Canvas.SetTop(r3, 256); artCanvas.Children.Add(r3);

        var readyBadge = new Border
        {
            CornerRadius = new CornerRadius(99),
            Background = Brushes.White,
            Padding = new Thickness(8, 8, 14, 8),
            SnapsToDevicePixels = true
        };
        var readyStack = new StackPanel { Orientation = Orientation.Horizontal };
        var readyDot = new Border
        {
            Width = 26,
            Height = 26,
            CornerRadius = new CornerRadius(13),
            Background = (Brush)FindResource("BrushGreen"),
            Margin = new Thickness(0, 0, 8, 0)
        };
        readyDot.Child = CreateIcon("check", Brushes.White, size: 14, strokeThickness: 3);
        readyStack.Children.Add(readyDot);
        readyStack.Children.Add(new TextBlock { Text = "Prête à retirer", FontSize = 12.5, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushInk"), VerticalAlignment = VerticalAlignment.Center });
        readyBadge.Child = readyStack;
        Canvas.SetLeft(readyBadge, 24);
        Canvas.SetTop(readyBadge, 338);
        artCanvas.Children.Add(readyBadge);

        art.Child = artCanvas;
        Grid.SetRow(art, 0); grid.Children.Add(art);

        // Text
        var textStack = new StackPanel { Margin = new Thickness(24, 20, 24, 0) };
        textStack.Children.Add(new TextBlock
        {
            Text = "Chaque commande, livrée à temps",
            FontSize = 24,
            FontWeight = FontWeights.Bold,
            Foreground = (Brush)FindResource("BrushInk"),
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 28
        });
        textStack.Children.Add(new TextBlock
        {
            Text = "Suivez l'échéance et le statut de chaque vêtement, de la coupe à l'essayage jusqu'à la livraison.",
            FontSize = 13.5,
            Foreground = (Brush)FindResource("BrushMuted"),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 8, 0, 0),
            LineHeight = 18
        });
        Grid.SetRow(textStack, 1); grid.Children.Add(textStack);

        // Footer buttons
        var foot = new DockPanel { Margin = new Thickness(24, 0, 24, 20), VerticalAlignment = VerticalAlignment.Center };
        var dots = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        dots.Children.Add(new Border { Width = 8, Height = 8, CornerRadius = new CornerRadius(4), Background = (Brush)FindResource("BrushMist"), Margin = new Thickness(0, 0, 5, 0) });
        dots.Children.Add(new Border { Width = 26, Height = 8, CornerRadius = new CornerRadius(4), Background = (Brush)FindResource("BrushIndigo"), Margin = new Thickness(0, 0, 5, 0) });
        dots.Children.Add(new Border { Width = 8, Height = 8, CornerRadius = new CornerRadius(4), Background = (Brush)FindResource("BrushMist") });
        foot.Children.Add(dots);

        var nextBtn = new Button
        {
            Content = "Suivant →",
            Width = 120,
            Height = 48,
            Background = (Brush)FindResource("BrushIndigo"),
            Foreground = Brushes.White,
            FontWeight = FontWeights.Bold,
            FontSize = 14,
            Cursor = Cursors.Hand
        };
        nextBtn.Click += (s, e) => NavigateTo("Onboarding3");
        DockPanel.SetDock(nextBtn, Dock.Right);
        foot.Children.Add(nextBtn);

        Grid.SetRow(foot, 2); grid.Children.Add(foot);
        return grid;
    }

    private FrameworkElement BuildFloatingOrderRow(string day, string month, string title, string client, OrderStatus status, bool isHot)
    {
        var b = new Border
        {
            Width = 270,
            Height = 68,
            CornerRadius = new CornerRadius(18),
            Background = Brushes.White,
            BorderBrush = (Brush)FindResource("BrushLine"),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(10, 8, 12, 8),
            SnapsToDevicePixels = true
        };
        var dock = new DockPanel();

        // Date box
        var dBox = new Border
        {
            Width = 46,
            CornerRadius = new CornerRadius(12),
            Background = isHot ? (Brush)FindResource("BrushRose") : (Brush)FindResource("BrushChalk"),
            Padding = new Thickness(0, 4, 0, 4),
            VerticalAlignment = VerticalAlignment.Center
        };
        var ds = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
        ds.Children.Add(new TextBlock { Text = day, FontSize = 16, FontWeight = FontWeights.ExtraBold, Foreground = isHot ? Brushes.White : (Brush)FindResource("BrushInk"), HorizontalAlignment = HorizontalAlignment.Center });
        ds.Children.Add(new TextBlock { Text = month, FontSize = 9, FontWeight = FontWeights.Bold, Foreground = isHot ? Brushes.White : (Brush)FindResource("BrushMuted"), HorizontalAlignment = HorizontalAlignment.Center });
        dBox.Child = ds;
        DockPanel.SetDock(dBox, Dock.Left);
        dock.Children.Add(dBox);

        var badge = CreateStatusBadge(status);
        DockPanel.SetDock(badge, Dock.Right);
        dock.Children.Add(badge);

        var mid = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 8, 0) };
        mid.Children.Add(new TextBlock { Text = title, FontSize = 13.5, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushInk") });
        mid.Children.Add(new TextBlock { Text = client, FontSize = 11, Foreground = (Brush)FindResource("BrushMuted") });
        dock.Children.Add(mid);

        b.Child = dock;
        b.Cursor = Cursors.Hand;
        b.MouseEnter += (s, e) => b.Background = (Brush)new BrushConverter().ConvertFrom("#F6F7FD")!;
        b.MouseLeave += (s, e) => b.Background = Brushes.White;
        b.MouseLeftButtonDown += (s, e) =>
        {
            var matched = _orderService.GetAllOrdersAsync().Result.FirstOrDefault(o => o.ClientName.Contains(client) || o.ModelName.Contains(title));
            if (matched != null) _selectedOrder = matched;
            NavigateTo("OrderDetail");
        };
        return b;
    }

    private FrameworkElement BuildFloatingOrderRow(Order order)
    {
        string day = order.DueDate.ToString("dd");
        string month = order.DueDate.ToString("MMM.", new System.Globalization.CultureInfo("fr-FR")).ToUpperInvariant();
        var b = BuildFloatingOrderRow(day, month, order.ModelName, order.ClientName, order.Status, order.IsHotDueDate);
        b.Width = double.NaN;
        b.HorizontalAlignment = HorizontalAlignment.Stretch;
        b.MouseLeftButtonDown += (s, e) =>
        {
            _selectedOrder = order;
            NavigateTo("OrderDetail");
        };
        return b;
    }

    // =========================================================================
    // 4. ONBOARDING 3 (Page 4)
    // =========================================================================
    private FrameworkElement BuildOnboarding3Screen()
    {
        var grid = new Grid { Background = (Brush)FindResource("BrushChalk") };
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(390) });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80) });

        var art = new Border { Background = (Brush)FindResource("GradMist"), CornerRadius = new CornerRadius(0, 0, 40, 40) };
        var artCanvas = new Canvas();

        var skipBtn = new Border
        {
            CornerRadius = new CornerRadius(999),
            Background = new SolidColorBrush(Color.FromArgb(0xD9, 0xFF, 0xFF, 0xFF)),
            Padding = new Thickness(14, 6, 14, 6),
            Cursor = Cursors.Hand
        };
        skipBtn.Child = new TextBlock { Text = "Passer", FontSize = 12.5, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushIndigo") };
        skipBtn.MouseLeftButtonDown += (s, e) => NavigateTo("Login");
        Canvas.SetRight(skipBtn, 18);
        Canvas.SetTop(skipBtn, 10);
        artCanvas.Children.Add(skipBtn);

        var collage = new Grid
        {
            Width = 290,
            RenderTransform = new RotateTransform(-7),
            RenderTransformOrigin = new Point(0.5, 0.5)
        };
        collage.ColumnDefinitions.Add(new ColumnDefinition());
        collage.ColumnDefinitions.Add(new ColumnDefinition());
        var colA = new StackPanel { Margin = new Thickness(0, 26, 5, 0) };
        colA.Children.Add(CreateGarmentImage("robe_soiree.jpg", 136, 170, new CornerRadius(16), "gown"));
        colA.Children.Add(new Border { Height = 10 });
        colA.Children.Add(CreateGarmentImage("kaba_africain.jpg", 136, 130, new CornerRadius(16), "kaba"));
        var colB = new StackPanel();
        colB.Children.Add(CreateGarmentImage("jupe_pagne.jpg", 136, 120, new CornerRadius(16), "skirt"));
        colB.Children.Add(new Border { Height = 10 });
        colB.Children.Add(CreateGarmentImage("costume_homme.jpg", 136, 180, new CornerRadius(16), "suit"));
        Grid.SetColumn(colA, 0);
        Grid.SetColumn(colB, 1);
        collage.Children.Add(colA);
        collage.Children.Add(colB);
        Canvas.SetLeft(collage, 34);
        Canvas.SetTop(collage, 54);
        artCanvas.Children.Add(collage);

        var heart = new Border
        {
            Width = 46,
            Height = 46,
            CornerRadius = new CornerRadius(23),
            Background = Brushes.White,
            SnapsToDevicePixels = true
        };
        heart.Child = CreateIcon("heart", (Brush)FindResource("BrushRose"), fill: (Brush)FindResource("BrushRose"), size: 20);
        Canvas.SetRight(heart, 26);
        Canvas.SetTop(heart, 86);
        artCanvas.Children.Add(heart);

        var catPill = new Border
        {
            CornerRadius = new CornerRadius(99),
            Background = (Brush)FindResource("BrushIndigo"),
            Padding = new Thickness(16, 9, 16, 9)
        };
        catPill.Child = new TextBlock { Text = "Catalogue de modèles", FontSize = 12.5, FontWeight = FontWeights.Bold, Foreground = Brushes.White };
        Canvas.SetLeft(catPill, 22);
        Canvas.SetTop(catPill, 338);
        artCanvas.Children.Add(catPill);

        art.Child = artCanvas;
        Grid.SetRow(art, 0); grid.Children.Add(art);

        // Text
        var textStack = new StackPanel { Margin = new Thickness(24, 20, 24, 0) };
        textStack.Children.Add(new TextBlock
        {
            Text = "Inspirez-vous, puis passez à la création",
            FontSize = 24,
            FontWeight = FontWeights.Bold,
            Foreground = (Brush)FindResource("BrushInk"),
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 28
        });
        textStack.Children.Add(new TextBlock
        {
            Text = "Parcourez un catalogue de modèles, enregistrez vos coups de cœur et liez-les à une commande.",
            FontSize = 13.5,
            Foreground = (Brush)FindResource("BrushMuted"),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 8, 0, 0),
            LineHeight = 18
        });
        Grid.SetRow(textStack, 1); grid.Children.Add(textStack);

        // Footer buttons
        var foot = new DockPanel { Margin = new Thickness(24, 0, 24, 20), VerticalAlignment = VerticalAlignment.Center };
        var dots = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        dots.Children.Add(new Border { Width = 8, Height = 8, CornerRadius = new CornerRadius(4), Background = (Brush)FindResource("BrushMist"), Margin = new Thickness(0, 0, 5, 0) });
        dots.Children.Add(new Border { Width = 8, Height = 8, CornerRadius = new CornerRadius(4), Background = (Brush)FindResource("BrushMist"), Margin = new Thickness(0, 0, 5, 0) });
        dots.Children.Add(new Border { Width = 26, Height = 8, CornerRadius = new CornerRadius(4), Background = (Brush)FindResource("BrushIndigo") });
        foot.Children.Add(dots);

        var startBtn = new Button
        {
            Content = "Commencer",
            Width = 140,
            Height = 48,
            Background = (Brush)FindResource("BrushRose"),
            Foreground = Brushes.White,
            FontWeight = FontWeights.Bold,
            FontSize = 14,
            Cursor = Cursors.Hand
        };
        startBtn.Click += (s, e) => NavigateTo("Login");
        DockPanel.SetDock(startBtn, Dock.Right);
        foot.Children.Add(startBtn);

        Grid.SetRow(foot, 2); grid.Children.Add(foot);
        return grid;
    }

    // =========================================================================
    // 5. CONNEXION / LOGIN (Page 5)
    // =========================================================================
    private FrameworkElement BuildLoginScreen()
    {
        var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var stack = new StackPanel { Margin = new Thickness(0, 0, 0, 30) };

        stack.Children.Add(CreateTape(false));

        var pad = new StackPanel { Margin = new Thickness(20, 20, 20, 0) };
        var logoBox = new Border
        {
            Width = 58,
            Height = 58,
            CornerRadius = new CornerRadius(18),
            Background = (Brush)FindResource("BrushIndigo"),
            HorizontalAlignment = HorizontalAlignment.Left,
            SnapsToDevicePixels = true
        };
        logoBox.Child = CreateLogoMark(40);
        pad.Children.Add(logoBox);

        pad.Children.Add(new TextBlock { Text = "Bon retour !", FontSize = 28, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushInk"), Margin = new Thickness(0, 16, 0, 4) });
        pad.Children.Add(new TextBlock { Text = "Connectez-vous pour retrouver vos clients et vos commandes.", FontSize = 13.5, Foreground = (Brush)FindResource("BrushMuted") });

        // Fields
        var f1 = BuildInputCard("E-mail ou téléphone", "rose.ndongo@mail.com", false, "✉️");
        f1.Margin = new Thickness(0, 20, 0, 10);
        pad.Children.Add(f1);

        var f2 = BuildInputCard("Mot de passe", "••••••••", true, "👁️");
        pad.Children.Add(f2);

        var forgotLink = new TextBlock
        {
            Text = "Mot de passe oublié ?",
            FontSize = 13,
            FontWeight = FontWeights.Bold,
            Foreground = (Brush)FindResource("BrushIndigo"),
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 10, 0, 24),
            Cursor = Cursors.Hand
        };
        forgotLink.MouseLeftButtonDown += (s, e) => NavigateTo("ForgotPassword");
        pad.Children.Add(forgotLink);

        var loginBtn = new Button
        {
            Content = "Se connecter",
            Height = 52,
            Background = (Brush)FindResource("BrushIndigo"),
            Foreground = Brushes.White,
            FontWeight = FontWeights.Bold,
            FontSize = 15,
            Cursor = Cursors.Hand
        };
        loginBtn.Click += (s, e) =>
        {
            _isLoggedIn = true;
            NavigateTo("Clients");
        };
        pad.Children.Add(loginBtn);

        var regLink = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 36, 0, 10) };
        regLink.Children.Add(new TextBlock { Text = "Pas encore de compte ? ", FontSize = 13.5, Foreground = (Brush)FindResource("BrushMuted") });
        var regClick = new TextBlock
        {
            Text = "Créer un compte",
            FontSize = 13.5,
            FontWeight = FontWeights.Bold,
            Foreground = (Brush)FindResource("BrushIndigo"),
            Cursor = Cursors.Hand,
            TextDecorations = TextDecorations.Underline
        };
        regClick.MouseLeftButtonDown += (s, e) => NavigateTo("Register");
        regLink.Children.Add(regClick);
        pad.Children.Add(regLink);

        stack.Children.Add(pad);
        scroll.Content = stack;
        return scroll;
    }

    // =========================================================================
    // 6. MOT DE PASSE OUBLIÉ (Page 6)
    // =========================================================================
    private FrameworkElement BuildForgotPasswordScreen()
    {
        var stack = new StackPanel { Margin = new Thickness(20, 14, 20, 20) };
        var top = new DockPanel();
        var back = BuildIconBtn("back", () => NavigateTo("Login"));
        top.Children.Add(back);
        top.Children.Add(new TextBlock { Text = "Récupération", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushMuted"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center });
        stack.Children.Add(top);

        var iconBox = new Border { Width = 64, Height = 64, CornerRadius = new CornerRadius(20), Background = (Brush)FindResource("BrushRoseSoft"), Margin = new Thickness(0, 24, 0, 20), HorizontalAlignment = HorizontalAlignment.Left };
        iconBox.Child = CreateIcon("lock", (Brush)FindResource("BrushRoseDark"), size: 28);
        stack.Children.Add(iconBox);

        stack.Children.Add(new TextBlock { Text = "Mot de passe oublié ?", FontSize = 26, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushInk") });
        stack.Children.Add(new TextBlock { Text = "Entrez votre e-mail ou votre téléphone. Nous vous enverrons un code pour choisir un nouveau mot de passe.", FontSize = 13.5, Foreground = (Brush)FindResource("BrushMuted"), Margin = new Thickness(0, 8, 0, 24), TextWrapping = TextWrapping.Wrap });

        stack.Children.Add(BuildInputCard("E-mail ou téléphone", "rose.ndongo@mail.com", false, "✉️"));

        var sendBtn = new Button { Content = "Envoyer le code", Height = 52, Background = (Brush)FindResource("BrushIndigo"), Foreground = Brushes.White, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 20, 0, 0), Cursor = Cursors.Hand };
        sendBtn.Click += (s, e) => MessageBox.Show("Code de récupération envoyé avec succès !", "Sweet Tailor", MessageBoxButton.OK, MessageBoxImage.Information);
        stack.Children.Add(sendBtn);

        var backLink = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 40, 0, 10) };
        backLink.Children.Add(new TextBlock { Text = "Vous vous souvenez ? ", FontSize = 13.5, Foreground = (Brush)FindResource("BrushMuted") });
        var backClick = new TextBlock { Text = "Retour à la connexion", FontSize = 13.5, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushIndigo"), Cursor = Cursors.Hand, TextDecorations = TextDecorations.Underline };
        backClick.MouseLeftButtonDown += (s, e) => NavigateTo("Login");
        backLink.Children.Add(backClick);
        stack.Children.Add(backLink);

        return stack;
    }

    // =========================================================================
    // 7. INSCRIPTION / VALIDATION (Page 7)
    // =========================================================================
    private FrameworkElement BuildRegisterScreen()
    {
        var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var stack = new StackPanel { Margin = new Thickness(20, 14, 20, 30) };

        var top = new DockPanel();
        top.Children.Add(BuildIconBtn("back", () => NavigateTo("Login")));
        top.Children.Add(new TextBlock { Text = "Inscription", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushMuted"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center });
        stack.Children.Add(top);

        stack.Children.Add(new TextBlock { Text = "Créer un compte", FontSize = 26, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushInk"), Margin = new Thickness(0, 14, 0, 2) });
        stack.Children.Add(new TextBlock { Text = "Quelques infos pour ouvrir votre carnet.", FontSize = 13.5, Foreground = (Brush)FindResource("BrushMuted"), Margin = new Thickness(0, 0, 0, 16) });

        stack.Children.Add(BuildInputCard("Nom complet", "Rose Ndongo", false, "✓"));
        var fPhone = BuildInputCard("Téléphone", "+237 6 77 00 00 00", false, "✓"); fPhone.Margin = new Thickness(0, 8, 0, 0); stack.Children.Add(fPhone);
        var fEmail = BuildInputCard("E-mail", "rose.ndongo@mail", false, "⚠️", "Adresse e-mail invalide. Exemple : nom@mail.com", true); fEmail.Margin = new Thickness(0, 8, 0, 0); stack.Children.Add(fEmail);
        var fPass = BuildInputCard("Mot de passe", "••••••", true, "👁️", "8 caractères minimum, dont 1 chiffre"); fPass.Margin = new Thickness(0, 8, 0, 0); stack.Children.Add(fPass);
        var fConf = BuildInputCard("Confirmer le mot de passe", "•••••••", true, "⚠️", "Les mots de passe ne correspondent pas", true); fConf.Margin = new Thickness(0, 8, 0, 0); stack.Children.Add(fConf);

        // CGU Checkbox (Fully interactive!)
        bool isChecked = false;
        var btn = new Button
        {
            Content = "Créer mon compte",
            Height = 52,
            Background = (Brush)new BrushConverter().ConvertFrom("#C9CCEA")!,
            Foreground = Brushes.White,
            FontWeight = FontWeights.Bold,
            FontSize = 15,
            IsEnabled = false,
            Cursor = Cursors.Hand
        };
        btn.Click += (s, e) =>
        {
            _isLoggedIn = true;
            NavigateTo("Clients");
        };

        var cgu = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 14, 0, 16), Cursor = Cursors.Hand };
        var checkBorder = new Border
        {
            Width = 22,
            Height = 22,
            CornerRadius = new CornerRadius(6),
            BorderBrush = (Brush)FindResource("BrushMuted"),
            BorderThickness = new Thickness(1.5),
            Background = Brushes.White,
            Margin = new Thickness(0, 0, 10, 0),
            SnapsToDevicePixels = true
        };

        cgu.MouseLeftButtonDown += (s, e) =>
        {
            isChecked = !isChecked;
            if (isChecked)
            {
                checkBorder.Background = (Brush)FindResource("BrushIndigo");
                checkBorder.BorderBrush = (Brush)FindResource("BrushIndigo");
                checkBorder.Child = CreateIcon("check", Brushes.White, size: 14, strokeThickness: 2.2);
                btn.IsEnabled = true;
                btn.Background = (Brush)FindResource("BrushIndigo");
            }
            else
            {
                checkBorder.Background = Brushes.White;
                checkBorder.BorderBrush = (Brush)FindResource("BrushMuted");
                checkBorder.Child = null;
                btn.IsEnabled = false;
                btn.Background = (Brush)new BrushConverter().ConvertFrom("#C9CCEA")!;
            }
        };

        cgu.Children.Add(checkBorder);
        var cguText = new TextBlock { Text = "J'accepte les conditions d'utilisation", FontSize = 12.5, Foreground = (Brush)FindResource("BrushInk"), VerticalAlignment = VerticalAlignment.Center };
        cgu.Children.Add(cguText);
        stack.Children.Add(cgu);

        stack.Children.Add(btn);

        // Footer: Switch to Login
        var regFooter = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 24, 0, 10) };
        regFooter.Children.Add(new TextBlock { Text = "Déjà un compte ? ", FontSize = 13.5, Foreground = (Brush)FindResource("BrushMuted") });
        var logLink = new TextBlock { Text = "Se connecter", FontSize = 13.5, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushIndigo"), Cursor = Cursors.Hand, TextDecorations = TextDecorations.Underline };
        logLink.MouseLeftButtonDown += (s, e) => NavigateTo("Login");
        regFooter.Children.Add(logLink);
        stack.Children.Add(regFooter);

        scroll.Content = stack;
        return scroll;
    }

    // =========================================================================
    // 8. MES CLIENTS - ACCUEIL (Page 8)
    // =========================================================================
    private FrameworkElement BuildClientsScreen()
    {
        var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var stack = new StackPanel { Margin = new Thickness(0, 10, 0, 96) };

        // Topbar
        var top = new DockPanel { Margin = new Thickness(20, 0, 20, 12) };
        var filterBtn = BuildIconBtn("filter", () => NavigateTo("Search"));
        DockPanel.SetDock(filterBtn, Dock.Right);
        top.Children.Add(filterBtn);

        var topText = new StackPanel();
        topText.Children.Add(new TextBlock { Text = "Atelier de Rose", FontSize = 12.5, Foreground = (Brush)FindResource("BrushMuted") });
        topText.Children.Add(new TextBlock { Text = "Mes clients", FontSize = 28, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushInk") });
        top.Children.Add(topText);
        stack.Children.Add(top);

        // Search Bar (Interactive click opens search)
        var searchBar = new Border
        {
            Margin = new Thickness(20, 0, 20, 14),
            Height = 48,
            CornerRadius = new CornerRadius(14),
            Background = Brushes.White,
            BorderBrush = (Brush)FindResource("BrushLine"),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(14, 0, 14, 0),
            Cursor = Cursors.Hand,
            SnapsToDevicePixels = true
        };
        var searchDock = new DockPanel { VerticalAlignment = VerticalAlignment.Center };
        var searchIcon = CreateIcon("search", (Brush)FindResource("BrushMuted"), size: 18);
        searchIcon.Margin = new Thickness(0, 0, 10, 0);
        searchDock.Children.Add(searchIcon);
        searchDock.Children.Add(new TextBlock { Text = "Rechercher un client ou une commande", FontSize = 13.5, Foreground = (Brush)FindResource("BrushMuted"), VerticalAlignment = VerticalAlignment.Center });
        searchBar.Child = searchDock;
        searchBar.MouseLeftButtonDown += (s, e) => NavigateTo("Search");
        stack.Children.Add(searchBar);

        // Filter Chips
        var chips = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(20, 0, 20, 14) };
        chips.Children.Add(BuildChip("Tous · 48", true));
        chips.Children.Add(BuildChip("En cours · 12", false));
        chips.Children.Add(BuildChip("À livrer · 5", false));
        chips.Children.Add(BuildChip("Livrés", false));
        stack.Children.Add(chips);

        // Banner: 3 livraisons à venir with tape
        var banner = new Border
        {
            Margin = new Thickness(20, 0, 20, 16),
            CornerRadius = new CornerRadius(20),
            Background = (Brush)FindResource("BrushIndigo"),
            ClipToBounds = true,
            SnapsToDevicePixels = true
        };
        var bannerStack = new StackPanel();
        var bannerContent = new DockPanel { Margin = new Thickness(16, 14, 16, 12) };
        var calBox = new Border { Width = 40, Height = 40, CornerRadius = new CornerRadius(12), Background = new SolidColorBrush(Color.FromArgb(0x28, 0xFF, 0xFF, 0xFF)) };
        calBox.Child = CreateIcon("calendar", Brushes.White, size: 20);
        DockPanel.SetDock(calBox, Dock.Right);
        bannerContent.Children.Add(calBox);

        var bText = new StackPanel();
        bText.Children.Add(new TextBlock { Text = "Cette semaine", FontSize = 12, Foreground = (Brush)new BrushConverter().ConvertFrom("#C9CCEA")! });
        bText.Children.Add(new TextBlock { Text = "3 livraisons à venir", FontSize = 20, FontWeight = FontWeights.Bold, Foreground = Brushes.White, Margin = new Thickness(0, 2, 0, 0) });
        bannerContent.Children.Add(bText);
        bannerStack.Children.Add(bannerContent);

        bannerStack.Children.Add(CreateTape(true));
        banner.Child = bannerStack;
        stack.Children.Add(banner);

        // Client Rows
        var clientsList = new StackPanel { Margin = new Thickness(20, 0, 20, 0) };
        var allClients = _clientService.GetClientsAsync().Result;
        foreach (var client in allClients)
        {
            clientsList.Children.Add(BuildClientRow(client));
        }
        stack.Children.Add(clientsList);

        // Grid with Screen Content and Floating Action Button (FAB)
        var rootGrid = new Grid();
        scroll.Content = stack;
        rootGrid.Children.Add(scroll);

        // FAB positioned cleanly ABOVE the bottom navbar (bottom: 92px)
        var fab = new Border
        {
            Width = 56,
            Height = 56,
            CornerRadius = new CornerRadius(18),
            Background = (Brush)FindResource("BrushRose"),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 20, 92),
            Cursor = Cursors.Hand,
            SnapsToDevicePixels = true
        };
        fab.Child = CreateIcon("plus", Brushes.White, size: 24, strokeThickness: 2.6);
        fab.MouseEnter += (s, e) => fab.Background = (Brush)new BrushConverter().ConvertFrom("#D64E6F")!;
        fab.MouseLeave += (s, e) => fab.Background = (Brush)FindResource("BrushRose");
        fab.MouseLeftButtonDown += (s, e) => NavigateTo("ClientForm");
        rootGrid.Children.Add(fab);

        return rootGrid;
    }

    private FrameworkElement BuildClientRow(Client client)
    {
        var border = new Border
        {
            CornerRadius = new CornerRadius(18),
            Background = Brushes.White,
            BorderBrush = (Brush)FindResource("BrushLine"),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(14, 12, 14, 12),
            Margin = new Thickness(0, 0, 0, 10),
            Cursor = Cursors.Hand,
            SnapsToDevicePixels = true
        };
        var dock = new DockPanel();

        var avatar = CreateAvatar(client.AvatarInitials, client.AvatarColorClass, 46);
        DockPanel.SetDock(avatar, Dock.Left);
        dock.Children.Add(avatar);

        var badge = CreateStatusBadge(client.CurrentOrderStatus);
        DockPanel.SetDock(badge, Dock.Right);
        dock.Children.Add(badge);

        var mid = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 8, 0) };
        mid.Children.Add(new TextBlock { Text = client.FullName, FontSize = 14.5, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushInk") });
        var descText = !string.IsNullOrWhiteSpace(client.CurrentOrderSummary) ? client.CurrentOrderSummary : client.Address;
        mid.Children.Add(new TextBlock { Text = descText, FontSize = 12, Foreground = (Brush)FindResource("BrushMuted"), Margin = new Thickness(0, 2, 0, 0) });
        dock.Children.Add(mid);

        border.Child = dock;

        border.MouseEnter += (s, e) => border.Background = (Brush)new BrushConverter().ConvertFrom("#F6F7FD")!;
        border.MouseLeave += (s, e) => border.Background = Brushes.White;
        border.MouseLeftButtonDown += (s, e) =>
        {
            _selectedClient = client;
            NavigateTo("ClientDetail");
        };
        return border;
    }

    private FrameworkElement BuildClientRow(string init, string col, string name, string orderDesc, OrderStatus status)
    {
        var matched = _clientService.GetClientsAsync().Result.FirstOrDefault(c => c.FullName.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (matched != null)
        {
            return BuildClientRow(matched);
        }

        var fallbackClient = new Client
        {
            FullName = name,
            AvatarInitials = init,
            AvatarColorClass = col,
            CurrentOrderSummary = orderDesc,
            CurrentOrderStatus = status
        };
        return BuildClientRow(fallbackClient);
    }

    // =========================================================================
    // 9. CLIENTS : ÉTAT VIDE (Page 9)
    // =========================================================================
    private FrameworkElement BuildClientsEmptyScreen()
    {
        var stack = new StackPanel { Margin = new Thickness(20, 10, 20, 80) };

        var top = new DockPanel { Margin = new Thickness(0, 0, 0, 14) };
        var topText = new StackPanel();
        topText.Children.Add(new TextBlock { Text = "Atelier de Rose", FontSize = 12.5, Foreground = (Brush)FindResource("BrushMuted") });
        topText.Children.Add(new TextBlock { Text = "Mes clients", FontSize = 28, FontWeight = FontWeights.Bold });
        top.Children.Add(topText);
        stack.Children.Add(top);

        var searchBar = new Border
        {
            Height = 48,
            CornerRadius = new CornerRadius(14),
            Background = (Brush)new BrushConverter().ConvertFrom("#EEEFF6")!,
            BorderBrush = (Brush)new BrushConverter().ConvertFrom("#E2E3EF")!,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(14, 0, 14, 0),
            Margin = new Thickness(0, 0, 0, 40)
        };
        var sDock = new DockPanel { VerticalAlignment = VerticalAlignment.Center };
        sDock.Children.Add(CreateIcon("search", (Brush)new BrushConverter().ConvertFrom("#A6AAC8")!, size: 18));
        sDock.Children.Add(new TextBlock { Text = "Rechercher un client", FontSize = 13.5, Foreground = (Brush)new BrushConverter().ConvertFrom("#A6AAC8")!, Margin = new Thickness(8, 0, 0, 0) });
        searchBar.Child = sDock;
        stack.Children.Add(searchBar);

        // Glowing center illustration
        var glow = new Border
        {
            Width = 140,
            Height = 140,
            CornerRadius = new CornerRadius(70),
            Background = (Brush)FindResource("BrushMist"),
            HorizontalAlignment = HorizontalAlignment.Center,
            SnapsToDevicePixels = true
        };
        var noteCard = new Border
        {
            Width = 92,
            Height = 92,
            CornerRadius = new CornerRadius(24),
            Background = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        var noteGrid = new Grid();
        noteGrid.Children.Add(CreateIcon("users", (Brush)FindResource("BrushIndigo"), size: 36));

        var plusTag = new Border
        {
            Width = 28,
            Height = 28,
            CornerRadius = new CornerRadius(14),
            Background = (Brush)FindResource("BrushRose"),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 4, 4)
        };
        plusTag.Child = CreateIcon("plus", Brushes.White, size: 14, strokeThickness: 2.2);
        noteGrid.Children.Add(plusTag);

        noteCard.Child = noteGrid;
        glow.Child = noteCard;
        stack.Children.Add(glow);

        stack.Children.Add(new TextBlock { Text = "Aucun client pour le moment", FontSize = 20, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushInk"), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 24, 0, 0) });
        stack.Children.Add(new TextBlock { Text = "Ajoutez votre premier client pour enregistrer ses mesures et suivre ses commandes.", FontSize = 13.5, Foreground = (Brush)FindResource("BrushMuted"), TextAlignment = TextAlignment.Center, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(20, 8, 20, 24) });

        var addBtn = new Button { Content = "+ Ajouter un client", Height = 52, Background = (Brush)FindResource("BrushRose"), Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 15, Cursor = Cursors.Hand };
        addBtn.Click += (s, e) => NavigateTo("ClientForm");
        stack.Children.Add(addBtn);

        return stack;
    }

    // =========================================================================
    // 10. CLIENTS : CHARGEMENT (Page 10)
    // =========================================================================
    private FrameworkElement BuildClientsLoadingScreen()
    {
        var stack = new StackPanel { Margin = new Thickness(20, 10, 20, 80) };

        var top = new DockPanel { Margin = new Thickness(0, 0, 0, 14) };
        var topText = new StackPanel();
        topText.Children.Add(new TextBlock { Text = "Atelier de Rose", FontSize = 12.5, Foreground = (Brush)FindResource("BrushMuted") });
        topText.Children.Add(new TextBlock { Text = "Mes clients", FontSize = 28, FontWeight = FontWeights.Bold });
        top.Children.Add(topText);
        stack.Children.Add(top);

        var topBar = new Border { Height = 4, CornerRadius = new CornerRadius(2), Background = (Brush)FindResource("BrushTape"), Margin = new Thickness(0, 0, 0, 14) };
        stack.Children.Add(topBar);

        // Skeleton Banner
        var skelBanner = new Border { Height = 54, CornerRadius = new CornerRadius(16), Background = (Brush)FindResource("BrushMist"), Margin = new Thickness(0, 0, 0, 14) };
        stack.Children.Add(skelBanner);

        // Skeleton Chips
        var skelChips = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 18) };
        skelChips.Children.Add(new Border { Width = 70, Height = 28, CornerRadius = new CornerRadius(14), Background = (Brush)FindResource("BrushMist"), Margin = new Thickness(0, 0, 8, 0) });
        skelChips.Children.Add(new Border { Width = 80, Height = 28, CornerRadius = new CornerRadius(14), Background = (Brush)FindResource("BrushMist"), Margin = new Thickness(0, 0, 8, 0) });
        skelChips.Children.Add(new Border { Width = 70, Height = 28, CornerRadius = new CornerRadius(14), Background = (Brush)FindResource("BrushMist") });
        stack.Children.Add(skelChips);

        // 4 Skeleton Rows
        for (int i = 0; i < 4; i++)
        {
            var skelRow = new Border
            {
                Height = 64,
                CornerRadius = new CornerRadius(18),
                Background = Brushes.White,
                BorderBrush = (Brush)FindResource("BrushLine"),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 10)
            };
            var sDock = new DockPanel();
            sDock.Children.Add(new Border { Width = 40, Height = 40, CornerRadius = new CornerRadius(20), Background = (Brush)FindResource("BrushMist") });
            var pPill = new Border { Width = 56, Height = 20, CornerRadius = new CornerRadius(10), Background = (Brush)FindResource("BrushMist"), VerticalAlignment = VerticalAlignment.Center };
            DockPanel.SetDock(pPill, Dock.Right);
            sDock.Children.Add(pPill);

            var lines = new StackPanel { Margin = new Thickness(12, 4, 0, 0) };
            lines.Children.Add(new Border { Width = 110, Height = 12, CornerRadius = new CornerRadius(6), Background = (Brush)FindResource("BrushMist"), HorizontalAlignment = HorizontalAlignment.Left });
            lines.Children.Add(new Border { Width = 150, Height = 10, CornerRadius = new CornerRadius(5), Background = (Brush)FindResource("BrushMist"), HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 6, 0, 0) });
            sDock.Children.Add(lines);

            skelRow.Child = sDock;
            stack.Children.Add(skelRow);
        }

        stack.Children.Add(new TextBlock { Text = "Chargement de vos clients…", FontSize = 13, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushMuted"), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 16, 0, 0) });
        return stack;
    }

    // =========================================================================
    // 11. CLIENTS : ERREUR RÉSEAU (Page 11)
    // =========================================================================
    private FrameworkElement BuildClientsErrorScreen()
    {
        var stack = new StackPanel { Margin = new Thickness(20, 10, 20, 80) };

        var top = new DockPanel { Margin = new Thickness(0, 0, 0, 10) };
        var topText = new StackPanel();
        topText.Children.Add(new TextBlock { Text = "Atelier de Rose", FontSize = 12.5, Foreground = (Brush)FindResource("BrushMuted") });
        topText.Children.Add(new TextBlock { Text = "Mes clients", FontSize = 28, FontWeight = FontWeights.Bold });
        top.Children.Add(topText);
        stack.Children.Add(top);

        var errBanner = new Border
        {
            CornerRadius = new CornerRadius(12),
            Background = (Brush)FindResource("BrushErrorSoft"),
            BorderBrush = (Brush)FindResource("BrushError"),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(12, 10, 12, 10),
            Margin = new Thickness(0, 0, 0, 30)
        };
        var ebDock = new DockPanel();
        ebDock.Children.Add(CreateIcon("alert", (Brush)FindResource("BrushError"), size: 16));
        ebDock.Children.Add(new TextBlock { Text = "Impossible de charger la liste des clients.", FontSize = 12.5, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushError"), Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center });
        errBanner.Child = ebDock;
        stack.Children.Add(errBanner);

        // Center WiFi Error Glow
        var glow = new Border { Width = 140, Height = 140, CornerRadius = new CornerRadius(70), Background = (Brush)FindResource("BrushRoseSoft"), HorizontalAlignment = HorizontalAlignment.Center };
        var errCircle = new Border { Width = 92, Height = 92, CornerRadius = new CornerRadius(24), Background = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        errCircle.Child = CreateIcon("wifioff", (Brush)FindResource("BrushError"), size: 38);
        glow.Child = errCircle;
        stack.Children.Add(glow);

        stack.Children.Add(new TextBlock { Text = "Connexion impossible", FontSize = 22, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushInk"), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 24, 0, 0) });
        stack.Children.Add(new TextBlock { Text = "Vérifiez votre connexion Internet puis réessayez. Vos données ne sont pas perdues.\nCode : NET-01", FontSize = 13, Foreground = (Brush)FindResource("BrushMuted"), TextAlignment = TextAlignment.Center, Margin = new Thickness(20, 8, 20, 20) });

        var retryBtn = new Button { Content = "Réessayer", Height = 52, Background = (Brush)FindResource("BrushIndigo"), Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 15, Cursor = Cursors.Hand };
        retryBtn.Click += (s, e) => NavigateTo("Clients");
        stack.Children.Add(retryBtn);

        var homeBtn = new Button { Content = "Revenir à l'accueil", Height = 48, Background = Brushes.White, BorderBrush = (Brush)FindResource("BrushMist"), BorderThickness = new Thickness(1.5), FontWeight = FontWeights.Bold, FontSize = 14, Margin = new Thickness(0, 10, 0, 0), Cursor = Cursors.Hand };
        homeBtn.Click += (s, e) => NavigateTo("Clients");
        stack.Children.Add(homeBtn);

        return stack;
    }

    // =========================================================================
    // 12, 22, 23. FICHE CLIENT (Mesures, Commandes, Notes)
    // =========================================================================
    private FrameworkElement BuildClientDetailScreen(string activeTab)
    {
        var client = _selectedClient ?? _clientService.GetClientsAsync().Result.FirstOrDefault() ?? new Client { FullName = "Nadège Mbarga", AvatarInitials = "NM" };
        var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var stack = new StackPanel { Margin = new Thickness(20, 10, 20, 88) };

        // Topbar
        var top = new DockPanel();
        top.Children.Add(BuildIconBtn("back", () => NavigateTo("Clients")));
        var editBtn = BuildIconBtn("edit", () => NavigateTo("ClientForm"));
        DockPanel.SetDock(editBtn, Dock.Right);
        top.Children.Add(editBtn);
        top.Children.Add(new TextBlock { Text = "Fiche client", FontSize = 14, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center });
        stack.Children.Add(top);

        // Avatar + Name + Contact
        stack.Children.Add(CreateAvatar(client.AvatarInitials, client.AvatarColorClass, 80));
        stack.Children.Add(new TextBlock { Text = client.FullName, FontSize = 22, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 10, 0, 2) });
        stack.Children.Add(new TextBlock { Text = $"{client.PhoneNumber} · {client.Address}", FontSize = 12.5, Foreground = (Brush)FindResource("BrushMuted"), HorizontalAlignment = HorizontalAlignment.Center });

        // Call / WhatsApp (crisp vector buttons!)
        var actions = new UniformGrid { Columns = 2, Margin = new Thickness(10, 14, 10, 16) };

        var callBtn = new Button
        {
            Height = 44,
            Background = Brushes.White,
            BorderBrush = (Brush)FindResource("BrushMist"),
            BorderThickness = new Thickness(1.5),
            Margin = new Thickness(0, 0, 6, 0),
            Cursor = Cursors.Hand
        };
        var callStack = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
        callStack.Children.Add(CreateIcon("phone", (Brush)FindResource("BrushIndigo"), size: 16));
        callStack.Children.Add(new TextBlock { Text = "Appeler", FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushIndigo"), Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center });
        callBtn.Content = callStack;
        callBtn.Click += (s, e) => MessageBox.Show($"Appel vers {client.PhoneNumber}...", "Sweet Tailor");
        actions.Children.Add(callBtn);

        var chatBtn = new Button
        {
            Height = 44,
            Background = Brushes.White,
            BorderBrush = (Brush)FindResource("BrushMist"),
            BorderThickness = new Thickness(1.5),
            Margin = new Thickness(6, 0, 0, 0),
            Cursor = Cursors.Hand
        };
        var chatStack = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
        chatStack.Children.Add(CreateIcon("chat", (Brush)FindResource("BrushIndigo"), size: 16));
        chatStack.Children.Add(new TextBlock { Text = "WhatsApp", FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushIndigo"), Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center });
        chatBtn.Content = chatStack;
        chatBtn.Click += (s, e) => MessageBox.Show($"Ouverture WhatsApp avec {client.FullName}...", "Sweet Tailor");
        actions.Children.Add(chatBtn);
        stack.Children.Add(actions);

        // Segmented Tabs
        var tabContainer = new Border { CornerRadius = new CornerRadius(14), Background = (Brush)FindResource("BrushMist"), Padding = new Thickness(4) };
        var tabsGrid = new UniformGrid { Columns = 3 };

        var t1 = BuildTabButton("Mesures", activeTab == "Mesures", () => NavigateTo("ClientDetail"));
        var t2 = BuildTabButton("Commandes", activeTab == "Commandes", () => NavigateTo("ClientOrders"));
        var t3 = BuildTabButton("Notes", activeTab == "Notes", () => NavigateTo("ClientNotes"));

        tabsGrid.Children.Add(t1); tabsGrid.Children.Add(t2); tabsGrid.Children.Add(t3);
        tabContainer.Child = tabsGrid;
        stack.Children.Add(tabContainer);

        // TAB CONTENT
        if (activeTab == "Mesures")
        {
            var card = new Border
            {
                Margin = new Thickness(0, 16, 0, 14),
                CornerRadius = new CornerRadius(20),
                Background = Brushes.White,
                BorderBrush = (Brush)FindResource("BrushLine"),
                BorderThickness = new Thickness(1),
                ClipToBounds = true,
                SnapsToDevicePixels = true
            };
            var cs = new StackPanel();
            var ch = new DockPanel { Margin = new Thickness(16, 12, 16, 8) };
            var lastDate = client.Measurements.FirstOrDefault()?.LastUpdated.ToString("d MMM yyyy", new System.Globalization.CultureInfo("fr-FR")) ?? "12 sept. 2026";
            ch.Children.Add(new TextBlock { Text = lastDate, FontSize = 12, Foreground = (Brush)FindResource("BrushMuted"), VerticalAlignment = VerticalAlignment.Center });
            DockPanel.SetDock(ch.Children[0], Dock.Right);
            ch.Children.Add(new TextBlock { Text = "Dernières mesures", FontSize = 16, FontWeight = FontWeights.Bold });
            cs.Children.Add(ch);

            if (client.Measurements.Count > 0)
            {
                foreach (var m in client.Measurements)
                {
                    var r = new DockPanel { Margin = new Thickness(16, 6, 16, 6) };
                    var vb = new Border
                    {
                        CornerRadius = new CornerRadius(8),
                        Background = m.IsHighlighted ? (Brush)FindResource("BrushRoseSoft") : (Brush)FindResource("BrushChalk"),
                        BorderBrush = m.IsHighlighted ? (Brush)FindResource("BrushRose") : (Brush)FindResource("BrushMist"),
                        BorderThickness = new Thickness(1),
                        Padding = new Thickness(10, 4, 10, 4)
                    };
                    vb.Child = new TextBlock
                    {
                        Text = $"{m.FormattedValue} {m.Unit}",
                        FontSize = 13,
                        FontWeight = FontWeights.Bold,
                        Foreground = m.IsHighlighted ? (Brush)FindResource("BrushRoseDark") : (Brush)FindResource("BrushInk")
                    };
                    DockPanel.SetDock(vb, Dock.Right);
                    r.Children.Add(vb);
                    r.Children.Add(new TextBlock
                    {
                        Text = m.Name,
                        FontSize = 13.5,
                        FontWeight = m.IsHighlighted ? FontWeights.Bold : FontWeights.Normal,
                        Foreground = (Brush)FindResource("BrushInk"),
                        VerticalAlignment = VerticalAlignment.Center
                    });
                    cs.Children.Add(r);
                }
            }
            else
            {
                var emptyNotice = new TextBlock
                {
                    Text = "Aucune mesure enregistrée pour ce client.",
                    FontSize = 13,
                    Foreground = (Brush)FindResource("BrushMuted"),
                    Margin = new Thickness(16, 14, 16, 14),
                    TextAlignment = TextAlignment.Center
                };
                cs.Children.Add(emptyNotice);
            }

            cs.Children.Add(CreateTape(true));
            card.Child = cs;
            stack.Children.Add(card);

            var fullBtn = new Button
            {
                Content = "Modifier ou ajouter des mesures",
                Height = 50,
                Background = (Brush)FindResource("BrushIndigo"),
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 14.5,
                Cursor = Cursors.Hand
            };
            fullBtn.Click += (s, e) => NavigateTo("Measurements");
            stack.Children.Add(fullBtn);
        }
        else if (activeTab == "Commandes")
        {
            var clientOrders = _orderService.GetAllOrdersAsync().Result
                .Where(o => o.ClientId == client.Id || o.ClientName.Equals(client.FullName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var activeOrdersCount = clientOrders.Count(o => o.Status != OrderStatus.Livree);

            var statGrid = new UniformGrid { Columns = 2, Margin = new Thickness(0, 14, 0, 12) };
            var b1 = new Border { CornerRadius = new CornerRadius(16), Background = (Brush)FindResource("BrushIndigo"), Padding = new Thickness(14, 10, 14, 10), Margin = new Thickness(0, 0, 5, 0) };
            var b1s = new StackPanel();
            b1s.Children.Add(new TextBlock { Text = activeOrdersCount.ToString(), FontSize = 22, FontWeight = FontWeights.Bold, Foreground = Brushes.White });
            b1s.Children.Add(new TextBlock { Text = "en cours", FontSize = 12, Foreground = (Brush)new BrushConverter().ConvertFrom("#C9CCEA")! });
            b1.Child = b1s;
            statGrid.Children.Add(b1);

            var b2 = new Border { CornerRadius = new CornerRadius(16), Background = Brushes.White, BorderBrush = (Brush)FindResource("BrushLine"), BorderThickness = new Thickness(1), Padding = new Thickness(14, 10, 14, 10), Margin = new Thickness(5, 0, 0, 0) };
            var b2s = new StackPanel();
            b2s.Children.Add(new TextBlock { Text = clientOrders.Count.ToString(), FontSize = 22, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushInk") });
            b2s.Children.Add(new TextBlock { Text = "au total", FontSize = 12, Foreground = (Brush)FindResource("BrushMuted") });
            b2.Child = b2s;
            statGrid.Children.Add(b2);
            stack.Children.Add(statGrid);

            if (clientOrders.Count > 0)
            {
                foreach (var ord in clientOrders)
                {
                    stack.Children.Add(BuildClientOrderCard(ord));
                }
            }
            else
            {
                var emptyCard = new Border
                {
                    CornerRadius = new CornerRadius(16),
                    Background = Brushes.White,
                    BorderBrush = (Brush)FindResource("BrushLine"),
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(16, 20, 16, 20),
                    Margin = new Thickness(0, 0, 0, 8)
                };
                emptyCard.Child = new TextBlock
                {
                    Text = "Aucune commande en cours pour ce client.",
                    FontSize = 13,
                    Foreground = (Brush)FindResource("BrushMuted"),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                stack.Children.Add(emptyCard);
            }

            var newOrderBtn = new Button
            {
                Content = "+ Nouvelle commande",
                Height = 50,
                Background = (Brush)FindResource("BrushRose"),
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 12, 0, 0),
                Cursor = Cursors.Hand
            };
            newOrderBtn.Click += (s, e) =>
            {
                _selectedClient = client;
                NavigateTo("NewOrder");
            };
            stack.Children.Add(newOrderBtn);
        }
        else // Notes
        {
            // Interactive Note Registration Card
            var addCard = new Border
            {
                CornerRadius = new CornerRadius(18),
                Background = Brushes.White,
                BorderBrush = (Brush)FindResource("BrushLine"),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(16, 14, 16, 14),
                Margin = new Thickness(0, 14, 0, 16),
                SnapsToDevicePixels = true
            };
            var addStack = new StackPanel();
            addStack.Children.Add(new TextBlock
            {
                Text = $"Ajouter une note pour {client.FullName}",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("BrushInk"),
                Margin = new Thickness(0, 0, 0, 8)
            });

            var noteTextBox = new TextBox
            {
                MinHeight = 64,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                FontSize = 13.5,
                Foreground = (Brush)FindResource("BrushInk"),
                Background = (Brush)FindResource("BrushChalk"),
                BorderBrush = (Brush)FindResource("BrushMist"),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10, 8, 10, 8),
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            addStack.Children.Add(noteTextBox);

            var optionsDock = new DockPanel { Margin = new Thickness(0, 10, 0, 0) };

            var importantCheck = new CheckBox
            {
                Content = "⭐ Note importante (prioritaire)",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("BrushInk"),
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = Cursors.Hand
            };
            optionsDock.Children.Add(importantCheck);

            var saveNoteBtn = new Button
            {
                Content = "Enregistrer la note",
                Height = 38,
                Padding = new Thickness(16, 0, 16, 0),
                Background = (Brush)FindResource("BrushIndigo"),
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 13,
                Cursor = Cursors.Hand,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            DockPanel.SetDock(saveNoteBtn, Dock.Right);
            optionsDock.Children.Add(saveNoteBtn);
            addStack.Children.Add(optionsDock);

            saveNoteBtn.Click += (s, e) =>
            {
                var content = noteTextBox.Text?.Trim();
                if (string.IsNullOrWhiteSpace(content))
                {
                    MessageBox.Show("Veuillez saisir le contenu de la note.", "Sweet Tailor");
                    return;
                }
                bool isImp = importantCheck.IsChecked == true;
                _clientService.AddNoteAsync(client.Id, content, isImp);
                MessageBox.Show("Note enregistrée avec succès !", "Sweet Tailor");
                NavigateTo("ClientNotes");
            };

            addCard.Child = addStack;
            stack.Children.Add(addCard);

            // History header
            var histTitle = new TextBlock
            {
                Text = $"Historique des notes ({client.Notes.Count})",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("BrushInk"),
                Margin = new Thickness(0, 0, 0, 8)
            };
            stack.Children.Add(histTitle);

            if (client.Notes.Count == 0)
            {
                var emptyNoteCard = new Border
                {
                    CornerRadius = new CornerRadius(14),
                    Background = Brushes.White,
                    BorderBrush = (Brush)FindResource("BrushLine"),
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(16),
                    Margin = new Thickness(0, 0, 0, 8)
                };
                emptyNoteCard.Child = new TextBlock
                {
                    Text = "Aucune note pour le moment. Saisissez vos remarques sur les préférences vestimentaires ou retouches ci-dessus.",
                    FontSize = 12.5,
                    Foreground = (Brush)FindResource("BrushMuted"),
                    TextWrapping = TextWrapping.Wrap
                };
                stack.Children.Add(emptyNoteCard);
            }
            else
            {
                foreach (var note in client.Notes)
                {
                    stack.Children.Add(BuildNoteCard(note.DateFormatted, note.Content, note.IsImportant));
                }
            }
        }

        scroll.Content = stack;
        return scroll;
    }

    private FrameworkElement BuildTabButton(string text, bool isSelected, Action onClick)
    {
        var b = new Border
        {
            CornerRadius = new CornerRadius(10),
            Background = isSelected ? Brushes.White : Brushes.Transparent,
            Padding = new Thickness(0, 8, 0, 8),
            Cursor = Cursors.Hand
        };
        b.Child = new TextBlock
        {
            Text = text,
            FontSize = 13,
            FontWeight = isSelected ? FontWeights.Bold : FontWeights.SemiBold,
            Foreground = isSelected ? (Brush)FindResource("BrushIndigo") : (Brush)FindResource("BrushMuted"),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        if (!isSelected)
        {
            b.MouseEnter += (s, e) => b.Background = (Brush)new BrushConverter().ConvertFrom("#ECEEF8")!;
            b.MouseLeave += (s, e) => b.Background = Brushes.Transparent;
        }
        b.MouseLeftButtonDown += (s, e) => onClick();
        return b;
    }

    private FrameworkElement BuildClientOrderCard(Order order)
    {
        var b = new Border
        {
            CornerRadius = new CornerRadius(18),
            Background = Brushes.White,
            BorderBrush = (Brush)FindResource("BrushLine"),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(12, 10, 12, 10),
            Margin = new Thickness(0, 0, 0, 8),
            Cursor = Cursors.Hand,
            SnapsToDevicePixels = true
        };
        var dock = new DockPanel();

        var iconBox = new Border
        {
            Width = 48,
            Height = 48,
            CornerRadius = new CornerRadius(12),
            Background = (Brush)FindResource("BrushChalk"),
            Margin = new Thickness(0, 0, 12, 0),
            ClipToBounds = true
        };
        iconBox.Child = CreateGarmentImage(order.ImagePath, 48, 48, new CornerRadius(12), order.SvgAsset);
        DockPanel.SetDock(iconBox, Dock.Left);
        dock.Children.Add(iconBox);

        var badge = CreateStatusBadge(order.Status);
        DockPanel.SetDock(badge, Dock.Right);
        dock.Children.Add(badge);

        var mid = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        mid.Children.Add(new TextBlock { Text = order.ModelName, FontSize = 14, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushInk") });
        var subText = !string.IsNullOrWhiteSpace(order.DueDateLabel) ? order.DueDateLabel : $"{order.FormattedPrice} · {order.Fabric}";
        mid.Children.Add(new TextBlock { Text = subText, FontSize = 11.5, Foreground = (Brush)FindResource("BrushMuted"), Margin = new Thickness(0, 2, 0, 0) });
        dock.Children.Add(mid);

        b.Child = dock;
        b.MouseEnter += (s, e) => b.Background = (Brush)new BrushConverter().ConvertFrom("#F6F7FD")!;
        b.MouseLeave += (s, e) => b.Background = Brushes.White;
        b.MouseLeftButtonDown += (s, e) =>
        {
            _selectedOrder = order;
            NavigateTo("OrderDetail");
        };
        return b;
    }

    private FrameworkElement BuildClientOrderCard(string garmentType, string title, string sub, OrderStatus status, string grad)
    {
        var b = new Border
        {
            CornerRadius = new CornerRadius(18),
            Background = Brushes.White,
            BorderBrush = (Brush)FindResource("BrushLine"),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(12, 10, 12, 10),
            Margin = new Thickness(0, 0, 0, 8),
            Cursor = Cursors.Hand,
            SnapsToDevicePixels = true
        };
        var dock = new DockPanel();

        var iconBox = new Border { Width = 44, Height = 44, CornerRadius = new CornerRadius(12), Background = (Brush)FindResource(grad), Margin = new Thickness(0, 0, 12, 0) };
        iconBox.Child = CreateGarmentVisual(garmentType, size: 28);
        DockPanel.SetDock(iconBox, Dock.Left);
        dock.Children.Add(iconBox);

        var badge = CreateStatusBadge(status);
        DockPanel.SetDock(badge, Dock.Right);
        dock.Children.Add(badge);

        var mid = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        mid.Children.Add(new TextBlock { Text = title, FontSize = 14, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushInk") });
        mid.Children.Add(new TextBlock { Text = sub, FontSize = 11.5, Foreground = (Brush)FindResource("BrushMuted"), Margin = new Thickness(0, 2, 0, 0) });
        dock.Children.Add(mid);

        b.Child = dock;
        b.MouseEnter += (s, e) => b.Background = (Brush)new BrushConverter().ConvertFrom("#F6F7FD")!;
        b.MouseLeave += (s, e) => b.Background = Brushes.White;
        b.MouseLeftButtonDown += (s, e) => NavigateTo("OrderDetail");
        return b;
    }

    private FrameworkElement BuildNoteCard(string date, string text, bool isImportant)
    {
        var b = new Border
        {
            CornerRadius = new CornerRadius(18),
            Background = Brushes.White,
            BorderBrush = (Brush)FindResource("BrushLine"),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(16, 12, 16, 12),
            Margin = new Thickness(0, 0, 0, 8),
            SnapsToDevicePixels = true
        };
        var s = new StackPanel();
        var h = new DockPanel();
        if (isImportant)
        {
            var imp = new Border { CornerRadius = new CornerRadius(6), Background = (Brush)FindResource("BrushTapeSoft"), Padding = new Thickness(6, 2, 6, 2) };
            imp.Child = new TextBlock { Text = "Important", FontSize = 10.5, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushTapeDark") };
            DockPanel.SetDock(imp, Dock.Right);
            h.Children.Add(imp);
        }
        var dt = new StackPanel { Orientation = Orientation.Horizontal };
        dt.Children.Add(CreateIcon("clock", (Brush)FindResource("BrushMuted"), size: 14));
        dt.Children.Add(new TextBlock { Text = " " + date, FontSize = 12, Foreground = (Brush)FindResource("BrushMuted"), VerticalAlignment = VerticalAlignment.Center });
        h.Children.Add(dt);
        s.Children.Add(h);

        s.Children.Add(new TextBlock { Text = text, FontSize = 13.5, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 0), Foreground = (Brush)FindResource("BrushInk") });
        b.Child = s;
        return b;
    }

    // =========================================================================
    // 13. MESURES DÉTAILLÉES (Page 13)
    // =========================================================================
    private FrameworkElement BuildMeasurementsScreen()
    {
        var client = _selectedClient ?? _clientService.GetClientsAsync().Result.FirstOrDefault() ?? new Client { FullName = "Nadège Mbarga" };
        var firstName = client.FullName.Split(' ').FirstOrDefault() ?? client.FullName;

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var top = new DockPanel { Margin = new Thickness(20, 10, 20, 10) };
        top.Children.Add(BuildIconBtn("back", () => NavigateTo("ClientDetail")));
        var saveIcon = BuildIconBtn("check", () =>
        {
            _clientService.UpdateMeasurementsAsync(client.Id, client.Measurements);
            MessageBox.Show($"Mesures de {client.FullName} enregistrées !", "Sweet Tailor");
            NavigateTo("ClientDetail");
        });
        DockPanel.SetDock(saveIcon, Dock.Right);
        top.Children.Add(saveIcon);
        top.Children.Add(new TextBlock { Text = $"Mesures · {firstName}", FontSize = 15, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center });
        Grid.SetRow(top, 0); grid.Children.Add(top);

        var tape = CreateTape(false);
        Grid.SetRow(tape, 1); grid.Children.Add(tape);

        var rowsStack = new StackPanel();
        var chipsHost = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(20, 12, 20, 12) };
        Grid.SetRow(chipsHost, 2); grid.Children.Add(chipsHost);

        var categories = new[] { "Haut du corps", "Bas", "Longueurs" };

        void RenderCategory(string selected)
        {
            chipsHost.Children.Clear();
            foreach (var cat in categories)
            {
                chipsHost.Children.Add(BuildChip(cat, cat == selected, () => RenderCategory(cat)));
            }

            rowsStack.Children.Clear();
            var targetCat = selected switch
            {
                "Haut du corps" => MeasurementCategory.HautDuCorps,
                "Bas" => MeasurementCategory.Bas,
                "Longueurs" => MeasurementCategory.Longueurs,
                _ => MeasurementCategory.HautDuCorps
            };

            var items = client.Measurements.Where(m => m.Category == targetCat).ToList();
            if (items.Count == 0)
            {
                items = selected switch
                {
                    "Haut du corps" => new List<MeasurementItem>
                    {
                        new() { Name = "Tour de cou", Value = 36, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                        new() { Name = "Carrure dos", Value = 40, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                        new() { Name = "Tour de poitrine", Value = 96, Unit = "cm", Category = MeasurementCategory.HautDuCorps },
                        new() { Name = "Tour de taille", Value = 78, Unit = "cm", Category = MeasurementCategory.HautDuCorps, IsHighlighted = true },
                        new() { Name = "Longueur de manche", Value = 60, Unit = "cm", Category = MeasurementCategory.HautDuCorps }
                    },
                    "Bas" => new List<MeasurementItem>
                    {
                        new() { Name = "Tour de hanches", Value = 100, Unit = "cm", Category = MeasurementCategory.Bas },
                        new() { Name = "Tour de cuisse", Value = 58, Unit = "cm", Category = MeasurementCategory.Bas },
                        new() { Name = "Entrejambe", Value = 78, Unit = "cm", Category = MeasurementCategory.Bas }
                    },
                    _ => new List<MeasurementItem>
                    {
                        new() { Name = "Longueur vêtement", Value = 110, Unit = "cm", Category = MeasurementCategory.Longueurs },
                        new() { Name = "Hauteur totale", Value = 170, Unit = "cm", Category = MeasurementCategory.Longueurs }
                    }
                };
                foreach (var it in items)
                {
                    if (!client.Measurements.Any(m => m.Name == it.Name)) client.Measurements.Add(it);
                }
            }

            foreach (var item in items)
            {
                var row = CreateMeasureInputRow(item.Name, item.FormattedValue, item.IsHighlighted, newVal =>
                {
                    if (double.TryParse(newVal.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsed))
                    {
                        item.Value = parsed;
                        item.LastUpdated = DateTime.Now;
                    }
                });
                rowsStack.Children.Add(row);
            }
        }

        RenderCategory("Haut du corps");

        var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var card = new Border
        {
            Margin = new Thickness(20, 0, 20, 0),
            CornerRadius = new CornerRadius(20),
            Background = Brushes.White,
            BorderBrush = (Brush)FindResource("BrushLine"),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(16, 8, 16, 8),
            SnapsToDevicePixels = true
        };
        card.Child = rowsStack;
        scroll.Content = card;
        Grid.SetRow(scroll, 3); grid.Children.Add(scroll);

        // Buttons Annuler / Enregistrer
        var bot = new UniformGrid { Columns = 2, Margin = new Thickness(20, 14, 20, 20) };
        var cancelBtn = new Button { Content = "Annuler", Height = 48, Background = Brushes.White, BorderBrush = (Brush)FindResource("BrushMist"), BorderThickness = new Thickness(1.5), FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 6, 0), Cursor = Cursors.Hand };
        cancelBtn.Click += (s, e) => NavigateTo("ClientDetail");
        bot.Children.Add(cancelBtn);

        var saveBtn = new Button { Content = "Enregistrer", Height = 48, Background = (Brush)FindResource("BrushIndigo"), Foreground = Brushes.White, FontWeight = FontWeights.Bold, Margin = new Thickness(6, 0, 0, 0), Cursor = Cursors.Hand };
        saveBtn.Click += (s, e) =>
        {
            _clientService.UpdateMeasurementsAsync(client.Id, client.Measurements);
            MessageBox.Show($"Mesures de {client.FullName} enregistrées avec succès !", "Sweet Tailor");
            NavigateTo("ClientDetail");
        };
        bot.Children.Add(saveBtn);
        Grid.SetRow(bot, 4); grid.Children.Add(bot);

        return grid;
    }

    private FrameworkElement CreateMeasureInputRow(string name, string value, bool isHighlighted, Action<string>? onValueChanged = null)
    {
        var row = new Grid { Margin = new Thickness(0, 8, 0, 8), MinHeight = 40 };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        row.Children.Add(new TextBlock
        {
            Text = name,
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = (Brush)FindResource("BrushInk"),
            VerticalAlignment = VerticalAlignment.Center
        });

        var box = new Border
        {
            MinWidth = 78,
            Height = 36,
            CornerRadius = new CornerRadius(10),
            Background = isHighlighted ? (Brush)FindResource("BrushRoseSoft") : (Brush)FindResource("BrushChalk"),
            BorderBrush = isHighlighted ? (Brush)FindResource("BrushRose") : (Brush)FindResource("BrushMist"),
            BorderThickness = new Thickness(isHighlighted ? 1.5 : 1),
            Padding = new Thickness(8, 0, 10, 0),
            Cursor = Cursors.IBeam,
            SnapsToDevicePixels = true
        };

        var inner = new Grid();
        inner.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        inner.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var measureInput = new TextBox
        {
            Text = value,
            FontSize = 15,
            FontWeight = FontWeights.Bold,
            Foreground = isHighlighted ? (Brush)FindResource("BrushRoseDark") : (Brush)FindResource("BrushIndigo"),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            BorderBrush = Brushes.Transparent,
            TextAlignment = TextAlignment.Right,
            MinWidth = 44,
            MaxLength = 5,
            CaretBrush = (Brush)FindResource("BrushIndigo"),
            Padding = new Thickness(0),
            VerticalContentAlignment = VerticalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsReadOnly = false,
            IsEnabled = true,
            Focusable = true,
            IsTabStop = true,
            AcceptsReturn = false
        };
        InputMethod.SetIsInputMethodEnabled(measureInput, true);

        measureInput.TextChanged += (s, e) => onValueChanged?.Invoke(measureInput.Text);

        measureInput.PreviewTextInput += (s, e) =>
        {
            e.Handled = e.Text.Any(ch => !char.IsDigit(ch) && ch != '.' && ch != ',');
        };
        DataObject.AddPastingHandler(measureInput, (s, e) =>
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                var paste = e.DataObject.GetData(DataFormats.Text) as string ?? string.Empty;
                if (paste.Any(ch => !char.IsDigit(ch) && ch != '.' && ch != ','))
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        });
        measureInput.GotFocus += (s, e) =>
        {
            box.BorderBrush = (Brush)FindResource("BrushIndigo");
            box.BorderThickness = new Thickness(1.8);
            measureInput.Dispatcher.BeginInvoke(new Action(() => measureInput.SelectAll()), DispatcherPriority.Input);
        };
        measureInput.LostFocus += (s, e) =>
        {
            box.BorderBrush = isHighlighted ? (Brush)FindResource("BrushRose") : (Brush)FindResource("BrushMist");
            box.BorderThickness = new Thickness(isHighlighted ? 1.5 : 1);
        };

        Grid.SetColumn(measureInput, 0);
        inner.Children.Add(measureInput);
        var unit = new TextBlock
        {
            Text = "cm",
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            Foreground = (Brush)FindResource("BrushMuted"),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(4, 0, 0, 0)
        };
        Grid.SetColumn(unit, 1);
        inner.Children.Add(unit);
        box.Child = inner;

        box.PreviewMouseLeftButtonDown += (s, e) =>
        {
            if (!measureInput.IsKeyboardFocusWithin)
            {
                measureInput.Focus();
                e.Handled = true;
            }
        };

        Grid.SetColumn(box, 1);
        row.Children.Add(box);
        return row;
    }

    // =========================================================================
    // 14. NOUVELLE COMMANDE (Page 14)
    // =========================================================================
    private FrameworkElement BuildNewOrderScreen()
    {
        var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var stack = new StackPanel { Margin = new Thickness(20, 10, 20, 30) };

        var top = new DockPanel { Margin = new Thickness(0, 0, 0, 14) };
        top.Children.Add(BuildIconBtn("close", () => NavigateTo("Orders")));
        top.Children.Add(new TextBlock { Text = "Nouvelle commande", FontSize = 15, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center });
        stack.Children.Add(top);

        // Client selection
        var clients = _clientService.GetClientsAsync().Result;
        var chosenClient = _selectedClient ?? clients.FirstOrDefault() ?? new Client { FullName = "Nadège Mbarga", PhoneNumber = "+237 6 77 00 00 00" };

        var clientCard = new Border
        {
            CornerRadius = new CornerRadius(14),
            Background = Brushes.White,
            BorderBrush = (Brush)FindResource("BrushLine"),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(14, 8, 14, 8),
            MinHeight = 58,
            SnapsToDevicePixels = true
        };
        var clientStack = new StackPanel();
        clientStack.Children.Add(new TextBlock { Text = "Client", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushMuted") });
        var clientDock = new DockPanel { Margin = new Thickness(0, 2, 0, 0) };
        var clientChev = CreateIcon("chevron", (Brush)FindResource("BrushMuted"), size: 14);
        clientChev.RenderTransform = new RotateTransform(90, 7, 7);
        DockPanel.SetDock(clientChev, Dock.Right);
        clientDock.Children.Add(clientChev);

        var clientCombo = new ComboBox
        {
            ItemsSource = clients.Select(c => c.FullName).ToList(),
            SelectedItem = chosenClient.FullName,
            FontSize = 14.5,
            FontWeight = FontWeights.SemiBold,
            Foreground = (Brush)FindResource("BrushInk"),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0)
        };
        clientCombo.SelectionChanged += (s, e) =>
        {
            if (clientCombo.SelectedItem is string selectedName)
            {
                var matched = clients.FirstOrDefault(c => c.FullName == selectedName);
                if (matched != null) chosenClient = matched;
            }
        };
        clientDock.Children.Add(clientCombo);
        clientStack.Children.Add(clientDock);
        clientCard.Child = clientStack;
        stack.Children.Add(clientCard);

        // Modèle input (Editable!)
        var modelCard = new Border
        {
            CornerRadius = new CornerRadius(14),
            Background = Brushes.White,
            BorderBrush = (Brush)FindResource("BrushLine"),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(14, 8, 14, 8),
            MinHeight = 58,
            Margin = new Thickness(0, 8, 0, 0),
            SnapsToDevicePixels = true
        };
        var modelStack = new StackPanel();
        modelStack.Children.Add(new TextBlock { Text = "Modèle de la commande", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushMuted") });
        var modelBox = new TextBox
        {
            Text = "Robe de soirée longue",
            FontSize = 15,
            FontWeight = FontWeights.SemiBold,
            Foreground = (Brush)FindResource("BrushInk"),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            CaretBrush = (Brush)FindResource("BrushIndigo"),
            Padding = new Thickness(0, 2, 0, 0)
        };
        modelStack.Children.Add(modelBox);
        modelCard.Child = modelStack;
        stack.Children.Add(modelCard);

        // Row 1: Tissu & Prix
        var row1 = new UniformGrid { Columns = 2, Margin = new Thickness(0, 8, 0, 0) };

        var tissuCard = new Border { CornerRadius = new CornerRadius(14), Background = Brushes.White, BorderBrush = (Brush)FindResource("BrushLine"), BorderThickness = new Thickness(1), Padding = new Thickness(14, 8, 14, 8), MinHeight = 58, Margin = new Thickness(0, 0, 4, 0), SnapsToDevicePixels = true };
        var tissuStack = new StackPanel();
        tissuStack.Children.Add(new TextBlock { Text = "Tissu", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushMuted") });
        var tissuBox = new TextBox { Text = "Satin bleu nuit", FontSize = 14.5, FontWeight = FontWeights.SemiBold, Foreground = (Brush)FindResource("BrushInk"), Background = Brushes.Transparent, BorderThickness = new Thickness(0), CaretBrush = (Brush)FindResource("BrushIndigo"), Padding = new Thickness(0, 2, 0, 0) };
        tissuStack.Children.Add(tissuBox);
        tissuCard.Child = tissuStack;
        row1.Children.Add(tissuCard);

        var prixCard = new Border { CornerRadius = new CornerRadius(14), Background = Brushes.White, BorderBrush = (Brush)FindResource("BrushLine"), BorderThickness = new Thickness(1), Padding = new Thickness(14, 8, 14, 8), MinHeight = 58, Margin = new Thickness(4, 0, 0, 0), SnapsToDevicePixels = true };
        var prixStack = new StackPanel();
        prixStack.Children.Add(new TextBlock { Text = "Prix (FCFA)", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushMuted") });
        var prixBox = new TextBox { Text = "45 000", FontSize = 14.5, FontWeight = FontWeights.SemiBold, Foreground = (Brush)FindResource("BrushInk"), Background = Brushes.Transparent, BorderThickness = new Thickness(0), CaretBrush = (Brush)FindResource("BrushIndigo"), Padding = new Thickness(0, 2, 0, 0) };
        prixStack.Children.Add(prixBox);
        prixCard.Child = prixStack;
        row1.Children.Add(prixCard);

        stack.Children.Add(row1);

        // Row 2: Avance & Échéance
        var row2 = new UniformGrid { Columns = 2, Margin = new Thickness(0, 8, 0, 0) };

        var avCard = new Border { CornerRadius = new CornerRadius(14), Background = Brushes.White, BorderBrush = (Brush)FindResource("BrushLine"), BorderThickness = new Thickness(1), Padding = new Thickness(14, 8, 14, 8), MinHeight = 58, Margin = new Thickness(0, 0, 4, 0), SnapsToDevicePixels = true };
        var avStack = new StackPanel();
        avStack.Children.Add(new TextBlock { Text = "Avance (FCFA)", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushMuted") });
        var avBox = new TextBox { Text = "20 000", FontSize = 14.5, FontWeight = FontWeights.SemiBold, Foreground = (Brush)FindResource("BrushInk"), Background = Brushes.Transparent, BorderThickness = new Thickness(0), CaretBrush = (Brush)FindResource("BrushIndigo"), Padding = new Thickness(0, 2, 0, 0) };
        avStack.Children.Add(avBox);
        avCard.Child = avStack;
        row2.Children.Add(avCard);

        var dateCard = new Border { CornerRadius = new CornerRadius(14), Background = Brushes.White, BorderBrush = (Brush)FindResource("BrushLine"), BorderThickness = new Thickness(1), Padding = new Thickness(14, 8, 14, 8), MinHeight = 58, Margin = new Thickness(4, 0, 0, 0), SnapsToDevicePixels = true };
        var dateStack = new StackPanel();
        dateStack.Children.Add(new TextBlock { Text = "Échéance", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushMuted") });
        var dateBox = new TextBox { Text = "08 oct. 2026", FontSize = 14.5, FontWeight = FontWeights.SemiBold, Foreground = (Brush)FindResource("BrushInk"), Background = Brushes.Transparent, BorderThickness = new Thickness(0), CaretBrush = (Brush)FindResource("BrushIndigo"), Padding = new Thickness(0, 2, 0, 0) };
        dateStack.Children.Add(dateBox);
        dateCard.Child = dateStack;
        row2.Children.Add(dateCard);

        stack.Children.Add(row2);

        // Photo / Fichier Upload Box (INTERACTIVE FILE PICKER)
        string? chosenImagePath = "robe_soiree.jpg";

        var photoBox = new Border
        {
            MinHeight = 130,
            CornerRadius = new CornerRadius(18),
            Background = Brushes.White,
            BorderBrush = (Brush)new BrushConverter().ConvertFrom("#C9CCEA")!,
            BorderThickness = new Thickness(2),
            Margin = new Thickness(0, 14, 0, 20),
            Cursor = Cursors.Hand,
            SnapsToDevicePixels = true
        };

        var photoGrid = new Grid();

        var pStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(10) };
        pStack.Children.Add(CreateIcon("camera", (Brush)FindResource("BrushIndigo"), size: 28));
        pStack.Children.Add(new TextBlock { Text = "Ajouter la photo ou le fichier du modèle", FontSize = 13.5, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushIndigo"), Margin = new Thickness(0, 6, 0, 2) });
        pStack.Children.Add(new TextBlock { Text = "Cliquez pour choisir une image ou un fichier (*.jpg, *.png, *.webp...)", FontSize = 11, Foreground = (Brush)FindResource("BrushMuted"), HorizontalAlignment = HorizontalAlignment.Center });

        var browseBtn = new Border
        {
            Height = 32,
            CornerRadius = new CornerRadius(8),
            Background = (Brush)FindResource("BrushMist"),
            Padding = new Thickness(12, 0, 12, 0),
            Margin = new Thickness(0, 8, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        var bTxt = new TextBlock { Text = "📁 Parcourir les fichiers...", FontSize = 12, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushIndigo"), VerticalAlignment = VerticalAlignment.Center };
        browseBtn.Child = bTxt;
        pStack.Children.Add(browseBtn);
        photoGrid.Children.Add(pStack);

        var previewImage = new Image
        {
            Stretch = Stretch.UniformToFill,
            Visibility = Visibility.Collapsed,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        photoGrid.Children.Add(previewImage);

        var fileBadge = new Border
        {
            CornerRadius = new CornerRadius(10),
            Background = new SolidColorBrush(Color.FromArgb(0xDD, 0x1E, 0x1B, 0x4B)),
            Padding = new Thickness(10, 5, 10, 5),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 0, 8),
            Visibility = Visibility.Collapsed
        };
        var fileBadgeText = new TextBlock { Text = "", FontSize = 11.5, FontWeight = FontWeights.Bold, Foreground = Brushes.White };
        fileBadge.Child = fileBadgeText;
        photoGrid.Children.Add(fileBadge);

        void OpenPicker()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Choisir l'image ou le fichier du modèle",
                Filter = "Fichiers images (*.png;*.jpg;*.jpeg;*.webp)|*.png;*.jpg;*.jpeg;*.webp|Tous les fichiers (*.*)|*.*"
            };
            if (dlg.ShowDialog() == true)
            {
                chosenImagePath = dlg.FileName;
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(dlg.FileName);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    previewImage.Source = bmp;
                    previewImage.Visibility = Visibility.Visible;
                    pStack.Visibility = Visibility.Collapsed;
                    fileBadgeText.Text = "✓ " + System.IO.Path.GetFileName(dlg.FileName);
                    fileBadge.Visibility = Visibility.Visible;
                    photoBox.Height = 170;

                    if (string.IsNullOrWhiteSpace(modelBox.Text) || modelBox.Text == "Robe de soirée longue")
                    {
                        var autoName = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName).Replace("_", " ").Replace("-", " ");
                        if (!string.IsNullOrWhiteSpace(autoName))
                        {
                            modelBox.Text = char.ToUpper(autoName[0]) + autoName.Substring(1);
                        }
                    }
                }
                catch
                {
                    fileBadgeText.Text = "✓ " + System.IO.Path.GetFileName(dlg.FileName);
                    fileBadge.Visibility = Visibility.Visible;
                }
            }
        }

        photoBox.MouseLeftButtonDown += (s, e) => OpenPicker();
        photoBox.Child = photoGrid;
        stack.Children.Add(photoBox);

        // Save Button
        var saveBtn = new Button
        {
            Content = "Enregistrer la commande",
            Height = 52,
            Background = (Brush)FindResource("BrushRose"),
            Foreground = Brushes.White,
            FontWeight = FontWeights.Bold,
            FontSize = 15,
            Cursor = Cursors.Hand
        };
        saveBtn.Click += (s, e) =>
        {
            var cleanPriceStr = new string(prixBox.Text.Where(char.IsDigit).ToArray());
            var cleanAvStr = new string(avBox.Text.Where(char.IsDigit).ToArray());
            decimal.TryParse(cleanPriceStr, out decimal price);
            decimal.TryParse(cleanAvStr, out decimal advance);
            if (price <= 0) price = 45000;
            if (advance <= 0) advance = 20000;

            var newOrder = new Order
            {
                ClientId = chosenClient.Id,
                ClientName = chosenClient.FullName,
                ClientPhone = chosenClient.PhoneNumber,
                ModelName = string.IsNullOrWhiteSpace(modelBox.Text) ? "Modèle sur mesure" : modelBox.Text.Trim(),
                Fabric = string.IsNullOrWhiteSpace(tissuBox.Text) ? "Tissu fourni" : tissuBox.Text.Trim(),
                TotalPrice = price,
                AdvancePayment = advance,
                DueDate = DateTime.Today.AddDays(7),
                DueDateLabel = string.IsNullOrWhiteSpace(dateBox.Text) ? $"Pour le {DateTime.Today.AddDays(7):d MMM yyyy}" : dateBox.Text,
                IsHotDueDate = true,
                Status = OrderStatus.Recue,
                ImagePath = chosenImagePath ?? "robe_soiree.jpg"
            };

            _orderService.CreateOrderAsync(newOrder);
            _selectedOrder = newOrder;
            MessageBox.Show($"Commande #{newOrder.Id} enregistrée avec succès !", "Sweet Tailor");
            NavigateTo("OrderDetail");
        };
        stack.Children.Add(saveBtn);

        scroll.Content = stack;
        return scroll;
    }

    // =========================================================================
    // 15. DÉTAIL D'UNE COMMANDE (Page 15)
    // =========================================================================
    private FrameworkElement BuildOrderDetailScreen()
    {
        var order = _selectedOrder ?? _orderService.GetAllOrdersAsync().Result.FirstOrDefault() ?? new Order
        {
            OrderNumber = "#0142",
            ModelName = "Robe de soirée longue",
            ClientName = "Nadège Mbarga",
            Status = OrderStatus.Essayage,
            DueDateLabel = "Dans 5 jours · 8 oct.",
            TotalPrice = 45000,
            AdvancePayment = 20000,
            ImagePath = "robe_soiree.jpg"
        };

        var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var stack = new StackPanel { Margin = new Thickness(20, 10, 20, 88) };

        var top = new DockPanel { Margin = new Thickness(0, 0, 0, 14) };
        top.Children.Add(BuildIconBtn("back", () => NavigateTo("Orders")));
        var moreBtn = BuildIconBtn("more", () => { });
        DockPanel.SetDock(moreBtn, Dock.Right);
        top.Children.Add(moreBtn);
        top.Children.Add(new TextBlock { Text = $"Commande {order.OrderNumber}", FontSize = 14.5, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center });
        stack.Children.Add(top);

        // Hero Row: Image + Details
        var hero = new DockPanel();
        var imgBox = new Border
        {
            Width = 110,
            Height = 140,
            CornerRadius = new CornerRadius(18),
            Background = (Brush)FindResource("GradIndigo"),
            Margin = new Thickness(0, 0, 14, 0),
            SnapsToDevicePixels = true,
            ClipToBounds = true
        };
        imgBox.Child = CreateGarmentImage(order.ImagePath, 110, 140, new CornerRadius(18), order.SvgAsset);
        hero.Children.Add(imgBox);

        var heroInfo = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        heroInfo.Children.Add(CreateStatusBadge(order.Status));
        heroInfo.Children.Add(new TextBlock { Text = order.ModelName, FontSize = 18, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushInk"), Margin = new Thickness(0, 6, 0, 2), TextWrapping = TextWrapping.Wrap });

        var clientLink = new TextBlock
        {
            Text = order.ClientName,
            FontSize = 13,
            Foreground = (Brush)FindResource("BrushIndigo"),
            FontWeight = FontWeights.SemiBold,
            Cursor = Cursors.Hand
        };
        clientLink.MouseLeftButtonDown += (s, e) =>
        {
            var cl = _clientService.GetClientsAsync().Result.FirstOrDefault(c => c.Id == order.ClientId || c.FullName.Equals(order.ClientName, StringComparison.OrdinalIgnoreCase));
            if (cl != null)
            {
                _selectedClient = cl;
                NavigateTo("ClientDetail");
            }
        };
        heroInfo.Children.Add(clientLink);

        var clockBadge = new Border { CornerRadius = new CornerRadius(10), Background = (Brush)FindResource("BrushRose"), Padding = new Thickness(8, 5, 8, 5), Margin = new Thickness(0, 8, 0, 0), HorizontalAlignment = HorizontalAlignment.Left };
        var clkStack = new StackPanel { Orientation = Orientation.Horizontal };
        clkStack.Children.Add(CreateIcon("clock", Brushes.White, size: 13));
        clkStack.Children.Add(new TextBlock { Text = " " + (string.IsNullOrWhiteSpace(order.DueDateLabel) ? "Échéance proche" : order.DueDateLabel), FontSize = 11.5, FontWeight = FontWeights.Bold, Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center });
        clockBadge.Child = clkStack;
        heroInfo.Children.Add(clockBadge);

        hero.Children.Add(heroInfo);
        stack.Children.Add(hero);

        // Payment Progress Card
        var payCard = new Border { Margin = new Thickness(0, 14, 0, 12), CornerRadius = new CornerRadius(18), Background = Brushes.White, BorderBrush = (Brush)FindResource("BrushLine"), BorderThickness = new Thickness(1), Padding = new Thickness(16, 12, 16, 12) };
        var payStack = new StackPanel();
        var payHead = new DockPanel();
        payHead.Children.Add(new TextBlock { Text = order.FormattedPaymentRatio, FontSize = 13, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Right });
        DockPanel.SetDock(payHead.Children[0], Dock.Right);
        payHead.Children.Add(new TextBlock { Text = "Paiement", FontSize = 13, FontWeight = FontWeights.Bold });
        payStack.Children.Add(payHead);

        var pBar = new Border { Height = 8, CornerRadius = new CornerRadius(4), Background = (Brush)FindResource("BrushMist"), Margin = new Thickness(0, 8, 0, 6) };
        double fillWidth = Math.Max(8, 280 * order.PaymentProgressRatio);
        pBar.Child = new Border { Width = fillWidth, Height = 8, CornerRadius = new CornerRadius(4), Background = (Brush)FindResource("BrushIndigo"), HorizontalAlignment = HorizontalAlignment.Left };
        payStack.Children.Add(pBar);

        payStack.Children.Add(new TextBlock { Text = $"Reste à payer : {order.FormattedRemaining}", FontSize = 12, Foreground = (Brush)FindResource("BrushMuted") });
        payCard.Child = payStack;
        stack.Children.Add(payCard);

        // Avancement & Stepper Card (The prominent progress state display!)
        int currentStepIdx = (int)order.Status;
        int percent = Math.Min(100, (currentStepIdx + 1) * 20);

        var stepCard = new Border { CornerRadius = new CornerRadius(18), Background = Brushes.White, BorderBrush = (Brush)FindResource("BrushLine"), BorderThickness = new Thickness(1), Padding = new Thickness(16, 14, 16, 14) };
        var stepStack = new StackPanel();

        // Avancement Header
        var avHead = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
        var pctBadge = new Border
        {
            CornerRadius = new CornerRadius(8),
            Background = (Brush)FindResource("BrushIndigoSoft"),
            Padding = new Thickness(8, 3, 8, 3)
        };
        pctBadge.Child = new TextBlock
        {
            Text = $"{percent}% complété",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = (Brush)FindResource("BrushIndigo")
        };
        DockPanel.SetDock(pctBadge, Dock.Right);
        avHead.Children.Add(pctBadge);

        avHead.Children.Add(new TextBlock { Text = "État d'avancement", FontSize = 16, FontWeight = FontWeights.Bold });
        stepStack.Children.Add(avHead);

        // Global Progress Bar
        var progressTrack = new Border
        {
            Height = 10,
            CornerRadius = new CornerRadius(5),
            Background = (Brush)FindResource("BrushMist"),
            Margin = new Thickness(0, 0, 0, 14)
        };
        double avWidth = Math.Max(12, 280.0 * percent / 100.0);
        progressTrack.Child = new Border
        {
            Width = avWidth,
            Height = 10,
            CornerRadius = new CornerRadius(5),
            Background = percent == 100 ? (Brush)FindResource("BrushGreen") : (Brush)FindResource("BrushIndigo"),
            HorizontalAlignment = HorizontalAlignment.Left
        };
        stepStack.Children.Add(progressTrack);

        // The 5 Steps
        var stepDefs = new[]
        {
            (Status: OrderStatus.Recue, Title: "1. Commande reçue", Desc: "Mesures et acompte validés"),
            (Status: OrderStatus.Couture, Title: "2. Coupe & couture", Desc: "Patronnage et confection en atelier"),
            (Status: OrderStatus.Essayage, Title: "3. Essayage client", Desc: "Séance d'ajustements et retouches"),
            (Status: OrderStatus.Prete, Title: "4. Prête à retirer", Desc: "Vêtement repassé et prêt à l'atelier"),
            (Status: OrderStatus.Livree, Title: "5. Livrée au client", Desc: "Commande remise et solde réglé")
        };

        for (int i = 0; i < stepDefs.Length; i++)
        {
            var def = stepDefs[i];
            int stepNum = (int)def.Status;
            StepState state;
            string sub;

            if (stepNum < currentStepIdx)
            {
                state = StepState.Done;
                sub = "Validé";
            }
            else if (stepNum == currentStepIdx)
            {
                state = StepState.Now;
                sub = "▶ En cours";
            }
            else
            {
                state = StepState.Todo;
                sub = def.Desc;
            }

            bool isLast = i == stepDefs.Length - 1;
            stepStack.Children.Add(BuildStepItem(def.Title, sub, state, isLast));
        }

        stepCard.Child = stepStack;
        stack.Children.Add(stepCard);

        // CTA: Passer à l'étape suivante
        if (order.Status != OrderStatus.Livree)
        {
            string nextStatusText = order.Status switch
            {
                OrderStatus.Recue => "Coupe & couture",
                OrderStatus.Couture => "Essayage client",
                OrderStatus.Essayage => "Prête à retirer",
                OrderStatus.Prete => "Marquer comme « Livrée »",
                _ => "Étape suivante"
            };

            var nextBtn = new Button
            {
                Content = $"Passer à « {nextStatusText} »",
                Height = 50,
                Background = (Brush)FindResource("BrushIndigo"),
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 14.5,
                Margin = new Thickness(0, 16, 0, 0),
                Cursor = Cursors.Hand
            };
            nextBtn.Click += (s, e) =>
            {
                _orderService.AdvanceOrderStatusAsync(order.Id);
                switch (order.Status)
                {
                    case OrderStatus.Recue: order.Status = OrderStatus.Couture; break;
                    case OrderStatus.Couture: order.Status = OrderStatus.Essayage; break;
                    case OrderStatus.Essayage: order.Status = OrderStatus.Prete; break;
                    case OrderStatus.Prete: order.Status = OrderStatus.Livree; break;
                }
                MessageBox.Show($"Statut de la commande mis à jour : {order.StatusText} !", "Sweet Tailor");
                NavigateTo("OrderDetail");
            };
            stack.Children.Add(nextBtn);
        }
        else
        {
            var doneCard = new Border
            {
                Margin = new Thickness(0, 16, 0, 0),
                CornerRadius = new CornerRadius(16),
                Background = (Brush)FindResource("BrushGreenSoft"),
                BorderBrush = (Brush)FindResource("BrushGreen"),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(16, 12, 16, 12)
            };
            var doneStack = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            doneStack.Children.Add(CreateIcon("check", (Brush)FindResource("BrushGreen"), size: 18, strokeThickness: 2.2));
            doneStack.Children.Add(new TextBlock
            {
                Text = "  Commande 100% finalisée et livrée au client.",
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("BrushGreenDark"),
                VerticalAlignment = VerticalAlignment.Center
            });
            doneCard.Child = doneStack;
            stack.Children.Add(doneCard);
        }

        scroll.Content = stack;
        return scroll;
    }

    private FrameworkElement BuildStepItem(string title, string sub, StepState state, bool isLast)
    {
        var grid = new Grid { Margin = new Thickness(0, 0, 0, isLast ? 0 : 14) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        if (!isLast)
        {
            var line = new Border
            {
                Width = 2,
                Background = state == StepState.Done ? (Brush)FindResource("BrushIndigo") : (Brush)FindResource("BrushMist"),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 24, 0, -14)
            };
            Grid.SetColumn(line, 0);
            grid.Children.Add(line);
        }

        Border dot;
        if (state == StepState.Done)
        {
            dot = new Border
            {
                Width = 22,
                Height = 22,
                CornerRadius = new CornerRadius(11),
                Background = (Brush)FindResource("BrushIndigo"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top
            };
            dot.Child = CreateIcon("check", Brushes.White, size: 12, strokeThickness: 2.2);
        }
        else if (state == StepState.Now)
        {
            dot = new Border
            {
                Width = 22,
                Height = 22,
                CornerRadius = new CornerRadius(11),
                Background = (Brush)FindResource("BrushRoseSoft"),
                BorderBrush = (Brush)FindResource("BrushRose"),
                BorderThickness = new Thickness(2),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top
            };
            dot.Child = new Border
            {
                Width = 8,
                Height = 8,
                CornerRadius = new CornerRadius(4),
                Background = (Brush)FindResource("BrushRose"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }
        else
        {
            dot = new Border
            {
                Width = 22,
                Height = 22,
                CornerRadius = new CornerRadius(11),
                Background = Brushes.White,
                BorderBrush = (Brush)FindResource("BrushMist"),
                BorderThickness = new Thickness(2),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top
            };
        }
        Grid.SetColumn(dot, 0);
        grid.Children.Add(dot);

        var textStack = new StackPanel { Margin = new Thickness(10, 0, 0, 0), VerticalAlignment = VerticalAlignment.Top };
        textStack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = state == StepState.Todo ? (Brush)new BrushConverter().ConvertFrom("#9A9EBD")! : (Brush)FindResource("BrushInk")
        });
        if (!string.IsNullOrEmpty(sub))
        {
            textStack.Children.Add(new TextBlock
            {
                Text = sub,
                FontSize = 12,
                Foreground = state == StepState.Now ? (Brush)FindResource("BrushRose") : (Brush)FindResource("BrushMuted"),
                FontWeight = state == StepState.Now ? FontWeights.SemiBold : FontWeights.Normal,
                Margin = new Thickness(0, 2, 0, 0)
            });
        }
        Grid.SetColumn(textStack, 1);
        grid.Children.Add(textStack);

        return grid;
    }

    // =========================================================================
    // 16. COMMANDES & ÉCHÉANCES (Page 16)
    // =========================================================================
    private FrameworkElement BuildOrdersScreen()
    {
        var root = new Grid();
        var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var stack = new StackPanel { Margin = new Thickness(20, 10, 20, 96) };

        var top = new DockPanel { Margin = new Thickness(0, 0, 0, 12) };
        top.Children.Add(BuildIconBtn("calendar", () => { }));
        DockPanel.SetDock(top.Children[0], Dock.Right);

        var orders = _orderService.GetAllOrdersAsync().Result;
        var activeCount = orders.Count(o => o.Status != OrderStatus.Livree);

        var tText = new StackPanel();
        tText.Children.Add(new TextBlock { Text = $"{activeCount} en cours", FontSize = 12, Foreground = (Brush)FindResource("BrushMuted") });
        tText.Children.Add(new TextBlock { Text = "Commandes", FontSize = 28, FontWeight = FontWeights.Bold });
        top.Children.Add(tText);
        stack.Children.Add(top);

        var chips = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 14) };
        chips.Children.Add(BuildChip("Toutes", true));
        chips.Children.Add(BuildChip("Reçues", false));
        chips.Children.Add(BuildChip("Couture", false));
        chips.Children.Add(BuildChip("Essayage", false));
        stack.Children.Add(chips);

        var refDate = DateTime.Today.AddDays(7);
        var thisWeek = orders.Where(o => o.DueDate <= refDate).ToList();
        var nextWeek = orders.Where(o => o.DueDate > refDate).ToList();

        if (thisWeek.Count > 0)
        {
            stack.Children.Add(new TextBlock { Text = "Cette semaine", FontSize = 13, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushMuted"), Margin = new Thickness(0, 6, 0, 8) });
            foreach (var ord in thisWeek)
            {
                var r = BuildFloatingOrderRow(ord);
                r.Margin = new Thickness(0, 0, 0, 8);
                stack.Children.Add(r);
            }
        }

        if (nextWeek.Count > 0)
        {
            stack.Children.Add(new TextBlock { Text = "Semaine prochaine", FontSize = 13, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushMuted"), Margin = new Thickness(0, 14, 0, 8) });
            foreach (var ord in nextWeek)
            {
                var r = BuildFloatingOrderRow(ord);
                r.Margin = new Thickness(0, 0, 0, 8);
                stack.Children.Add(r);
            }
        }
        else if (thisWeek.Count == 0)
        {
            var emptyNotice = new TextBlock
            {
                Text = "Aucune commande en cours pour le moment.",
                FontSize = 13,
                Foreground = (Brush)FindResource("BrushMuted"),
                Margin = new Thickness(0, 20, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            stack.Children.Add(emptyNotice);
        }

        scroll.Content = stack;
        root.Children.Add(scroll);

        // FAB positioned cleanly ABOVE the bottom navbar (bottom: 92px)
        var fab = new Border
        {
            Width = 56,
            Height = 56,
            CornerRadius = new CornerRadius(18),
            Background = (Brush)FindResource("BrushRose"),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 20, 92),
            Cursor = Cursors.Hand,
            SnapsToDevicePixels = true
        };
        fab.Child = CreateIcon("plus", Brushes.White, size: 24, strokeThickness: 2.6);
        fab.MouseEnter += (s, e) => fab.Background = (Brush)new BrushConverter().ConvertFrom("#D64E6F")!;
        fab.MouseLeave += (s, e) => fab.Background = (Brush)FindResource("BrushRose");
        fab.MouseLeftButtonDown += (s, e) => NavigateTo("NewOrder");
        root.Children.Add(fab);

        return root;
    }

    // =========================================================================
    // 17. RECHERCHE (Page 17)
    // =========================================================================
    private FrameworkElement BuildSearchScreen()
    {
        var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var stack = new StackPanel { Margin = new Thickness(20, 10, 20, 30) };

        var top = new DockPanel();
        top.Children.Add(BuildIconBtn("filter", () => { }));
        DockPanel.SetDock(top.Children[0], Dock.Right);

        var searchBox = new Border
        {
            Height = 48,
            CornerRadius = new CornerRadius(14),
            Background = Brushes.White,
            BorderBrush = (Brush)FindResource("BrushIndigo"),
            BorderThickness = new Thickness(1.8),
            Padding = new Thickness(12, 0, 12, 0),
            Margin = new Thickness(0, 0, 10, 0),
            SnapsToDevicePixels = true
        };
        var sDock = new DockPanel { VerticalAlignment = VerticalAlignment.Center };
        sDock.Children.Add(CreateIcon("search", (Brush)FindResource("BrushMuted"), size: 18));

        var clear = new Border
        {
            Width = 26,
            Height = 26,
            CornerRadius = new CornerRadius(13),
            Background = Brushes.Transparent,
            Cursor = Cursors.Hand
        };
        clear.Child = CreateIcon("close", (Brush)FindResource("BrushMuted"), size: 14);
        clear.MouseEnter += (s, e) => clear.Background = (Brush)new BrushConverter().ConvertFrom("#F0F1FA")!;
        clear.MouseLeave += (s, e) => clear.Background = Brushes.Transparent;
        clear.MouseLeftButtonDown += (s, e) => NavigateTo("SearchEmpty");
        DockPanel.SetDock(clear, Dock.Right);
        sDock.Children.Add(clear);

        var searchInput = new TextBox
        {
            Text = "Mba",
            FontSize = 15,
            FontWeight = FontWeights.Bold,
            Foreground = (Brush)FindResource("BrushInk"),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            CaretBrush = (Brush)FindResource("BrushIndigo"),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0)
        };
        sDock.Children.Add(searchInput);
        searchBox.Child = sDock;
        top.Children.Add(searchBox);
        stack.Children.Add(top);

        var chips = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 12, 0, 14) };
        chips.Children.Add(BuildChip("Tout", true));
        chips.Children.Add(BuildChip("Clients", false));
        chips.Children.Add(BuildChip("Commandes", false));
        chips.Children.Add(BuildChip("Statut ▾", false));
        stack.Children.Add(chips);

        stack.Children.Add(new TextBlock { Text = "Clients · 2 résultats", FontSize = 13, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushMuted"), Margin = new Thickness(0, 4, 0, 8) });
        stack.Children.Add(BuildHighlightedClientRow("NM", "a1", "Nadège ", "Mba", "rga", "2 commandes · dernière le 28 sept."));
        stack.Children.Add(BuildHighlightedClientRow("JM", "a3", "Joël ", "Mba", "ppé", "1 commande · livrée"));

        stack.Children.Add(new TextBlock { Text = "Commandes · 1 résultat", FontSize = 13, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushMuted"), Margin = new Thickness(0, 12, 0, 8) });
        stack.Children.Add(BuildFloatingOrderRow("08", "OCT.", "Robe de soirée", "Nadège Mbarga", OrderStatus.Essayage, true));

        stack.Children.Add(new TextBlock { Text = "Recherches récentes", FontSize = 13, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushMuted"), Margin = new Thickness(0, 14, 0, 8) });
        var recent = new StackPanel { Orientation = Orientation.Horizontal };
        recent.Children.Add(BuildHistoryChip("Kaba"));
        recent.Children.Add(BuildHistoryChip("Costume"));
        recent.Children.Add(BuildHistoryChip("Tchoua"));
        stack.Children.Add(recent);

        scroll.Content = stack;
        return scroll;
    }

    private FrameworkElement BuildHistoryChip(string term)
    {
        var b = new Border
        {
            Height = 34,
            CornerRadius = new CornerRadius(999),
            Background = Brushes.White,
            BorderBrush = (Brush)FindResource("BrushLine"),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(12, 0, 12, 0),
            Margin = new Thickness(0, 0, 8, 0),
            Cursor = Cursors.Hand
        };
        var s = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        s.Children.Add(CreateIcon("clock", (Brush)FindResource("BrushMuted"), size: 13));
        s.Children.Add(new TextBlock { Text = " " + term, FontSize = 12.5, Foreground = (Brush)FindResource("BrushInk"), FontWeight = FontWeights.SemiBold, Margin = new Thickness(4, 0, 0, 0) });
        b.Child = s;
        b.MouseEnter += (s2, e2) => b.Background = (Brush)new BrushConverter().ConvertFrom("#F0F1FA")!;
        b.MouseLeave += (s2, e2) => b.Background = Brushes.White;
        return b;
    }

    private FrameworkElement BuildHighlightedClientRow(string init, string col, string prefix, string highlight, string suffix, string desc)
    {
        var b = new Border { CornerRadius = new CornerRadius(16), Background = Brushes.White, BorderBrush = (Brush)FindResource("BrushLine"), BorderThickness = new Thickness(1), Padding = new Thickness(12), Margin = new Thickness(0, 0, 0, 8), Cursor = Cursors.Hand };
        var dock = new DockPanel();
        dock.Children.Add(CreateAvatar(init, col, 44));

        var arrow = CreateIcon("chevron", (Brush)FindResource("BrushMuted"), size: 16);
        arrow.VerticalAlignment = VerticalAlignment.Center;
        DockPanel.SetDock(arrow, Dock.Right);
        dock.Children.Add(arrow);

        var mid = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0) };
        var nameSpan = new TextBlock { FontSize = 14, FontWeight = FontWeights.Bold };
        nameSpan.Inlines.Add(new System.Windows.Documents.Run(prefix));
        nameSpan.Inlines.Add(new System.Windows.Documents.Run(highlight) { Background = (Brush)FindResource("BrushTape"), Foreground = (Brush)FindResource("BrushInk") });
        nameSpan.Inlines.Add(new System.Windows.Documents.Run(suffix));
        mid.Children.Add(nameSpan);
        mid.Children.Add(new TextBlock { Text = desc, FontSize = 11.5, Foreground = (Brush)FindResource("BrushMuted") });
        dock.Children.Add(mid);

        b.Child = dock;
        b.MouseEnter += (s, e) => b.Background = (Brush)new BrushConverter().ConvertFrom("#F6F7FD")!;
        b.MouseLeave += (s, e) => b.Background = Brushes.White;
        b.MouseLeftButtonDown += (s, e) =>
        {
            var matched = _clientService.GetClientsAsync().Result.FirstOrDefault(c => c.FullName.Contains(highlight, StringComparison.OrdinalIgnoreCase) || c.AvatarInitials == init);
            if (matched != null) _selectedClient = matched;
            NavigateTo("ClientDetail");
        };
        return b;
    }

    // =========================================================================
    // 18. RECHERCHE : AUCUN RÉSULTAT (Page 18)
    // =========================================================================
    private FrameworkElement BuildSearchEmptyScreen()
    {
        var stack = new StackPanel { Margin = new Thickness(20, 10, 20, 30) };

        var top = new DockPanel();
        top.Children.Add(BuildIconBtn("filter", () => { }));
        DockPanel.SetDock(top.Children[0], Dock.Right);

        var searchBox = new Border
        {
            Height = 48,
            CornerRadius = new CornerRadius(14),
            Background = Brushes.White,
            BorderBrush = (Brush)FindResource("BrushIndigo"),
            BorderThickness = new Thickness(1.8),
            Padding = new Thickness(12, 0, 12, 0),
            Margin = new Thickness(0, 0, 10, 0)
        };
        var sDock = new DockPanel { VerticalAlignment = VerticalAlignment.Center };
        sDock.Children.Add(CreateIcon("search", (Brush)FindResource("BrushMuted"), size: 18));
        var clear = CreateIcon("close", (Brush)FindResource("BrushMuted"), size: 14);
        DockPanel.SetDock(clear, Dock.Right);
        sDock.Children.Add(clear);
        sDock.Children.Add(new TextBlock { Text = "Zzz", FontSize = 15, FontWeight = FontWeights.Bold, Margin = new Thickness(8, 0, 0, 0) });
        searchBox.Child = sDock;
        top.Children.Add(searchBox);
        stack.Children.Add(top);

        var chips = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 12, 0, 30) };
        chips.Children.Add(BuildChip("Tout", true));
        chips.Children.Add(BuildChip("Clients", false));
        chips.Children.Add(BuildChip("Commandes", false));
        stack.Children.Add(chips);

        // Glow Center
        var glow = new Border { Width = 140, Height = 140, CornerRadius = new CornerRadius(70), Background = (Brush)FindResource("BrushTapeSoft"), HorizontalAlignment = HorizontalAlignment.Center };
        var centerBox = new Border { Width = 92, Height = 92, CornerRadius = new CornerRadius(24), Background = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        centerBox.Child = CreateIcon("search", (Brush)FindResource("BrushTapeDark"), size: 36);
        glow.Child = centerBox;
        stack.Children.Add(glow);

        stack.Children.Add(new TextBlock { Text = "Aucun résultat pour « Zzz »", FontSize = 20, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushInk"), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 24, 0, 0) });
        stack.Children.Add(new TextBlock { Text = "Vérifiez l'orthographe ou essayez un autre nom, un numéro de téléphone ou un type de vêtement.", FontSize = 13.5, Foreground = (Brush)FindResource("BrushMuted"), TextAlignment = TextAlignment.Center, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(20, 8, 20, 24) });

        var addBtn = new Button { Content = "+ Ajouter ce client", Height = 50, Background = Brushes.White, BorderBrush = (Brush)FindResource("BrushMist"), BorderThickness = new Thickness(1.5), FontWeight = FontWeights.Bold, FontSize = 14.5, Foreground = (Brush)FindResource("BrushIndigo"), Cursor = Cursors.Hand };
        addBtn.Click += (s, e) => NavigateTo("ClientForm");
        stack.Children.Add(addBtn);

        return stack;
    }

    // =========================================================================
    // 19. CATALOGUE DE MODÈLES (Page 19)
    // =========================================================================
    private FrameworkElement BuildCatalogScreen()
    {
        var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var stack = new StackPanel { Margin = new Thickness(20, 10, 20, 96) };

        var top = new DockPanel { Margin = new Thickness(0, 0, 0, 12) };
        top.Children.Add(BuildIconBtn("heart", () => { }));
        DockPanel.SetDock(top.Children[0], Dock.Right);
        var tText = new StackPanel();
        tText.Children.Add(new TextBlock { Text = "248 modèles", FontSize = 12, Foreground = (Brush)FindResource("BrushMuted") });
        tText.Children.Add(new TextBlock { Text = "Catalogue", FontSize = 28, FontWeight = FontWeights.Bold });
        top.Children.Add(tText);
        stack.Children.Add(top);

        var searchBar = new Border { Height = 48, CornerRadius = new CornerRadius(14), Background = Brushes.White, BorderBrush = (Brush)FindResource("BrushLine"), BorderThickness = new Thickness(1), Padding = new Thickness(14, 0, 14, 0), Margin = new Thickness(0, 0, 0, 12) };
        var sDock = new DockPanel { VerticalAlignment = VerticalAlignment.Center };
        sDock.Children.Add(CreateIcon("search", (Brush)FindResource("BrushMuted"), size: 18));
        sDock.Children.Add(new TextBlock { Text = "Rechercher un modèle, un style", FontSize = 13, Foreground = (Brush)FindResource("BrushMuted"), Margin = new Thickness(8, 0, 0, 0) });
        searchBar.Child = sDock;
        stack.Children.Add(searchBar);

        var chips = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 14) };
        chips.Children.Add(BuildChip("Tout", true));
        chips.Children.Add(BuildChip("Robes", false));
        chips.Children.Add(BuildChip("Pagne", false));
        chips.Children.Add(BuildChip("Costumes", false));
        chips.Children.Add(BuildChip("Mariage", false));
        stack.Children.Add(chips);

        // 2 Columns Staggered Grid with genuine fashion studio photography
        var grid = new UniformGrid { Columns = 2 };
        var col1 = new StackPanel { Margin = new Thickness(0, 0, 5, 0) };
        col1.Children.Add(BuildCatalogCard("gown", "Robe sirène en satin", "Mariage", 190, "GradRose", true, "robe_soiree.jpg"));
        col1.Children.Add(BuildCatalogCard("kaba", "Ensemble pagne", "Pagne", 150, "GradTape", true, "jupe_pagne.jpg"));
        col1.Children.Add(BuildCatalogCard("gown", "Robe cocktail", "Soirée", 200, "GradIndigo", false, "robe_cocktail.jpg"));
        grid.Children.Add(col1);

        var col2 = new StackPanel { Margin = new Thickness(5, 0, 0, 0) };
        col2.Children.Add(BuildCatalogCard("suit", "Costume 3 pièces", "Homme", 180, "GradMist", false, "costume_homme.jpg"));
        col2.Children.Add(BuildCatalogCard("skirt", "Jupe plissée en wax", "Pagne", 140, "GradGreen", false, "jupe_pagne.jpg"));
        col2.Children.Add(BuildCatalogCard("suit", "Kaba brodé", "Traditionnel", 190, "GradPink", false, "kaba_africain.jpg"));
        grid.Children.Add(col2);

        stack.Children.Add(grid);
        scroll.Content = stack;
        return scroll;
    }

    private FrameworkElement BuildCatalogCard(string garmentType, string title, string cat, double height, string gradKey, bool isFav, string? imagePath = null)
    {
        var s = new StackPanel { Margin = new Thickness(0, 0, 0, 12), Cursor = Cursors.Hand };
        var b = new Border
        {
            Height = height,
            CornerRadius = new CornerRadius(18),
            Background = (Brush)FindResource(gradKey),
            ClipToBounds = true,
            SnapsToDevicePixels = true
        };
        var canvas = new Canvas();

        if (!string.IsNullOrEmpty(imagePath))
        {
            var img = CreateGarmentImage(imagePath, 165, height, new CornerRadius(18), garmentType);
            canvas.Children.Add(img);
        }
        else
        {
            var garment = CreateGarmentVisual(garmentType, size: height * 0.58);
            Canvas.SetLeft(garment, 20);
            Canvas.SetTop(garment, (height - (height * 0.58)) / 2);
            canvas.Children.Add(garment);
        }

        var heart = new Border
        {
            Width = 32,
            Height = 32,
            CornerRadius = new CornerRadius(16),
            Background = new SolidColorBrush(Color.FromArgb(0xF0, 0xFF, 0xFF, 0xFF)),
            Cursor = Cursors.Hand
        };
        heart.Child = CreateIcon("heart",
            stroke: isFav ? (Brush)FindResource("BrushRose") : (Brush)FindResource("BrushMuted"),
            fill: isFav ? (Brush)FindResource("BrushRose") : Brushes.Transparent,
            size: 16);
        Canvas.SetRight(heart, 8);
        Canvas.SetTop(heart, 8);
        canvas.Children.Add(heart);

        b.Child = canvas;
        s.Children.Add(b);

        var info = new StackPanel { Margin = new Thickness(4, 4, 0, 0) };
        info.Children.Add(new TextBlock { Text = title, FontSize = 12.5, FontWeight = FontWeights.Bold });
        info.Children.Add(new TextBlock { Text = cat, FontSize = 11, Foreground = (Brush)FindResource("BrushMuted") });
        s.Children.Add(info);

        s.MouseLeftButtonDown += (e, v) => NavigateTo("CatalogDetail");
        return s;
    }

    // =========================================================================
    // 20. DÉTAIL D'UN MODÈLE (Page 20)
    // =========================================================================
    private FrameworkElement BuildCatalogDetailScreen()
    {
        var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var stack = new StackPanel { Margin = new Thickness(20, 10, 20, 30) };

        var top = new DockPanel { Margin = new Thickness(0, 0, 0, 14) };
        top.Children.Add(BuildIconBtn("back", () => NavigateTo("Catalog")));
        var shareBtn = BuildIconBtn("share", () => MessageBox.Show("Lien de partage copié !", "Sweet Tailor"));
        DockPanel.SetDock(shareBtn, Dock.Right);
        top.Children.Add(shareBtn);
        stack.Children.Add(top);

        var hero = new Border
        {
            Height = 220,
            CornerRadius = new CornerRadius(24),
            Background = (Brush)FindResource("GradRose"),
            ClipToBounds = true,
            SnapsToDevicePixels = true
        };
        var canvas = new Canvas();
        var heroImg = CreateGarmentImage("robe_soiree.jpg", 350, 220, new CornerRadius(24), "gown");
        canvas.Children.Add(heroImg);

        var tag = new Border { CornerRadius = new CornerRadius(999), Background = Brushes.White, Padding = new Thickness(12, 5, 12, 5) };
        tag.Child = new TextBlock { Text = "Mariage", FontSize = 11.5, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushIndigo") };
        Canvas.SetLeft(tag, 14);
        Canvas.SetBottom(tag, 14);
        canvas.Children.Add(tag);

        hero.Child = canvas;
        stack.Children.Add(hero);

        stack.Children.Add(new TextBlock { Text = "Robe sirène en satin", FontSize = 22, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 14, 0, 2) });
        stack.Children.Add(new TextBlock { Text = "Coupe ajustée, évasée en bas", FontSize = 13.5, Foreground = (Brush)FindResource("BrushMuted") });

        stack.Children.Add(new TextBlock { Text = "Mesures à prendre", FontSize = 13, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushMuted"), Margin = new Thickness(0, 14, 0, 8) });
        var chips = new WrapPanel();
        chips.Children.Add(BuildChip("Poitrine", false));
        chips.Children.Add(BuildChip("Taille", false));
        chips.Children.Add(BuildChip("Hanches", false));
        chips.Children.Add(BuildChip("Longueur robe", false));
        stack.Children.Add(chips);

        stack.Children.Add(new TextBlock { Text = "Modèles similaires", FontSize = 13, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushMuted"), Margin = new Thickness(0, 14, 0, 8) });
        var sims = new UniformGrid { Columns = 3 };
        sims.Children.Add(new Border { Height = 90, CornerRadius = new CornerRadius(14), Margin = new Thickness(0, 0, 4, 0), Child = CreateGarmentImage("robe_cocktail.jpg", 100, 90, new CornerRadius(14), "gown") });
        sims.Children.Add(new Border { Height = 90, CornerRadius = new CornerRadius(14), Margin = new Thickness(2, 0, 2, 0), Child = CreateGarmentImage("costume_homme.jpg", 100, 90, new CornerRadius(14), "suit") });
        sims.Children.Add(new Border { Height = 90, CornerRadius = new CornerRadius(14), Margin = new Thickness(4, 0, 0, 0), Child = CreateGarmentImage("kaba_africain.jpg", 100, 90, new CornerRadius(14), "kaba") });
        stack.Children.Add(sims);

        var actions = new DockPanel { Margin = new Thickness(0, 20, 0, 0) };
        var favBtn = new Border { Width = 52, Height = 52, CornerRadius = new CornerRadius(16), Background = Brushes.White, BorderBrush = (Brush)FindResource("BrushLine"), BorderThickness = new Thickness(1), Margin = new Thickness(0, 0, 10, 0), Cursor = Cursors.Hand };
        favBtn.Child = CreateIcon("heart", (Brush)FindResource("BrushRose"), fill: Brushes.Transparent, size: 20);
        actions.Children.Add(favBtn);

        var orderBtn = new Button { Content = "Créer une commande", Height = 52, Background = (Brush)FindResource("BrushRose"), Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 15, Cursor = Cursors.Hand };
        orderBtn.Click += (s, e) => NavigateTo("NewOrder");
        actions.Children.Add(orderBtn);
        stack.Children.Add(actions);

        scroll.Content = stack;
        return scroll;
    }

    // =========================================================================
    // 21. NOUVEAU CLIENT (Page 21)
    // =========================================================================
    private FrameworkElement BuildClientFormScreen()
    {
        var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var stack = new StackPanel { Margin = new Thickness(20, 10, 20, 30) };

        var top = new DockPanel { Margin = new Thickness(0, 0, 0, 14) };
        top.Children.Add(BuildIconBtn("close", () => NavigateTo("Clients")));
        top.Children.Add(new TextBlock { Text = "Nouveau client", FontSize = 15, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center });
        stack.Children.Add(top);

        // Photo circle placeholder with camera badge
        var photoGrid = new Grid { HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 6, 0, 0) };
        var photoBox = new Border
        {
            Width = 84,
            Height = 84,
            CornerRadius = new CornerRadius(42),
            Background = Brushes.White,
            BorderBrush = (Brush)new BrushConverter().ConvertFrom("#C9CCEA")!,
            BorderThickness = new Thickness(2),
            SnapsToDevicePixels = true
        };
        photoBox.Child = CreateIcon("user", (Brush)FindResource("BrushIndigo"), size: 36);
        photoGrid.Children.Add(photoBox);

        var camBadge = new Border
        {
            Width = 26,
            Height = 26,
            CornerRadius = new CornerRadius(13),
            Background = (Brush)FindResource("BrushRose"),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Cursor = Cursors.Hand
        };
        camBadge.Child = CreateIcon("camera", Brushes.White, size: 14);
        photoGrid.Children.Add(camBadge);
        stack.Children.Add(photoGrid);

        stack.Children.Add(new TextBlock { Text = "Ajouter une photo (facultatif)", FontSize = 11.5, Foreground = (Brush)FindResource("BrushMuted"), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 6, 0, 16) });

        // Nom complet
        var nameCard = new Border { CornerRadius = new CornerRadius(14), Background = Brushes.White, BorderBrush = (Brush)FindResource("BrushLine"), BorderThickness = new Thickness(1), Padding = new Thickness(14, 8, 14, 8), MinHeight = 58, SnapsToDevicePixels = true };
        var nameStack = new StackPanel();
        nameStack.Children.Add(new TextBlock { Text = "Nom complet", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushMuted") });
        var nameInput = new TextBox { Text = "Nadège Mbarga", FontSize = 15, FontWeight = FontWeights.SemiBold, Foreground = (Brush)FindResource("BrushInk"), Background = Brushes.Transparent, BorderThickness = new Thickness(0), CaretBrush = (Brush)FindResource("BrushIndigo"), Padding = new Thickness(0, 2, 0, 0) };
        nameStack.Children.Add(nameInput);
        nameCard.Child = nameStack;
        stack.Children.Add(nameCard);

        // Téléphone
        var telCard = new Border { CornerRadius = new CornerRadius(14), Background = Brushes.White, BorderBrush = (Brush)FindResource("BrushLine"), BorderThickness = new Thickness(1), Padding = new Thickness(14, 8, 14, 8), MinHeight = 58, Margin = new Thickness(0, 8, 0, 0), SnapsToDevicePixels = true };
        var telStack = new StackPanel();
        telStack.Children.Add(new TextBlock { Text = "Téléphone", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushMuted") });
        var telInput = new TextBox { Text = "+237 6 77 00 00 00", FontSize = 15, FontWeight = FontWeights.SemiBold, Foreground = (Brush)FindResource("BrushInk"), Background = Brushes.Transparent, BorderThickness = new Thickness(0), CaretBrush = (Brush)FindResource("BrushIndigo"), Padding = new Thickness(0, 2, 0, 0) };
        telStack.Children.Add(telInput);
        telCard.Child = telStack;
        stack.Children.Add(telCard);

        // Quartier ou adresse
        var adrCard = new Border { CornerRadius = new CornerRadius(14), Background = Brushes.White, BorderBrush = (Brush)FindResource("BrushLine"), BorderThickness = new Thickness(1), Padding = new Thickness(14, 8, 14, 8), MinHeight = 58, Margin = new Thickness(0, 8, 0, 0), SnapsToDevicePixels = true };
        var adrStack = new StackPanel();
        adrStack.Children.Add(new TextBlock { Text = "Quartier ou adresse", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushMuted") });
        var adrInput = new TextBox { Text = "Bonamoussadi, Douala", FontSize = 15, FontWeight = FontWeights.SemiBold, Foreground = (Brush)FindResource("BrushInk"), Background = Brushes.Transparent, BorderThickness = new Thickness(0), CaretBrush = (Brush)FindResource("BrushIndigo"), Padding = new Thickness(0, 2, 0, 0) };
        adrStack.Children.Add(adrInput);
        adrCard.Child = adrStack;
        stack.Children.Add(adrCard);

        // Catégorie
        stack.Children.Add(new TextBlock { Text = "Catégorie", FontSize = 12.5, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushMuted"), Margin = new Thickness(0, 12, 0, 6) });
        var catBox = new Border { CornerRadius = new CornerRadius(14), Background = (Brush)FindResource("BrushMist"), Padding = new Thickness(4) };
        var catGrid = new UniformGrid { Columns = 3 };
        catGrid.Children.Add(BuildTabButton("Femme", true, () => { }));
        catGrid.Children.Add(BuildTabButton("Homme", false, () => { }));
        catGrid.Children.Add(BuildTabButton("Enfant", false, () => { }));
        catBox.Child = catGrid;
        stack.Children.Add(catBox);

        // Note
        var noteCard = new Border { CornerRadius = new CornerRadius(14), Background = Brushes.White, BorderBrush = (Brush)FindResource("BrushLine"), BorderThickness = new Thickness(1), Padding = new Thickness(14, 8, 14, 8), MinHeight = 58, Margin = new Thickness(0, 10, 0, 20), SnapsToDevicePixels = true };
        var noteStack = new StackPanel();
        noteStack.Children.Add(new TextBlock { Text = "Note (facultatif)", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushMuted") });
        var noteInput = new TextBox { Text = "Préfère les coupes ajustées", FontSize = 14, FontWeight = FontWeights.Normal, Foreground = (Brush)FindResource("BrushInk"), Background = Brushes.Transparent, BorderThickness = new Thickness(0), CaretBrush = (Brush)FindResource("BrushIndigo"), Padding = new Thickness(0, 2, 0, 0) };
        noteStack.Children.Add(noteInput);
        noteCard.Child = noteStack;
        stack.Children.Add(noteCard);

        Client CreateFromForm()
        {
            var fullName = string.IsNullOrWhiteSpace(nameInput.Text) ? "Nouveau Client" : nameInput.Text.Trim();
            var newClient = new Client
            {
                FullName = fullName,
                PhoneNumber = telInput.Text.Trim(),
                Address = adrInput.Text.Trim(),
                AvatarInitials = Client.ComputeInitials(fullName),
                AvatarColorClass = "a1"
            };
            if (!string.IsNullOrWhiteSpace(noteInput.Text))
            {
                newClient.Notes.Add(new ClientNote { Content = noteInput.Text.Trim(), Date = DateTime.Now });
            }
            _clientService.AddClientAsync(newClient);
            _selectedClient = newClient;
            return newClient;
        }

        var btn1 = new Button { Content = "Enregistrer et saisir les mesures", Height = 52, Background = (Brush)FindResource("BrushRose"), Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 14.5, Margin = new Thickness(0, 0, 0, 8), Cursor = Cursors.Hand };
        btn1.Click += (s, e) => { CreateFromForm(); NavigateTo("Measurements"); };
        stack.Children.Add(btn1);

        var btn2 = new Button { Content = "Enregistrer seulement", Height = 48, Background = Brushes.White, BorderBrush = (Brush)FindResource("BrushMist"), BorderThickness = new Thickness(1.5), FontWeight = FontWeights.Bold, FontSize = 14, Cursor = Cursors.Hand };
        btn2.Click += (s, e) => { CreateFromForm(); NavigateTo("ClientDetail"); };
        stack.Children.Add(btn2);

        scroll.Content = stack;
        return scroll;
    }

    // =========================================================================
    // 24. CHARTE DES COMPOSANTS (Page 24)
    // =========================================================================
    private FrameworkElement BuildComponentsCharterScreen()
    {
        var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var stack = new StackPanel { Margin = new Thickness(14, 14, 14, 30) };

        stack.Children.Add(new TextBlock { Text = "Charte des états des composants", FontSize = 18, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 16) });

        var headerGrid = new Grid { Margin = new Thickness(0, 0, 0, 12) };
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        string[] cols = { "Normal", "Actif", "Désactivé", "Erreur" };
        for (int i = 0; i < 4; i++)
        {
            var th = new TextBlock { Text = cols[i], FontSize = 10.5, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushMuted"), HorizontalAlignment = HorizontalAlignment.Center };
            Grid.SetColumn(th, i + 1);
            headerGrid.Children.Add(th);
        }
        stack.Children.Add(headerGrid);

        // Boutons
        stack.Children.Add(BuildCharterRow("Bouton",
            new Button { Content = "Enregistrer", Height = 34, Background = (Brush)FindResource("BrushIndigo"), Foreground = Brushes.White, FontSize = 10.5, FontWeight = FontWeights.Bold, Margin = new Thickness(2) },
            new Button { Content = "Enregistrer", Height = 34, Background = new SolidColorBrush(Color.FromRgb(0x1B, 0x23, 0x52)), Foreground = Brushes.White, FontSize = 10.5, FontWeight = FontWeights.Bold, Margin = new Thickness(2) },
            new Button { Content = "Enregistrer", Height = 34, Background = (Brush)new BrushConverter().ConvertFrom("#C9CCEA")!, Foreground = Brushes.White, FontSize = 10.5, FontWeight = FontWeights.Bold, Margin = new Thickness(2), IsEnabled = false },
            new Button { Content = "Supprimer", Height = 34, Background = Brushes.White, BorderBrush = (Brush)FindResource("BrushError"), BorderThickness = new Thickness(1.5), Foreground = (Brush)FindResource("BrushError"), FontSize = 10.5, FontWeight = FontWeights.Bold, Margin = new Thickness(2) }
        ));

        // Champs de saisie
        stack.Children.Add(BuildCharterRow("Champ de\nsaisie",
            BuildMiniField("Téléphone", "+237 6...", false, false, false),
            BuildMiniField("Téléphone", "+237 6 77|", true, false, false),
            BuildMiniField("Téléphone", "+237 6...", false, true, false),
            BuildMiniField("Téléphone", "+237 6 7", false, false, true, "Numéro invalide")
        ));

        // Puces de filtre
        stack.Children.Add(BuildCharterRow("Puce de filtre",
            new Border { Height = 28, CornerRadius = new CornerRadius(14), Background = Brushes.White, BorderBrush = (Brush)FindResource("BrushLine"), BorderThickness = new Thickness(1), Padding = new Thickness(6, 0, 6, 0), HorizontalAlignment = HorizontalAlignment.Center, Child = new TextBlock { Text = "Essayage", FontSize = 10.5, FontWeight = FontWeights.SemiBold, Foreground = (Brush)FindResource("BrushMuted"), VerticalAlignment = VerticalAlignment.Center } },
            new Border { Height = 28, CornerRadius = new CornerRadius(14), Background = (Brush)FindResource("BrushIndigo"), Padding = new Thickness(6, 0, 6, 0), HorizontalAlignment = HorizontalAlignment.Center, Child = new TextBlock { Text = "Essayage", FontSize = 10.5, FontWeight = FontWeights.Bold, Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center } },
            new Border { Height = 28, CornerRadius = new CornerRadius(14), Background = (Brush)new BrushConverter().ConvertFrom("#EEEFF6")!, BorderBrush = (Brush)new BrushConverter().ConvertFrom("#E2E3EF")!, BorderThickness = new Thickness(1), Padding = new Thickness(6, 0, 6, 0), HorizontalAlignment = HorizontalAlignment.Center, Child = new TextBlock { Text = "Essayage", FontSize = 10.5, FontWeight = FontWeights.SemiBold, Foreground = (Brush)new BrushConverter().ConvertFrom("#A6AAC8")!, VerticalAlignment = VerticalAlignment.Center } },
            new Border { Height = 28, CornerRadius = new CornerRadius(14), Background = (Brush)FindResource("BrushErrorSoft"), BorderBrush = (Brush)FindResource("BrushError"), BorderThickness = new Thickness(1), Padding = new Thickness(6, 0, 6, 0), HorizontalAlignment = HorizontalAlignment.Center, Child = new TextBlock { Text = "Essayage", FontSize = 10.5, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushError"), VerticalAlignment = VerticalAlignment.Center } }
        ));

        // Case à cocher
        stack.Children.Add(BuildCharterRow("Case à cocher",
            new Border { Width = 20, Height = 20, CornerRadius = new CornerRadius(6), BorderBrush = (Brush)new BrushConverter().ConvertFrom("#B9BDDA")!, BorderThickness = new Thickness(1.5), Background = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center },
            new Border { Width = 20, Height = 20, CornerRadius = new CornerRadius(6), Background = (Brush)FindResource("BrushIndigo"), HorizontalAlignment = HorizontalAlignment.Center, Child = CreateIcon("check", Brushes.White, size: 12, strokeThickness: 2.2) },
            new Border { Width = 20, Height = 20, CornerRadius = new CornerRadius(6), Background = (Brush)new BrushConverter().ConvertFrom("#EEEFF6")!, BorderBrush = (Brush)new BrushConverter().ConvertFrom("#D8DAEA")!, BorderThickness = new Thickness(1.5), HorizontalAlignment = HorizontalAlignment.Center },
            new Border { Width = 20, Height = 20, CornerRadius = new CornerRadius(6), BorderBrush = (Brush)FindResource("BrushError"), BorderThickness = new Thickness(1.5), Background = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center }
        ));

        // Messages et chargement
        stack.Children.Add(new TextBlock { Text = "Messages et chargement", FontSize = 13, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushMuted"), Margin = new Thickness(0, 16, 0, 8) });
        var msgGrid = new UniformGrid { Columns = 2, Margin = new Thickness(0, 0, 0, 14) };
        msgGrid.Children.Add(BuildToast("Client enregistré avec succès.", ToastType.Success));
        msgGrid.Children.Add(BuildToast("Échec de l'enregistrement. Réessayez.", ToastType.Error));
        msgGrid.Children.Add(BuildToast("Synchronisation en cours…", ToastType.Info));

        var spinnerBtn = new Button { Height = 38, Background = (Brush)FindResource("BrushIndigo"), Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 11.5, Margin = new Thickness(2) };
        var spinStack = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
        spinStack.Children.Add(CreateIcon("clock", Brushes.White, size: 12));
        spinStack.Children.Add(new TextBlock { Text = " Enregistrement…", FontSize = 11, VerticalAlignment = VerticalAlignment.Center });
        spinnerBtn.Content = spinStack;
        msgGrid.Children.Add(spinnerBtn);
        stack.Children.Add(msgGrid);

        // 3 Concepts
        var concepts = new UniformGrid { Columns = 3, Margin = new Thickness(0, 6, 0, 0) };
        concepts.Children.Add(BuildConceptBox("Un seul langage", "Indigo pour l'action, rose pour l'accent, vert pour le succès, rouge pour l'erreur."));
        concepts.Children.Add(BuildConceptBox("Jamais la couleur seule", "Chaque erreur s'accompagne d'une icône et d'un message écrit."));
        concepts.Children.Add(BuildConceptBox("Zones tactiles", "Boutons principaux et champs de saisie d'au moins 48 px de haut."));
        stack.Children.Add(concepts);

        scroll.Content = stack;
        return scroll;
    }

    private FrameworkElement BuildCharterRow(string title, UIElement e1, UIElement e2, UIElement e3, UIElement e4)
    {
        var g = new Grid { Margin = new Thickness(0, 0, 0, 10) };
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var t = new TextBlock { Text = title, FontSize = 11, FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(t, 0); g.Children.Add(t);
        Grid.SetColumn(e1, 1); g.Children.Add(e1);
        Grid.SetColumn(e2, 2); g.Children.Add(e2);
        Grid.SetColumn(e3, 3); g.Children.Add(e3);
        Grid.SetColumn(e4, 4); g.Children.Add(e4);
        return g;
    }

    private FrameworkElement BuildMiniField(string label, string val, bool isActive, bool isDisabled, bool isError, string errorMsg = "")
    {
        var s = new StackPanel { Margin = new Thickness(2) };
        var b = new Border
        {
            CornerRadius = new CornerRadius(8),
            Background = isDisabled ? (Brush)new BrushConverter().ConvertFrom("#EEEFF6")! : (isError ? (Brush)FindResource("BrushErrorSoft") : Brushes.White),
            BorderBrush = isError ? (Brush)FindResource("BrushError") : (isActive ? (Brush)FindResource("BrushIndigo") : (isDisabled ? (Brush)new BrushConverter().ConvertFrom("#E2E3EF")! : (Brush)FindResource("BrushLine"))),
            BorderThickness = new Thickness(isActive ? 1.5 : 1),
            Padding = new Thickness(6, 4, 6, 4)
        };
        var inner = new StackPanel();
        inner.Children.Add(new TextBlock { Text = label, FontSize = 9, FontWeight = FontWeights.Bold, Foreground = isError ? (Brush)FindResource("BrushError") : (Brush)FindResource("BrushMuted") });
        inner.Children.Add(new TextBlock { Text = val, FontSize = 10.5, FontWeight = FontWeights.SemiBold, Foreground = isDisabled ? (Brush)new BrushConverter().ConvertFrom("#A6AAC8")! : (Brush)FindResource("BrushInk") });
        b.Child = inner;
        s.Children.Add(b);

        if (!string.IsNullOrEmpty(errorMsg))
        {
            var errStack = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 0) };
            errStack.Children.Add(CreateIcon("alert", (Brush)FindResource("BrushError"), size: 10));
            errStack.Children.Add(new TextBlock { Text = " " + errorMsg, FontSize = 8.5, Foreground = (Brush)FindResource("BrushError") });
            s.Children.Add(errStack);
        }
        return s;
    }

    private FrameworkElement BuildConceptBox(string title, string desc)
    {
        var b = new Border
        {
            CornerRadius = new CornerRadius(12),
            Background = (Brush)FindResource("BrushChalk"),
            Padding = new Thickness(10, 10, 10, 10),
            Margin = new Thickness(2)
        };
        var s = new StackPanel();
        s.Children.Add(new TextBlock { Text = title, FontSize = 11.5, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushIndigo"), Margin = new Thickness(0, 0, 0, 4) });
        s.Children.Add(new TextBlock { Text = desc, FontSize = 10.5, Foreground = (Brush)FindResource("BrushMuted"), TextWrapping = TextWrapping.Wrap });
        b.Child = s;
        return b;
    }

    private FrameworkElement BuildToast(string message, ToastType type)
    {
        var (bg, fg, iconKey) = type switch
        {
            ToastType.Success => ("#DDF3E8", "#1E7A50", "check"),
            ToastType.Error => ("#FDE8EA", "#B02A3A", "alert"),
            ToastType.Info => ("#E4E5F2", "#26306B", "alert"),
            _ => ("#E4E5F2", "#26306B", "alert")
        };

        var b = new Border
        {
            CornerRadius = new CornerRadius(10),
            Background = (Brush)new BrushConverter().ConvertFrom(bg)!,
            Padding = new Thickness(10, 6, 10, 6),
            Margin = new Thickness(2)
        };
        var s = new StackPanel { Orientation = Orientation.Horizontal };
        s.Children.Add(CreateIcon(iconKey, (Brush)new BrushConverter().ConvertFrom(fg)!, size: 12, strokeThickness: 2.0));
        s.Children.Add(new TextBlock { Text = " " + message, FontSize = 10, FontWeight = FontWeights.Bold, Foreground = (Brush)new BrushConverter().ConvertFrom(fg)!, VerticalAlignment = VerticalAlignment.Center });
        b.Child = s;
        return b;
    }

    // =========================================================================
    // 25. PROFIL & ATELIER
    // =========================================================================
    private FrameworkElement BuildProfileScreen()
    {
        var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var stack = new StackPanel { Margin = new Thickness(20, 16, 20, 96) };

        // 1. Profil Header
        var header = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 18) };
        var avatar = CreateAvatar("RN", "a1", 80);
        header.Children.Add(avatar);

        var nameText = new TextBlock
        {
            Text = "Rose Ndongo",
            FontSize = 22,
            FontWeight = FontWeights.Bold,
            Foreground = (Brush)FindResource("BrushInk"),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 2)
        };
        header.Children.Add(nameText);

        var shopText = new TextBlock
        {
            Text = "Atelier Rose d'Or · Bonamoussadi, Douala",
            FontSize = 13.5,
            Foreground = (Brush)FindResource("BrushMuted"),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        header.Children.Add(shopText);
        stack.Children.Add(header);

        // 2. Stats Atelier (3 columns)
        var statsCard = new Border
        {
            CornerRadius = new CornerRadius(18),
            Background = Brushes.White,
            BorderBrush = (Brush)FindResource("BrushLine"),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(16, 14, 16, 14),
            Margin = new Thickness(0, 0, 0, 18),
            SnapsToDevicePixels = true
        };
        var statsGrid = new UniformGrid { Columns = 3 };

        // Clients
        var col1 = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
        col1.Children.Add(new TextBlock { Text = "48", FontSize = 22, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushIndigo"), HorizontalAlignment = HorizontalAlignment.Center });
        col1.Children.Add(new TextBlock { Text = "Clients", FontSize = 12, Foreground = (Brush)FindResource("BrushMuted"), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 2, 0, 0) });
        statsGrid.Children.Add(col1);

        // Ce mois
        var col2 = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
        col2.Children.Add(new TextBlock { Text = "12", FontSize = 22, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushIndigo"), HorizontalAlignment = HorizontalAlignment.Center });
        col2.Children.Add(new TextBlock { Text = "Ce mois", FontSize = 12, Foreground = (Brush)FindResource("BrushMuted"), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 2, 0, 0) });
        statsGrid.Children.Add(col2);

        // À temps
        var col3 = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
        col3.Children.Add(new TextBlock { Text = "94%", FontSize = 22, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushGreenDark"), HorizontalAlignment = HorizontalAlignment.Center });
        col3.Children.Add(new TextBlock { Text = "À temps", FontSize = 12, Foreground = (Brush)FindResource("BrushMuted"), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 2, 0, 0) });
        statsGrid.Children.Add(col3);

        statsCard.Child = statsGrid;
        stack.Children.Add(statsCard);

        // 3. Panneau de Démo (Soutenance Orale)
        var demoPanel = new Border
        {
            CornerRadius = new CornerRadius(18),
            Background = (Brush)FindResource("BrushTapeSoft"),
            BorderBrush = (Brush)FindResource("BrushTape"),
            BorderThickness = new Thickness(1.5),
            Padding = new Thickness(18, 16, 18, 16),
            Margin = new Thickness(0, 0, 0, 18),
            SnapsToDevicePixels = true
        };
        var demoStack = new StackPanel();

        var demoTitleRow = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        var iconWrap = new Border
        {
            Width = 26,
            Height = 26,
            CornerRadius = new CornerRadius(13),
            Background = (Brush)FindResource("BrushTape"),
            Margin = new Thickness(0, 0, 8, 0)
        };
        iconWrap.Child = new TextBlock { Text = "⚡", FontSize = 13, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        demoTitleRow.Children.Add(iconWrap);
        demoTitleRow.Children.Add(new TextBlock { Text = "Panneau de Démo (Soutenance)", FontSize = 15, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushInk"), VerticalAlignment = VerticalAlignment.Center });
        demoStack.Children.Add(demoTitleRow);

        demoStack.Children.Add(new TextBlock
        {
            Text = "Basculez instantanément l'état des services mock pour présenter les scénarios au jury :",
            FontSize = 12.5,
            Foreground = (Brush)FindResource("BrushInk"),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 8, 0, 12)
        });

        // 2x2 Buttons Grid
        var demoGrid = new UniformGrid { Columns = 2, Margin = new Thickness(0, 0, 0, 8) };

        var btnNormal = new Button
        {
            Content = "Normal",
            Height = 42,
            Background = (Brush)FindResource("BrushIndigo"),
            Foreground = Brushes.White,
            FontWeight = FontWeights.Bold,
            FontSize = 12.5,
            Margin = new Thickness(0, 0, 4, 8),
            Cursor = Cursors.Hand
        };
        btnNormal.Click += (s, e) => NavigateTo("Clients");
        demoGrid.Children.Add(btnNormal);

        var btnEmpty = new Button
        {
            Content = "Vide (Empty)",
            Height = 42,
            Background = Brushes.White,
            BorderBrush = (Brush)FindResource("BrushLine"),
            BorderThickness = new Thickness(1.5),
            Foreground = (Brush)FindResource("BrushInk"),
            FontWeight = FontWeights.Bold,
            FontSize = 12.5,
            Margin = new Thickness(4, 0, 0, 8),
            Cursor = Cursors.Hand
        };
        btnEmpty.Click += (s, e) => NavigateTo("ClientsEmpty");
        demoGrid.Children.Add(btnEmpty);

        var btnError = new Button
        {
            Content = "Erreur NET-01",
            Height = 42,
            Background = (Brush)FindResource("BrushErrorSoft"),
            BorderBrush = (Brush)FindResource("BrushError"),
            BorderThickness = new Thickness(1.5),
            Foreground = (Brush)FindResource("BrushError"),
            FontWeight = FontWeights.Bold,
            FontSize = 12.5,
            Margin = new Thickness(0, 0, 4, 0),
            Cursor = Cursors.Hand
        };
        btnError.Click += (s, e) => NavigateTo("ClientsError");
        demoGrid.Children.Add(btnError);

        var btnSlow = new Button
        {
            Content = "Lent (5 s)",
            Height = 42,
            Background = (Brush)FindResource("BrushTape"),
            Foreground = (Brush)FindResource("BrushInk"),
            FontWeight = FontWeights.Bold,
            FontSize = 12.5,
            Margin = new Thickness(4, 0, 0, 0),
            Cursor = Cursors.Hand
        };
        btnSlow.Click += (s, e) => NavigateTo("ClientsLoading");
        demoGrid.Children.Add(btnSlow);

        demoStack.Children.Add(demoGrid);

        var btnReset = new Button
        {
            Content = "Réinitialiser les données",
            Height = 40,
            Background = Brushes.White,
            BorderBrush = (Brush)FindResource("BrushLine"),
            BorderThickness = new Thickness(1),
            Foreground = (Brush)FindResource("BrushInk"),
            FontWeight = FontWeights.SemiBold,
            FontSize = 12.5,
            Margin = new Thickness(0, 4, 0, 8),
            Cursor = Cursors.Hand
        };
        btnReset.Click += (s, e) => { MessageBox.Show("Données de démo réinitialisées avec succès !", "Sweet Tailor"); NavigateTo("Clients"); };
        demoStack.Children.Add(btnReset);

        var btnCharter = new Button
        {
            Content = "📐 Voir la charte des composants (Écran 24)",
            Height = 44,
            Background = (Brush)FindResource("BrushIndigo"),
            Foreground = Brushes.White,
            FontWeight = FontWeights.Bold,
            FontSize = 12.5,
            Cursor = Cursors.Hand
        };
        btnCharter.Click += (s, e) => NavigateTo("Components");
        demoStack.Children.Add(btnCharter);

        demoPanel.Child = demoStack;
        stack.Children.Add(demoPanel);

        // 4. Coordonnées de l'atelier
        stack.Children.Add(new TextBlock { Text = "Coordonnées de l'atelier", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushInk"), Margin = new Thickness(0, 0, 0, 8) });
        var contactCard = new Border
        {
            CornerRadius = new CornerRadius(18),
            Background = Brushes.White,
            BorderBrush = (Brush)FindResource("BrushLine"),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(16, 12, 16, 12),
            Margin = new Thickness(0, 0, 0, 18),
            SnapsToDevicePixels = true
        };
        var contactStack = new StackPanel();

        var mailRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 8) };
        mailRow.Children.Add(CreateIcon("mail", (Brush)FindResource("BrushIndigo"), size: 18));
        mailRow.Children.Add(new TextBlock { Text = " rose.ndongo@mail.com", FontSize = 13.5, Foreground = (Brush)FindResource("BrushInk"), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 0, 0) });
        contactStack.Children.Add(mailRow);

        contactStack.Children.Add(new Border { Height = 1, Background = (Brush)FindResource("BrushMist"), Margin = new Thickness(0, 4, 0, 8) });

        var phoneRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
        phoneRow.Children.Add(CreateIcon("phone", (Brush)FindResource("BrushIndigo"), size: 18));
        phoneRow.Children.Add(new TextBlock { Text = " +237 6 77 00 00 00", FontSize = 13.5, Foreground = (Brush)FindResource("BrushInk"), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 0, 0) });
        contactStack.Children.Add(phoneRow);

        contactCard.Child = contactStack;
        stack.Children.Add(contactCard);

        // 5. Préférences & Sauvegarde
        stack.Children.Add(new TextBlock { Text = "Préférences & Sauvegarde", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushInk"), Margin = new Thickness(0, 0, 0, 8) });
        var prefCard = new Border
        {
            CornerRadius = new CornerRadius(18),
            Background = Brushes.White,
            BorderBrush = (Brush)FindResource("BrushLine"),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(16, 12, 16, 12),
            Margin = new Thickness(0, 0, 0, 20),
            SnapsToDevicePixels = true
        };
        var prefStack = new StackPanel();

        // Switch 1: Sauvegarde
        var s1 = new DockPanel { Margin = new Thickness(0, 4, 0, 8) };
        var s1Toggle = BuildSwitch(true);
        DockPanel.SetDock(s1Toggle, Dock.Right);
        s1.Children.Add(s1Toggle);
        var s1Text = new StackPanel();
        s1Text.Children.Add(new TextBlock { Text = "Sauvegarde locale automatique", FontSize = 13.5, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushInk") });
        s1Text.Children.Add(new TextBlock { Text = "Données persistées sur le terminal", FontSize = 11.5, Foreground = (Brush)FindResource("BrushMuted") });
        s1.Children.Add(s1Text);
        prefStack.Children.Add(s1);

        prefStack.Children.Add(new Border { Height = 1, Background = (Brush)FindResource("BrushMist"), Margin = new Thickness(0, 4, 0, 8) });

        // Switch 2: Rappels
        var s2 = new DockPanel { Margin = new Thickness(0, 4, 0, 4) };
        var s2Toggle = BuildSwitch(true);
        DockPanel.SetDock(s2Toggle, Dock.Right);
        s2.Children.Add(s2Toggle);
        var s2Text = new StackPanel();
        s2Text.Children.Add(new TextBlock { Text = "Rappels d'essayages", FontSize = 13.5, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("BrushInk") });
        s2Text.Children.Add(new TextBlock { Text = "Notification 48h avant l'échéance", FontSize = 11.5, Foreground = (Brush)FindResource("BrushMuted") });
        s2.Children.Add(s2Text);
        prefStack.Children.Add(s2);

        prefCard.Child = prefStack;
        stack.Children.Add(prefCard);

        // 6. Bouton Déconnexion
        var logoutBtn = new Button
        {
            Content = "Se déconnecter",
            Height = 50,
            Background = Brushes.White,
            BorderBrush = (Brush)FindResource("BrushError"),
            BorderThickness = new Thickness(1.5),
            Foreground = (Brush)FindResource("BrushError"),
            FontWeight = FontWeights.Bold,
            FontSize = 14.5,
            Cursor = Cursors.Hand
        };
        logoutBtn.Click += (s, e) =>
        {
            _isLoggedIn = false;
            NavigateTo("Login");
        };
        stack.Children.Add(logoutBtn);

        scroll.Content = stack;
        return scroll;
    }

    private FrameworkElement BuildSwitch(bool isChecked)
    {
        var track = new Border
        {
            Width = 44,
            Height = 26,
            CornerRadius = new CornerRadius(13),
            Background = isChecked ? (Brush)FindResource("BrushIndigo") : (Brush)FindResource("BrushMist"),
            Cursor = Cursors.Hand,
            SnapsToDevicePixels = true
        };
        var thumb = new Border
        {
            Width = 20,
            Height = 20,
            CornerRadius = new CornerRadius(10),
            Background = Brushes.White,
            HorizontalAlignment = isChecked ? HorizontalAlignment.Right : HorizontalAlignment.Left,
            Margin = new Thickness(3)
        };
        track.Child = thumb;

        track.MouseLeftButtonDown += (s, e) =>
        {
            isChecked = !isChecked;
            track.Background = isChecked ? (Brush)FindResource("BrushIndigo") : (Brush)FindResource("BrushMist");
            thumb.HorizontalAlignment = isChecked ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        };

        return track;
    }
}