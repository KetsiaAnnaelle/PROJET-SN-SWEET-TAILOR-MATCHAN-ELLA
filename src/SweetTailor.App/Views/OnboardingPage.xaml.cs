using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using SweetTailor.Core.ViewModels;
using MPath = Microsoft.Maui.Controls.Shapes.Path;

namespace SweetTailor.App.Views;

// =====================================================================================
//  PAGE
// =====================================================================================
public partial class OnboardingPage : ContentPage
{
    private const string OnboardingDoneKey = "HasCompletedOnboardingV2";
    private const string LoginRoute = "//LoginPage"; // route déclarée dans AppShell (écran 5)

    private static readonly Color Indigo = Color.FromArgb("#26306B");
    private static readonly Color Mist = Color.FromArgb("#E4E5F2");
    private static readonly Color Rose = Color.FromArgb("#E8607F");

    private readonly OnboardingViewModel _viewModel;
    private readonly Border[] _dots;
    private bool _isNavigating;

    public OnboardingPage(OnboardingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _dots = new[] { Dot0, Dot1, Dot2 };

        OnboardingCarousel.PositionChanged += OnCarouselPositionChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _isNavigating = false;
        UpdateUi(OnboardingCarousel.Position, animate: false);
        StartArrowNudge();

        // laisse le temps aux slides de se dessiner puis lance l'animation de la slide courante
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(250),
            () => OnboardingSlideView.Activate(OnboardingCarousel.Position));
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        NextButton.AbortAnimation("nudge");
    }

    // Bouton retour Android : slide précédente, ne quitte pas l'onboarding
    protected override bool OnBackButtonPressed()
    {
        if (OnboardingCarousel.Position > 0)
            OnboardingCarousel.ScrollTo(OnboardingCarousel.Position - 1, animate: true);
        return true;
    }

    private void OnCarouselPositionChanged(object? sender, PositionChangedEventArgs e)
    {
        UpdateUi(e.CurrentPosition, animate: true);
        OnboardingSlideView.Activate(e.CurrentPosition);
    }

    private void UpdateUi(int position, bool animate)
    {
        var last = Math.Max(_viewModel.Slides.Count - 1, 0);
        position = Math.Clamp(position, 0, last);
        var isLast = position == last;

        NextButton.Text = isLast ? "Commencer" : "Suivant  →";
        NextButton.BackgroundColor = isLast ? Rose : Indigo;

        for (var i = 0; i < _dots.Length; i++)
        {
            var dot = _dots[i];
            var active = i == position;
            var target = active ? 26d : 8d;
            dot.BackgroundColor = active ? Indigo : Mist;

            if (animate)
            {
                dot.AbortAnimation("dotWidth");
                new Animation(v => dot.WidthRequest = v, dot.WidthRequest, target)
                    .Commit(dot, "dotWidth", 16, 260, Easing.CubicOut);
            }
            else
            {
                dot.WidthRequest = target;
            }
        }
    }

    // petite flèche qui "respire" sur le bouton Suivant
    private void StartArrowNudge()
    {
        NextButton.AbortAnimation("nudge");
        new Animation(v => NextButton.TranslationX = 3.5 * (0.5 - 0.5 * Math.Cos(v)), 0, Math.PI * 2)
            .Commit(NextButton, "nudge", 16, 1500, Easing.Linear, null, () => true);
    }

    // retour tactile : léger "press"
    private static void Press(VisualElement v)
    {
        v.AbortAnimation("press");
        var a = new Animation();
        a.Add(0, 0.35, new Animation(x => v.Scale = x, 1, 0.94, Easing.CubicOut));
        a.Add(0.35, 1, new Animation(x => v.Scale = x, 0.94, 1, Easing.SpringOut));
        a.Commit(v, "press", 16, 320);
    }

    // 2, 3, 4 → 5 (Connexion)
    private async void OnSkipClicked(object? sender, EventArgs e)
    {
        Press(SkipButton);
        await FinishAsync();
    }

    private async void OnNextClicked(object? sender, EventArgs e)
    {
        Press(NextButton);
        var last = _viewModel.Slides.Count - 1;

        if (OnboardingCarousel.Position < last)
            OnboardingCarousel.ScrollTo(OnboardingCarousel.Position + 1, animate: true);
        else
            await FinishAsync(); // « Commencer »
    }

    private async Task FinishAsync()
    {
        if (_isNavigating) return;
        _isNavigating = true;

        Preferences.Set(OnboardingDoneKey, true);
        await Shell.Current.GoToAsync(LoginRoute);
    }
}

// =====================================================================================
//  DESSIN DU MÈTRE RUBAN (graduations)
// =====================================================================================
internal sealed class RulerDrawable : IDrawable
{
    public float U = 1f;
    public float Small = 7f;
    public float Big = 13f;

    public void Draw(ICanvas canvas, RectF r)
    {
        canvas.FillColor = Color.FromArgb("#F4C95D");
        canvas.FillRectangle(r);

        canvas.StrokeColor = Color.FromArgb("#14173A");
        canvas.StrokeSize = 1f;
        for (float x = 0; x < r.Width; x += 8f * U)
            canvas.DrawLine(x + 0.5f, r.Bottom, x + 0.5f, r.Bottom - Small * U);

        canvas.StrokeSize = 1.5f;
        for (float x = 0; x < r.Width; x += 40f * U)
            canvas.DrawLine(x + 0.75f, r.Bottom, x + 0.75f, r.Bottom - Big * U);
    }
}

// =====================================================================================
//  SLIDE D'ONBOARDING : illustration dessinée à l'échelle réelle + animations + texte
//  Base de dessin = maquette (342 x 440). Tout est recalculé selon l'espace disponible.
// =====================================================================================
public sealed class OnboardingSlideView : ContentView
{
    // ---- Polices de la maquette (à enregistrer dans MauiProgram, voir explications) ----
    private const string Body = "DMSans";
    private const string Bold = "DMSansBold";
    private const string Display = "BricolageExtraBold";

    // ---- Palette de la maquette ----
    private static readonly Color Indigo = Color.FromArgb("#26306B");
    private static readonly Color Ink = Color.FromArgb("#14173A");
    private static readonly Color Rose = Color.FromArgb("#E8607F");
    private static readonly Color RoseSoft = Color.FromArgb("#FCE4EA");
    private static readonly Color RoseText = Color.FromArgb("#B8325A");
    private static readonly Color Tape = Color.FromArgb("#F4C95D");
    private static readonly Color TapeSoft = Color.FromArgb("#FDF3D6");
    private static readonly Color TapeText = Color.FromArgb("#8A6A10");
    private static readonly Color Chalk = Color.FromArgb("#F6F5FA");
    private static readonly Color Mist = Color.FromArgb("#E4E5F2");
    private static readonly Color Line = Color.FromArgb("#E8E9F3");
    private static readonly Color Muted = Color.FromArgb("#6B7094");
    private static readonly Color Green = Color.FromArgb("#2E9E6B");
    private static readonly Color White = Colors.White;

    private const double DesignW = 342, DesignH = 440;
    private static readonly PathGeometryConverter GeoConv = new();

    // ---- Notification "slide active" ----
    public static int CurrentSlide { get; private set; }
    private static event Action<int>? SlideActivated;
    public static void Activate(int index)
    {
        CurrentSlide = index;
        SlideActivated?.Invoke(index);
    }

    // ---- Propriétés XAML ----
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(OnboardingSlideView), string.Empty,
        propertyChanged: (b, _, n) => ((OnboardingSlideView)b)._title.Text = n as string ?? string.Empty);

    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(
        nameof(Description), typeof(string), typeof(OnboardingSlideView), string.Empty,
        propertyChanged: (b, _, n) => ((OnboardingSlideView)b)._desc.Text = n as string ?? string.Empty);

    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string Description { get => (string)GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }

    private int _variant;
    public int Variant
    {
        get => _variant;
        set { _variant = value; ApplyBackground(); }
    }

    // ---- Vues ----
    private readonly Border _artBorder;
    private readonly Grid _artHost;
    private readonly Label _title;
    private readonly Label _desc;
    private AbsoluteLayout? _canvas;

    // ---- État de dessin ----
    private double _u = 1, _ox, _oy, _w, _h;
    private bool _built;
    private bool _first;
    private CancellationTokenSource? _cts;
    private Action? _script;

    private sealed record Item(VisualElement V, double Rot, double Op);
    private readonly List<Item> _items = new();
    private readonly List<(Span Span, int Target)> _counters = new();

    public OnboardingSlideView()
    {
        _title = new Label
        {
            FontFamily = Display,
            FontSize = 26,
            TextColor = Ink,
            LineHeight = 1.12,
            FontAutoScalingEnabled = false
        };
        _desc = new Label
        {
            FontFamily = Body,
            FontSize = 14.5,
            TextColor = Muted,
            LineHeight = 1.45,
            FontAutoScalingEnabled = false
        };

        _artHost = new Grid();
        _artBorder = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(0, 0, 40, 40) },
            Content = _artHost
        };

        var text = new VerticalStackLayout
        {
            Spacing = 10,
            Padding = new Thickness(20, 18, 20, 8),
            MinimumHeightRequest = 150,
            Children = { _title, _desc }
        };

        var root = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            }
        };
        root.Add(_artBorder);
        root.Add(text);
        Grid.SetRow(text, 1);
        Content = root;

        _artHost.SizeChanged += (_, _) => Rebuild();
        SizeChanged += (_, _) => UpdateTextSizes();

        HandlerChanged += (_, _) =>
        {
            SlideActivated -= OnSlideActivated;
            if (Handler != null) SlideActivated += OnSlideActivated;
            else StopAll();
        };
    }

    // =================================================================================
    //  Fond dégradé de la zone illustration
    // =================================================================================
    private void ApplyBackground()
    {
        var (c1, c2) = Variant switch
        {
            0 => ("#FCE4EA", "#F9CFDA"),
            1 => ("#FDF3D6", "#F9E3A0"),
            _ => ("#ECEDF8", "#CDD0EC")
        };
        _artBorder.Background = new LinearGradientBrush(
            new GradientStopCollection
            {
                new GradientStop(Color.FromArgb(c1), 0f),
                new GradientStop(Color.FromArgb(c2), 1f)
            },
            new Point(0.55, 0), new Point(0.45, 1));
    }

    private void UpdateTextSizes()
    {
        if (Width <= 0) return;
        var k = Math.Clamp(Width / 360d, 0.88, 1.3);
        _title.FontSize = 26 * k;
        _desc.FontSize = 14.5 * k;
    }

    // =================================================================================
    //  Dessin de l'illustration à la taille réelle disponible
    // =================================================================================
    private void Rebuild()
    {
        var w = _artHost.Width;
        var h = _artHost.Height;
        if (w <= 0 || h <= 0) return;
        if (_built && Math.Abs(w - _w) < 1 && Math.Abs(h - _h) < 1) return;

        StopAll();

        _w = w;
        _h = h;
        _u = Math.Clamp(Math.Min(w / DesignW, h / DesignH), 0.5, 1.8);
        _ox = (w - DesignW * _u) / 2;
        _oy = (h - DesignH * _u) / 2;

        _items.Clear();
        _counters.Clear();
        _script = null;
        _artHost.Children.Clear();
        _canvas = new AbsoluteLayout();
        _artHost.Children.Add(_canvas);

        switch (Variant)
        {
            case 0: BuildMeasures(); break;
            case 1: BuildOrders(); break;
            default: BuildCatalog(); break;
        }

        _built = true;
        if (CurrentSlide == Variant) Play(first: true);
    }

    // ---------- helpers de dessin ----------
    private double S(double v) => v * _u;

    private RoundRectangle RR(double r) => new() { CornerRadius = new CornerRadius(S(r)) };

    private Shadow Sh() => new()
    {
        Brush = new SolidColorBrush(Ink),
        Opacity = 0.30f,
        Radius = (float)S(14),
        Offset = new Point(0, S(10))
    };

    private Label Lbl(string text, double size, Color color, string font, bool center = false)
    {
        var l = new Label
        {
            Text = text,
            FontSize = S(size),
            TextColor = color,
            FontFamily = font,
            LineBreakMode = LineBreakMode.NoWrap,
            FontAutoScalingEnabled = false
        };
        if (center)
        {
            l.HorizontalTextAlignment = TextAlignment.Center;
            l.HorizontalOptions = LayoutOptions.Center;
        }
        return l;
    }

    // position en unités de la maquette (centrée dans l'espace disponible)
    private void Put(View v, double x, double y, double w = -1, double h = -1)
    {
        AbsoluteLayout.SetLayoutBounds(v, new Rect(_ox + S(x), _oy + S(y), w < 0 ? -1 : S(w), h < 0 ? -1 : S(h)));
        _canvas!.Children.Add(v);
    }

    // position en pixels réels (pour ancrer les cercles aux bords)
    private void PutRaw(View v, double x, double y, double w, double h)
    {
        AbsoluteLayout.SetLayoutBounds(v, new Rect(x, y, w, h));
        _canvas!.Children.Add(v);
    }

    private static BoxView Circle(Color c, double d) =>
        new() { Color = c, CornerRadius = new CornerRadius(d / 2) };

    private T Reg<T>(T v) where T : VisualElement
    {
        _items.Add(new Item(v, v.Rotation, v.Opacity));
        return v;
    }

    private MPath Icon(string data, Color? stroke, double thick, double w, double h, Color? fill = null) => new()
    {
        Data = (Geometry)GeoConv.ConvertFromInvariantString(data)!,
        Stroke = stroke == null ? new SolidColorBrush(Colors.Transparent) : new SolidColorBrush(stroke),
        Fill = fill == null ? new SolidColorBrush(Colors.Transparent) : new SolidColorBrush(fill),
        StrokeThickness = S(thick),
        StrokeLineCap = PenLineCap.Round,
        StrokeLineJoin = PenLineJoin.Round,
        Aspect = Stretch.Uniform,
        WidthRequest = S(w),
        HeightRequest = S(h),
        HorizontalOptions = LayoutOptions.Center,
        VerticalOptions = LayoutOptions.Center
    };

    private GraphicsView Ruler(double width, double height, float small, float big) => new()
    {
        Drawable = new RulerDrawable { U = (float)_u, Small = small, Big = big },
        WidthRequest = S(width),
        HeightRequest = S(height)
    };

    private Border Pill(View content, Color bg, double padH = 10, double padV = 4) => new()
    {
        BackgroundColor = bg,
        StrokeThickness = 0,
        StrokeShape = RR(99),
        Padding = new Thickness(S(padH), S(padV)),
        VerticalOptions = LayoutOptions.Center,
        Content = content
    };

    // =================================================================================
    //  SLIDE 1 — Mesures
    // =================================================================================
    private void BuildMeasures()
    {
        PutRaw(Circle(TapeSoft, S(260)), S(-90), _oy + S(60), S(260), S(260));
        PutRaw(Circle(Color.FromArgb("#8CFFFFFF"), S(150)), _w + S(40) - S(150), _oy + S(260), S(150), S(150));

        // --- carte des mesures ---
        var header = new HorizontalStackLayout
        {
            Spacing = S(10),
            Children =
            {
                new Border
                {
                    WidthRequest = S(38), HeightRequest = S(38), StrokeThickness = 0,
                    StrokeShape = RR(19), BackgroundColor = RoseSoft,
                    Content = new Label
                    {
                        Text = "NM", FontFamily = Display, FontSize = S(13), TextColor = RoseText,
                        FontAutoScalingEnabled = false,
                        HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center
                    }
                },
                new VerticalStackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        Lbl("Nadège Mbarga", 14, Ink, Bold),
                        Lbl("Mesures du 12 sept.", 11, Muted, Body)
                    }
                }
            }
        };

        var rows = new VerticalStackLayout { Spacing = 0, Margin = new Thickness(0, S(8), 0, 0) };
        var data = new (string L, int V)[] { ("Poitrine", 92), ("Taille", 74), ("Hanches", 98), ("Longueur robe", 112) };
        for (var i = 0; i < data.Length; i++)
        {
            var last = i == data.Length - 1;
            rows.Add(MeasureRow(data[i].L, data[i].V, last));
            if (!last) rows.Add(DashedLine(190));
        }

        var card = new Border
        {
            WidthRequest = S(222),
            BackgroundColor = White,
            StrokeThickness = 0,
            StrokeShape = RR(20),
            Rotation = -5,
            Shadow = Sh(),
            Content = new VerticalStackLayout
            {
                Spacing = 0,
                Children =
                {
                    new VerticalStackLayout
                    {
                        Spacing = 0,
                        Padding = new Thickness(S(16), S(16), S(16), 0),
                        Children = { header, rows }
                    },
                    Ruler(222, 16, 5, 9)
                }
            }
        };
        Put(Reg(card), 60, 96, 222);

        // --- pastille "Taille 74 cm" ---
        var badge = new Border
        {
            BackgroundColor = Indigo,
            StrokeThickness = 0,
            StrokeShape = RR(14),
            Padding = new Thickness(S(13), S(9)),
            Rotation = -5,
            Shadow = Sh(),
            Content = Lbl("Taille 74 cm", 12.5, White, Bold)
        };
        Put(Reg(badge), 18, 62);

        // --- mètre ruban incliné ---
        var tape = new Border
        {
            StrokeThickness = 0,
            StrokeShape = RR(4),
            Rotation = -14,
            Shadow = Sh(),
            Content = Ruler(300, 22, 7, 13)
        };
        Put(Reg(tape), 20, 368, 300, 22);

        // --- rond rose avec check ---
        var check = new Border
        {
            BackgroundColor = Rose,
            StrokeThickness = 0,
            StrokeShape = RR(26),
            Shadow = Sh(),
            Content = Icon("M5 12.5l4.5 4.5L19 7.5", White, 2.2, 22, 16)
        };
        Put(Reg(check), 266, 314, 52, 52);

        _script = () =>
        {
            Enter(card, 0, dy: 50, s0: 0.92, r0: -8, len: 800, e: Easing.SpringOut);
            Enter(badge, 300, dy: -22, s0: 0.5, r0: 8, len: 650, e: Easing.SpringOut);
            Enter(tape, 420, dx: -80, r0: 6, len: 700);
            Enter(check, 760, s0: 0, len: 650, e: Easing.SpringOut);
            Count(250);

            Later(1500, () =>
            {
                Bob(card, 3, 3400);
                Bob(badge, 4, 2700, 1.2);
                Pulse(check, 0.08, 1700);
            });
        };
    }

    private View MeasureRow(string label, int value, bool last)
    {
        var span = new Span { Text = value.ToString(), FontFamily = Bold };
        var fs = new FormattedString();
        fs.Spans.Add(span);
        fs.Spans.Add(new Span { Text = " cm", FontFamily = Body, FontSize = S(10.5), TextColor = Muted });
        _counters.Add((span, value));

        var chip = new Border
        {
            BackgroundColor = Chalk,
            Stroke = new SolidColorBrush(Mist),
            StrokeThickness = 1,
            StrokeShape = RR(10),
            Padding = new Thickness(S(10), S(3)),
            VerticalOptions = LayoutOptions.Center,
            MinimumWidthRequest = S(60),
            Content = new Label
            {
                FormattedText = fs,
                FontSize = S(12.5),
                TextColor = Ink,
                FontAutoScalingEnabled = false,
                HorizontalTextAlignment = TextAlignment.Center
            }
        };

        var grid = new Grid
        {
            Padding = new Thickness(0, S(9), 0, last ? S(12) : S(9)),
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };
        var lbl = Lbl(label, 12.5, Ink, Body);
        lbl.VerticalOptions = LayoutOptions.Center;
        grid.Add(lbl);
        grid.Add(chip);
        Grid.SetColumn(chip, 1);
        return grid;
    }

    private View DashedLine(double width) => new Microsoft.Maui.Controls.Shapes.Line
    {
        X1 = 0, Y1 = 0.5, X2 = S(width), Y2 = 0.5,
        Stroke = new SolidColorBrush(Mist),
        StrokeThickness = 1,
        StrokeDashArray = new DoubleCollection { 3, 3 },
        WidthRequest = S(width),
        HeightRequest = 1
    };

    // =================================================================================
    //  SLIDE 2 — Commandes et échéances
    // =================================================================================
    private void BuildOrders()
    {
        PutRaw(Circle(Color.FromArgb("#80FFFFFF"), S(240)), _w + S(80) - S(240), _oy + S(50), S(240), S(240));
        PutRaw(Circle(RoseSoft, S(120)), S(-30), _oy + S(280), S(120), S(120));

        var row3 = OrderRow("17", "OCT.", "Robe de mariée", "Sandrine Kamga", "Couture", TapeSoft, TapeText, false, 288, 3, 12);
        row3.Opacity = 0.9;
        Put(Reg(row3), 34, 278, 288);

        var row2 = OrderRow("11", "OCT.", "Ensemble pagne", "Christelle Tchoua", "Couture", TapeSoft, TapeText, false, 290, -2, 12);
        Put(Reg(row2), 20, 196, 290);

        var row1 = OrderRow("08", "OCT.", "Robe de soirée", "Nadège Mbarga", "Essayage", RoseSoft, RoseText, true, 290, 1.5, 14);
        Put(Reg(row1), 30, 106, 290);

        // badge "Dans 5 jours"
        var days = new Border
        {
            BackgroundColor = Rose,
            StrokeThickness = 0,
            StrokeShape = RR(99),
            Padding = new Thickness(S(12), S(7)),
            Shadow = Sh(),
            Content = new HorizontalStackLayout
            {
                Spacing = S(6),
                Children =
                {
                    Icon("M12 3.5a8.5 8.5 0 1 0 0 17a8.5 8.5 0 1 0 0-17ZM12 7.5V12l3 2", White, 1.8, 14, 14),
                    new Label
                    {
                        Text = "Dans 5 jours", FontFamily = Bold, FontSize = S(12), TextColor = White,
                        FontAutoScalingEnabled = false, VerticalOptions = LayoutOptions.Center
                    }
                }
            }
        };
        Put(Reg(days), 24, 62);

        // badge "Prête à retirer"
        var tick = new Border
        {
            WidthRequest = S(26), HeightRequest = S(26), StrokeThickness = 0,
            StrokeShape = RR(13), BackgroundColor = Green,
            Content = Icon("M5 12.5l4.5 4.5L19 7.5", White, 3, 12, 9)
        };
        var ready = new Border
        {
            BackgroundColor = White,
            StrokeThickness = 0,
            StrokeShape = RR(99),
            Padding = new Thickness(S(8), S(8), S(14), S(8)),
            Shadow = Sh(),
            Content = new HorizontalStackLayout
            {
                Spacing = S(8),
                Children =
                {
                    tick,
                    new Label
                    {
                        Text = "Prête à retirer", FontFamily = Bold, FontSize = S(12.5), TextColor = Ink,
                        FontAutoScalingEnabled = false, VerticalOptions = LayoutOptions.Center
                    }
                }
            }
        };
        Put(Reg(ready), 24, 376);
        Reg(tick);

        _script = () =>
        {
            Enter(row1, 0, dx: 80, r0: 4, len: 700);
            Enter(row2, 130, dx: 80, r0: -4, len: 700);
            Enter(row3, 260, dx: 80, r0: 4, len: 700);
            Enter(days, 480, s0: 0.4, len: 650, e: Easing.SpringOut);
            Enter(ready, 700, dy: 50, s0: 0.9, len: 750, e: Easing.SpringOut);
            Enter(tick, 1050, s0: 0, len: 500, e: Easing.SpringOut);

            Later(1500, () =>
            {
                Bob(row1, 2.5, 3300);
                Bob(row2, 3, 3700, 1.5);
                Bob(row3, 2.5, 3100, 3);
                Pulse(days, 0.07, 1500);
                Bob(ready, 2, 2900, 2);
            });
        };
    }

    private Border OrderRow(string day, string month, string name, string client, string status,
                            Color pillBg, Color pillFg, bool hot, double width, double rotation, double padV)
    {
        var dateBox = new Border
        {
            WidthRequest = S(50),
            HeightRequest = S(46),
            StrokeThickness = 0,
            StrokeShape = RR(14),
            BackgroundColor = hot ? Rose : Chalk,
            VerticalOptions = LayoutOptions.Center,
            Content = new VerticalStackLayout
            {
                Spacing = 0,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    Lbl(day, 18, hot ? White : Ink, Display, true),
                    Lbl(month, 10, hot ? White : Muted, Bold, true)
                }
            }
        };

        var nameLbl = Lbl(name, 14, Ink, Bold);
        nameLbl.LineBreakMode = LineBreakMode.TailTruncation;
        var clientLbl = Lbl(client, 12, Muted, Body);
        clientLbl.LineBreakMode = LineBreakMode.TailTruncation;

        var mid = new VerticalStackLayout
        {
            Spacing = S(2),
            VerticalOptions = LayoutOptions.Center,
            Children = { nameLbl, clientLbl }
        };

        var pill = Pill(Lbl(status, 11, pillFg, Bold), pillBg);

        var grid = new Grid
        {
            ColumnSpacing = S(12),
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(new GridLength(S(50))),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };
        grid.Add(dateBox);
        grid.Add(mid);
        grid.Add(pill);
        Grid.SetColumn(mid, 1);
        Grid.SetColumn(pill, 2);

        return new Border
        {
            BackgroundColor = White,
            Stroke = new SolidColorBrush(Line),
            StrokeThickness = 1,
            StrokeShape = RR(18),
            Padding = new Thickness(S(14), S(padV)),
            Rotation = rotation,
            Shadow = Sh(),
            Content = grid
        };
    }

    // =================================================================================
    //  SLIDE 3 — Catalogue
    // =================================================================================
    private void BuildCatalog()
    {
        PutRaw(Circle(Color.FromArgb("#8CFFFFFF"), S(230)), S(-60), _oy + S(230), S(230), S(230));

        var tL1 = Tile("#FCE4EA", "#F4B3C4", 170, "M48 10Q60 22 72 10L80 40 70 62C84 100 98 140 104 172H16C22 140 36 100 50 62L40 40Z", Indigo, 53, 98);
        var tL2 = Tile("#3B4A9E", "#1B2352", 130, "M48 10c0 14 24 14 24 0l10 8 6 28-12 6 10 118H24L34 52l-12-6 6-28 10-8-2-0Z", Tape, 40, 97);
        var tR1 = Tile("#FDF3D6", "#F4C95D", 120, "M40 14Q60 30 80 14L108 36 96 62 84 54 88 160H32L36 54 24 62 12 36Z", Indigo, 58, 89);
        var tR2 = Tile("#ECEDF8", "#B7BCE0", 180, "M44 10 60 34 76 10 104 24 112 70 98 74 96 168H24L22 74 8 70 16 24Z", Indigo, 63, 96);
        Reg(tL1); Reg(tL2); Reg(tR1); Reg(tR2);

        var grid = new HorizontalStackLayout
        {
            Spacing = S(10),
            Rotation = -7,
            Children =
            {
                new VerticalStackLayout { Spacing = S(12), Margin = new Thickness(0, S(26), 0, 0), Children = { tL1, tL2 } },
                new VerticalStackLayout { Spacing = S(12), Children = { tR1, tR2 } }
            }
        };
        Put(grid, 34, 70);

        // coeur
        var heart = new Border
        {
            BackgroundColor = White,
            StrokeThickness = 0,
            StrokeShape = RR(23),
            Shadow = Sh(),
            Content = Icon("M12 20s-7.5-4.6-7.5-10A4.2 4.2 0 0 1 12 7.6 4.2 4.2 0 0 1 19.5 10C19.5 15.4 12 20 12 20Z",
                           Rose, 1.8, 22, 19, Rose)
        };
        Put(Reg(heart), 270, 96, 46, 46);

        // pastille
        var pill = new Border
        {
            BackgroundColor = Indigo,
            StrokeThickness = 0,
            StrokeShape = RR(99),
            Padding = new Thickness(S(16), S(9)),
            Shadow = Sh(),
            Content = Lbl("Catalogue de modèles", 12.5, White, Bold)
        };
        Put(Reg(pill), 22, 371);

        _script = () =>
        {
            Enter(tL1, 0, dy: 80, s0: 0.9, len: 750, e: Easing.SpringOut);
            Enter(tR1, 100, dy: 80, s0: 0.9, len: 750, e: Easing.SpringOut);
            Enter(tL2, 200, dy: 80, s0: 0.9, len: 750, e: Easing.SpringOut);
            Enter(tR2, 300, dy: 80, s0: 0.9, len: 750, e: Easing.SpringOut);
            Enter(heart, 600, s0: 0, len: 600, e: Easing.SpringOut);
            Enter(pill, 700, dx: -70, len: 650);

            Later(1500, () =>
            {
                Bob(tL1, 3, 3500);
                Bob(tR1, 3, 3100, 2);
                Bob(tL2, 3, 3300, 4);
                Bob(tR2, 3, 3700, 1);
                Beat(heart);
            });
        };
    }

    private Border Tile(string c1, string c2, double height, string path, Color fill, double gw, double gh) => new()
    {
        WidthRequest = S(140),
        HeightRequest = S(height),
        StrokeThickness = 0,
        StrokeShape = RR(18),
        Background = new LinearGradientBrush(
            new GradientStopCollection
            {
                new GradientStop(Color.FromArgb(c1), 0f),
                new GradientStop(Color.FromArgb(c2), 1f)
            },
            new Point(0.3, 0), new Point(0.7, 1)),
        Content = Icon(path, null, 0, gw, gh, fill)
    };

    // =================================================================================
    //  ANIMATIONS
    // =================================================================================
    private void OnSlideActivated(int index)
    {
        if (index == Variant) Play(first: false);
        else StopAll();
    }

    private void Play(bool first)
    {
        if (_script == null) return;
        StopAll();
        _first = first;
        _cts = new CancellationTokenSource();
        EnterText();
        _script();
    }

    private void StopAll()
    {
        _cts?.Cancel();
        _cts = null;

        foreach (var it in _items)
        {
            it.V.AbortAnimation("enter");
            it.V.AbortAnimation("idle");
            it.V.AbortAnimation("idle2");
            it.V.TranslationX = 0;
            it.V.TranslationY = 0;
            it.V.Scale = 1;
            it.V.Rotation = it.Rot;
            it.V.Opacity = it.Op;
        }

        foreach (var (span, target) in _counters)
            span.Text = target.ToString();

        _canvas?.AbortAnimation("count");
        _title.AbortAnimation("text");
        _desc.AbortAnimation("text");
        _title.Opacity = 1; _title.TranslationY = 0;
        _desc.Opacity = 1; _desc.TranslationY = 0;
    }

    private async void Later(uint ms, Action action)
    {
        var cts = _cts;
        if (cts == null) return;
        try
        {
            await Task.Delay((int)ms, cts.Token);
            action();
        }
        catch (OperationCanceledException) { }
    }

    // Entrée d'un élément : translation + zoom + rotation + fondu, avec délai
    private void Enter(VisualElement v, uint delay, double dx = 0, double dy = 0, double s0 = 1,
                       double r0 = 0, uint len = 650, Easing? e = null)
    {
        e ??= Easing.CubicOut;
        var st = _items.First(i => i.V == v);
        var op0 = _first ? 0 : 0.2 * st.Op;
        var x0 = S(dx);
        var y0 = S(dy);

        v.Opacity = op0;
        v.TranslationX = x0;
        v.TranslationY = y0;
        v.Scale = s0;
        v.Rotation = st.Rot + r0;

        Later(delay, () =>
        {
            var a = new Animation();
            a.Add(0, 1, new Animation(x => v.TranslationX = x, x0, 0, e));
            a.Add(0, 1, new Animation(x => v.TranslationY = x, y0, 0, e));
            a.Add(0, 1, new Animation(x => v.Scale = x, s0, 1, e));
            a.Add(0, 1, new Animation(x => v.Rotation = x, st.Rot + r0, st.Rot, e));
            a.Add(0, 0.5, new Animation(x => v.Opacity = x, op0, st.Op));
            a.Commit(v, "enter", 16, len, Easing.Linear);
        });
    }

    private void EnterText()
    {
        _title.Opacity = 0; _title.TranslationY = 18;
        _desc.Opacity = 0; _desc.TranslationY = 18;

        Later(120, () => Rise(_title));
        Later(240, () => Rise(_desc));
    }

    private static void Rise(VisualElement v)
    {
        var a = new Animation();
        a.Add(0, 1, new Animation(x => v.TranslationY = x, 18, 0, Easing.CubicOut));
        a.Add(0, 1, new Animation(x => v.Opacity = x, 0, 1, Easing.CubicOut));
        a.Commit(v, "text", 16, 520, Easing.Linear);
    }

    // Compteur : les mesures "montent" jusqu'à leur valeur
    private void Count(uint delay)
    {
        foreach (var (span, target) in _counters)
            span.Text = "0";

        Later(delay, () =>
        {
            var a = new Animation();
            foreach (var (span, target) in _counters)
            {
                var s = span;
                var t = target;
                a.Add(0, 1, new Animation(x => s.Text = ((int)Math.Round(x)).ToString(), 0, t, Easing.CubicOut));
            }
            a.Commit(_canvas!, "count", 16, 1100, Easing.Linear);
        });
    }

    // Flottement doux en boucle
    private void Bob(VisualElement v, double amp, uint len, double phase = 0)
    {
        var a = S(amp);
        new Animation(x => v.TranslationY = Math.Sin(x) * a, phase, phase + Math.PI * 2)
            .Commit(v, "idle", 16, len, Easing.Linear, null, () => true);
    }

    // Pulsation douce en boucle
    private void Pulse(VisualElement v, double amp, uint len)
    {
        new Animation(x => v.Scale = 1 + amp * (0.5 - 0.5 * Math.Cos(x)), 0, Math.PI * 2)
            .Commit(v, "idle2", 16, len, Easing.Linear, null, () => true);
    }

    // Battement de coeur
    private void Beat(VisualElement v)
    {
        new Animation(x => v.Scale = 1 + 0.22 * Math.Pow(Math.Max(0, Math.Sin(2 * x)), 6), 0, Math.PI * 2)
            .Commit(v, "idle2", 16, 1700, Easing.Linear, null, () => true);
    }
}