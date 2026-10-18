namespace SweetTailor.App.Controls;

public partial class TapeMeasure : ContentView
{
    public static readonly BindableProperty IsSmallProperty =
        BindableProperty.Create(nameof(IsSmall), typeof(bool), typeof(TapeMeasure), false, propertyChanged: (b, o, n) => ((TapeMeasure)b).UpdateHeight());

    public bool IsSmall
    {
        get => (bool)GetValue(IsSmallProperty);
        set => SetValue(IsSmallProperty, value);
    }

    public TapeMeasure()
    {
        InitializeComponent();
        CanvasView.Drawable = new TapeMeasureDrawable(this);
        UpdateHeight();
    }

    private void UpdateHeight()
    {
        HeightRequest = IsSmall ? 16 : 24;
    }

    private class TapeMeasureDrawable : IDrawable
    {
        private readonly TapeMeasure _owner;

        public TapeMeasureDrawable(TapeMeasure owner)
        {
            _owner = owner;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = Color.FromArgb("#F4C95D");
            canvas.FillRectangle(dirtyRect);

            canvas.StrokeColor = Color.FromArgb("#14173A");
            var isSmall = _owner.IsSmall;
            var height = dirtyRect.Height;

            int step = 8;
            for (float x = 0; x < dirtyRect.Width; x += step)
            {
                bool isMajor = ((int)x % 40) == 0;
                float markHeight = isMajor ? (isSmall ? 10 : 14) : (isSmall ? 5 : 7);
                canvas.StrokeSize = isMajor ? 1.5f : 1f;
                canvas.DrawLine(x, height - markHeight, x, height);
            }
        }
    }
}
