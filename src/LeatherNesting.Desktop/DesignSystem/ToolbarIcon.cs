using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace LeatherNesting.Desktop.DesignSystem;

public enum ToolbarIconKey
{
    NewLayout,
    OrderManagement,
    CadTools,
    StartNesting,
    StopNesting,
    CancelNesting,
    ZoomExtent,
    SettingsWindow,
    EqualWidthStrip,
    SendCut,
}

/// <summary>A self-contained vector icon whose shapes are independent of installed fonts and bitmaps.</summary>
public sealed class ToolbarIcon : Viewbox
{
    internal ToolbarIcon(ToolbarIconKey key, IReadOnlyList<Shape> shapes)
    {
        Key = key;
        Shapes = shapes;
        Width = AppTheme.ToolbarIconSize;
        Height = AppTheme.ToolbarIconSize;
        Stretch = Stretch.Uniform;

        var canvas = new Canvas { Width = 32, Height = 32 };
        foreach (var shape in shapes)
            canvas.Children.Add(shape);
        Child = canvas;
    }

    public ToolbarIconKey Key { get; }

    public IReadOnlyList<Shape> Shapes { get; }
}

public static class ToolbarIconFactory
{
    public static ToolbarIcon Create(ToolbarIconKey key) => key switch
    {
        ToolbarIconKey.NewLayout => Icon(key,
            Box(6, 4, 20, 24), Line(16, 13, 16, 24, AppTheme.ToolbarAccent), Line(10, 19, 22, 19, AppTheme.ToolbarAccent)),
        ToolbarIconKey.OrderManagement => Icon(key,
            Box(8, 6, 16, 22), Box(12, 3, 8, 6), Line(12, 14, 21, 14), Line(12, 19, 21, 19), Line(12, 24, 18, 24)),
        ToolbarIconKey.CadTools => Icon(key,
            Line(5, 26, 18, 6), Line(18, 6, 27, 26), Line(5, 26, 27, 26), Line(9, 21, 24, 21), Circle(13, 12, 6, AppTheme.ToolbarAccent)),
        ToolbarIconKey.StartNesting => Icon(key,
            Box(5, 6, 22, 20), Line(10, 10, 10, 22), Line(10, 16, 18, 11), Line(18, 11, 18, 21), Line(18, 21, 25, 16)),
        ToolbarIconKey.StopNesting => Icon(key,
            Box(5, 6, 22, 20, AppTheme.ToolbarDanger), FilledBox(11, 11, 10, 10, AppTheme.ToolbarDanger)),
        ToolbarIconKey.CancelNesting => Icon(key,
            Circle(4, 4, 24, AppTheme.ToolbarWarning), Line(8, 8, 24, 24, AppTheme.ToolbarDanger, 3), Line(24, 8, 8, 24, AppTheme.ToolbarDanger, 3)),
        ToolbarIconKey.ZoomExtent => Icon(key,
            Line(5, 12, 5, 5), Line(5, 5, 12, 5), Line(20, 5, 27, 5), Line(27, 5, 27, 12),
            Line(27, 20, 27, 27), Line(27, 27, 20, 27), Line(12, 27, 5, 27), Line(5, 27, 5, 20), Circle(11, 10, 10)),
        ToolbarIconKey.SettingsWindow => Icon(key,
            Box(4, 6, 24, 19), Line(4, 11, 28, 11), Circle(8, 8, 2, AppTheme.ToolbarAccent), Line(10, 16, 22, 16), Line(10, 21, 18, 21)),
        ToolbarIconKey.EqualWidthStrip => Icon(key,
            Box(5, 8, 22, 6), Box(5, 19, 22, 6), Line(9, 5, 9, 28, AppTheme.ToolbarAccent), Line(23, 5, 23, 28, AppTheme.ToolbarAccent)),
        ToolbarIconKey.SendCut => Icon(key,
            Line(4, 8, 16, 16), Line(16, 16, 4, 24), Line(4, 24, 4, 8),
            Line(15, 9, 28, 5), Line(28, 5, 24, 27), Line(24, 27, 15, 19), Line(10, 16, 25, 16, AppTheme.ToolbarWarning, 3)),
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, null),
    };

    private static ToolbarIcon Icon(ToolbarIconKey key, params Shape[] shapes) => new(key, shapes);

    private static Line Line(double x1, double y1, double x2, double y2, IBrush? brush = null, double thickness = 2) => new()
    {
        StartPoint = new Point(x1, y1),
        EndPoint = new Point(x2, y2),
        Stroke = brush ?? AppTheme.ToolbarIcon,
        StrokeThickness = thickness,
        StrokeLineCap = PenLineCap.Round,
    };

    private static Rectangle Box(double left, double top, double width, double height, IBrush? brush = null)
    {
        var shape = new Rectangle
        {
            Width = width,
            Height = height,
            Stroke = brush ?? AppTheme.ToolbarIcon,
            StrokeThickness = 2,
            Fill = Brushes.Transparent,
        };
        Canvas.SetLeft(shape, left);
        Canvas.SetTop(shape, top);
        return shape;
    }

    private static Rectangle FilledBox(double left, double top, double width, double height, IBrush brush)
    {
        var shape = new Rectangle { Width = width, Height = height, Fill = brush };
        Canvas.SetLeft(shape, left);
        Canvas.SetTop(shape, top);
        return shape;
    }

    private static Ellipse Circle(double left, double top, double diameter, IBrush? brush = null)
    {
        var shape = new Ellipse
        {
            Width = diameter,
            Height = diameter,
            Stroke = brush ?? AppTheme.ToolbarIcon,
            StrokeThickness = 2,
            Fill = Brushes.Transparent,
        };
        Canvas.SetLeft(shape, left);
        Canvas.SetTop(shape, top);
        return shape;
    }
}
