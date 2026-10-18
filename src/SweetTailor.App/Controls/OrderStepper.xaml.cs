using SweetTailor.Core.Models;

namespace SweetTailor.App.Controls;

public partial class OrderStepper : ContentView
{
    public static readonly BindableProperty CurrentStatusProperty =
        BindableProperty.Create(nameof(CurrentStatus), typeof(OrderStatus), typeof(OrderStepper), OrderStatus.Recue, propertyChanged: (b, o, n) => ((OrderStepper)b).RenderSteps((OrderStatus)n));

    public OrderStatus CurrentStatus
    {
        get => (OrderStatus)GetValue(CurrentStatusProperty);
        set => SetValue(CurrentStatusProperty, value);
    }

    private static readonly (OrderStatus Status, string Name, string Desc)[] AllSteps = new[]
    {
        (OrderStatus.Recue, "Commande reçue", "Mesures et acompte validés"),
        (OrderStatus.Couture, "Coupe et couture", "Patronnage et confection en atelier"),
        (OrderStatus.Essayage, "Essayage client", "Ajustements et retouches finales"),
        (OrderStatus.Prete, "Prête à retirer", "Vêtement repassé et emballé"),
        (OrderStatus.Livree, "Livrée", "Solde réglé et commande clôturée")
    };

    public OrderStepper()
    {
        InitializeComponent();
        RenderSteps(CurrentStatus);
    }

    public void RenderSteps(OrderStatus current)
    {
        StepsContainer.Children.Clear();
        int currentIndex = (int)current;

        for (int i = 0; i < AllSteps.Length; i++)
        {
            var step = AllSteps[i];
            int stepIndex = (int)step.Status;
            bool isDone = stepIndex < currentIndex;
            bool isNow = stepIndex == currentIndex;
            bool isLast = i == AllSteps.Length - 1;

            var rowGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star)
                },
                Padding = new Thickness(0, 0, 0, isLast ? 0 : 16)
            };

            // Timeline node with vertical line
            var leftStack = new Grid { WidthRequest = 28 };
            if (!isLast)
            {
                var line = new BoxView
                {
                    WidthRequest = 2,
                    Color = isDone ? Color.FromArgb("#26306B") : Color.FromArgb("#E8E9F3"),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Fill,
                    Margin = new Thickness(0, 14, 0, -16)
                };
                leftStack.Children.Add(line);
            }

            var nodeBorder = new Border
            {
                WidthRequest = 24,
                HeightRequest = 24,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(12) },
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start
            };

            if (isDone)
            {
                nodeBorder.BackgroundColor = Color.FromArgb("#26306B");
                nodeBorder.StrokeThickness = 0;
                nodeBorder.Content = new Label { Text = "✓", TextColor = Colors.White, FontSize = 12, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
            }
            else if (isNow)
            {
                nodeBorder.BackgroundColor = Colors.White;
                nodeBorder.Stroke = Color.FromArgb("#E8607F");
                nodeBorder.StrokeThickness = 3;
                nodeBorder.Content = new BoxView { WidthRequest = 8, HeightRequest = 8, CornerRadius = 4, Color = Color.FromArgb("#E8607F"), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
            }
            else
            {
                nodeBorder.BackgroundColor = Colors.White;
                nodeBorder.Stroke = Color.FromArgb("#E8E9F3");
                nodeBorder.StrokeThickness = 2;
            }
            leftStack.Children.Add(nodeBorder);
            Grid.SetColumn(leftStack, 0);
            rowGrid.Children.Add(leftStack);

            // Right text stack
            var textStack = new VerticalStackLayout { Spacing = 2, Margin = new Thickness(12, 0, 0, 0), VerticalOptions = LayoutOptions.Start };
            var titleLabel = new Label
            {
                Text = step.Name,
                FontSize = 14.5,
                FontAttributes = FontAttributes.Bold,
                TextColor = isNow ? Color.FromArgb("#26306B") : (isDone ? Color.FromArgb("#14173A") : Color.FromArgb("#6B7094"))
            };
            var descLabel = new Label
            {
                Text = step.Desc,
                FontSize = 12,
                TextColor = Color.FromArgb("#6B7094")
            };
            textStack.Children.Add(titleLabel);
            textStack.Children.Add(descLabel);

            Grid.SetColumn(textStack, 1);
            rowGrid.Children.Add(textStack);

            StepsContainer.Children.Add(rowGrid);
        }
    }
}
