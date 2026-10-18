using SweetTailor.Core.Models;

namespace SweetTailor.App.Controls;

public partial class StepperView : ContentView
{
    public static readonly BindableProperty StepsProperty =
        BindableProperty.Create(nameof(Steps), typeof(List<OrderStep>), typeof(StepperView), null, propertyChanged: OnStepsChanged);

    public List<OrderStep> Steps
    {
        get => (List<OrderStep>)GetValue(StepsProperty);
        set => SetValue(StepsProperty, value);
    }

    public StepperView()
    {
        InitializeComponent();
    }

    private static void OnStepsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StepperView stepper && newValue is List<OrderStep> steps)
        {
            stepper.RenderSteps(steps);
        }
    }

    private void RenderSteps(List<OrderStep> steps)
    {
        StepsContainer.Children.Clear();
        if (steps == null || steps.Count == 0) return;

        for (int i = 0; i < steps.Count; i++)
        {
            var step = steps[i];
            var isLast = i == steps.Count - 1;

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star)
                },
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition(GridLength.Auto)
                },
                Padding = new Thickness(0, 0, 0, isLast ? 0 : 14)
            };

            // Left column: Dot and vertical connector line
            var leftStack = new Grid
            {
                WidthRequest = 24,
                HorizontalOptions = LayoutOptions.Center
            };

            // Connector line
            if (!isLast)
            {
                var connectorLine = new BoxView
                {
                    WidthRequest = 2,
                    Color = step.State == StepState.Done ? Color.FromArgb("#26306B") : Color.FromArgb("#E4E5F2"),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Fill,
                    Margin = new Thickness(0, 22, 0, 0)
                };
                leftStack.Children.Add(connectorLine);
            }

            // Dot
            var dotBorder = new Border
            {
                WidthRequest = 22,
                HeightRequest = 22,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(11) },
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start
            };

            if (step.State == StepState.Done)
            {
                dotBorder.BackgroundColor = Color.FromArgb("#26306B");
                dotBorder.Stroke = Color.FromArgb("#26306B");
                dotBorder.StrokeThickness = 2;
                dotBorder.Content = new Label
                {
                    Text = "✓",
                    TextColor = Colors.White,
                    FontSize = 11,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };
            }
            else if (step.State == StepState.Now)
            {
                dotBorder.BackgroundColor = Color.FromArgb("#FCE4EA");
                dotBorder.Stroke = Color.FromArgb("#E8607F");
                dotBorder.StrokeThickness = 2;
                dotBorder.Content = new BoxView
                {
                    WidthRequest = 8,
                    HeightRequest = 8,
                    CornerRadius = 4,
                    Color = Color.FromArgb("#E8607F"),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };
            }
            else // Todo
            {
                dotBorder.BackgroundColor = Colors.White;
                dotBorder.Stroke = Color.FromArgb("#E4E5F2");
                dotBorder.StrokeThickness = 2;
            }

            leftStack.Children.Add(dotBorder);
            Grid.SetColumn(leftStack, 0);
            grid.Children.Add(leftStack);

            // Right column: Title and Subtitle
            var contentStack = new VerticalStackLayout
            {
                Spacing = 2,
                Margin = new Thickness(12, 1, 0, 0)
            };

            var titleLabel = new Label
            {
                Text = step.Title,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = step.State == StepState.Todo ? Color.FromArgb("#9A9EBD") : Color.FromArgb("#14173A")
            };
            contentStack.Children.Add(titleLabel);

            if (!string.IsNullOrWhiteSpace(step.Subtitle))
            {
                var subLabel = new Label
                {
                    Text = step.Subtitle,
                    FontSize = 12,
                    TextColor = Color.FromArgb("#6B7094")
                };
                contentStack.Children.Add(subLabel);
            }

            Grid.SetColumn(contentStack, 1);
            grid.Children.Add(contentStack);

            StepsContainer.Children.Add(grid);
        }
    }
}
