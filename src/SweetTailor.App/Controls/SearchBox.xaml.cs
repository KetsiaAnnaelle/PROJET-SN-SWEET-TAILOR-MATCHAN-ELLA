using System.Windows.Input;

namespace SweetTailor.App.Controls;

public partial class SearchBox : ContentView
{
    private CancellationTokenSource? _searchDebounce;

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(SearchBox), string.Empty, BindingMode.TwoWay, propertyChanged: (b, o, n) => ((SearchBox)b).OnTextChanged((string)n));

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(SearchBox), "Rechercher…", propertyChanged: (b, o, n) => ((SearchBox)b).SearchInput.Placeholder = (string)n);

    public static readonly BindableProperty SearchCommandProperty =
        BindableProperty.Create(nameof(SearchCommand), typeof(ICommand), typeof(SearchBox));

    public static readonly BindableProperty SearchCommandParameterProperty =
        BindableProperty.Create(nameof(SearchCommandParameter), typeof(object), typeof(SearchBox));

    public static readonly BindableProperty FocusedCommandProperty =
        BindableProperty.Create(nameof(FocusedCommand), typeof(ICommand), typeof(SearchBox));

    public static readonly BindableProperty ClearCommandProperty =
        BindableProperty.Create(nameof(ClearCommand), typeof(ICommand), typeof(SearchBox));

    public event EventHandler<string>? TextChanged;
    public event EventHandler? Tapped;

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public ICommand? SearchCommand
    {
        get => (ICommand?)GetValue(SearchCommandProperty);
        set => SetValue(SearchCommandProperty, value);
    }

    public object? SearchCommandParameter
    {
        get => GetValue(SearchCommandParameterProperty);
        set => SetValue(SearchCommandParameterProperty, value);
    }

    public ICommand? FocusedCommand
    {
        get => (ICommand?)GetValue(FocusedCommandProperty);
        set => SetValue(FocusedCommandProperty, value);
    }

    public ICommand? ClearCommand
    {
        get => (ICommand?)GetValue(ClearCommandProperty);
        set => SetValue(ClearCommandProperty, value);
    }

    public SearchBox()
    {
        InitializeComponent();

        SearchInput.TextChanged += OnInputTextChanged;

        SearchInput.Focused += (s, e) =>
        {
            UpdateFocusState(true);
            ExecuteCommand(FocusedCommand, SearchCommandParameter);
        };
        SearchInput.Unfocused += (s, e) => UpdateFocusState(false);
        SearchInput.Completed += (s, e) =>
        {
            _searchDebounce?.Cancel();
            ExecuteCommand(SearchCommand, SearchCommandParameter);
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => Tapped?.Invoke(this, EventArgs.Empty);
        GestureRecognizers.Add(tap);

        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(IsEnabled))
            {
                UpdateDisabledState();
            }
        };

        UpdateDisabledState();
    }

    public new void Focus()
    {
        SearchInput.Focus();
    }

    private void OnTextChanged(string text)
    {
        SearchInput.Text = text;
        ClearButton.IsVisible = !string.IsNullOrEmpty(text);
    }

    private void OnClearClicked(object? sender, EventArgs e)
    {
        SearchInput.Text = string.Empty;
        Text = string.Empty;
        ClearButton.IsVisible = false;
        ExecuteCommand(ClearCommand, null);
        SearchInput.Focus();
    }

    private async void OnInputTextChanged(object? sender, TextChangedEventArgs e)
    {
        Text = e.NewTextValue;
        ClearButton.IsVisible = !string.IsNullOrEmpty(e.NewTextValue);
        TextChanged?.Invoke(this, e.NewTextValue ?? string.Empty);

        _searchDebounce?.Cancel();
        _searchDebounce?.Dispose();
        var debounce = new CancellationTokenSource();
        _searchDebounce = debounce;
        try
        {
            await Task.Delay(300, debounce.Token);
            ExecuteCommand(SearchCommand, SearchCommandParameter);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static void ExecuteCommand(ICommand? command, object? parameter)
    {
        if (command?.CanExecute(parameter) == true)
        {
            command.Execute(parameter);
        }
    }

    private void UpdateFocusState(bool focused)
    {
        if (!IsEnabled) return;

        if (focused)
        {
            SearchBorder.Stroke = Color.FromArgb("#26306B");
            SearchBorder.StrokeThickness = 1.5;
        }
        else
        {
            SearchBorder.Stroke = Color.FromArgb("#E8E9F3");
            SearchBorder.StrokeThickness = 1;
        }
    }

    private void UpdateDisabledState()
    {
        if (!IsEnabled)
        {
            SearchBorder.BackgroundColor = Color.FromArgb("#EEEFF6");
            SearchBorder.Stroke = Color.FromArgb("#E2E3EF");
            SearchInput.TextColor = Color.FromArgb("#A6AAC8");
            SearchInput.IsEnabled = false;
            SearchIcon.Fill = Color.FromArgb("#A6AAC8");
        }
        else
        {
            SearchBorder.BackgroundColor = Colors.White;
            SearchBorder.Stroke = Color.FromArgb("#E8E9F3");
            SearchInput.TextColor = Color.FromArgb("#14173A");
            SearchInput.IsEnabled = true;
            SearchIcon.Fill = Color.FromArgb("#6B7094");
        }
    }
}
