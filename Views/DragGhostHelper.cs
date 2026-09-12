using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace SourceConfigMaker.Views;

internal sealed class DragGhost : IDisposable
{
    private readonly Border _ghost;
    private readonly AdornerLayer _layer;

    public DragGhost(TopLevel topLevel, string text, Point initialPosition)
    {
        if (topLevel == null) throw new ArgumentNullException(nameof(topLevel));

        _layer = AdornerLayer.GetAdornerLayer(topLevel)
                 ?? throw new InvalidOperationException("AdornerLayer не найден.");

        _ghost = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#3A7BFF")),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10, 6),
            IsHitTestVisible = false,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Child = new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                FontSize = 12,
                FontWeight = FontWeight.SemiBold
            }
        };

        AdornerLayer.SetAdornedElement(_ghost, topLevel);
        _layer.Children.Add(_ghost);

        Update(initialPosition);
    }

    public void Update(Point position)
    {
        _ghost.RenderTransform = new TranslateTransform(position.X + 14, position.Y + 14);
    }

    public void Dispose()
    {
        _layer.Children.Remove(_ghost);
    }
}