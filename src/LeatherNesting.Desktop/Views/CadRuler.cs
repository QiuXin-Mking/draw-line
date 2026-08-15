using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Views;

/// <summary>Orientation of a CAD canvas ruler.</summary>
public enum CadRulerOrientation { Horizontal, Vertical }

/// <summary>Self-drawn ruler that repaints its ticks from the canvas view state.
/// Background and ticks use the shared <see cref="AppTheme"/> ruler brushes; the tick
/// step is chosen adaptively so labels stay readable across zoom levels.</summary>
public sealed class CadRuler : Control
{
    private static readonly double[] StepCandidates = [1, 2, 5, 10, 20, 50, 100, 200, 500, 1000, 2000, 5000];

    public CadRuler(CanvasView source, CadRulerOrientation orientation)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Orientation = orientation;
        Source.ViewChanged += (_, _) => InvalidateVisual();
    }

    public CanvasView Source { get; }

    public CadRulerOrientation Orientation { get; }

    /// <summary>Shared semantic brush used for the ruler surface (self-drawn; see Render).</summary>
    public IBrush Surface => AppTheme.RulerChrome;

    /// <summary>Shared semantic brush used for ticks and labels (self-drawn; see Render).</summary>
    public IBrush TickBrush => AppTheme.RulerTick;

    /// <summary>Current adaptive tick spacing in model millimetres (readable on screen).</summary>
    public double TickStepMm => PickStep(Source.ViewScale);

    /// <summary>Converts a model-space coordinate to this ruler's pixel axis.</summary>
    public double ModelToPixel(double model)
    {
        return Orientation == CadRulerOrientation.Horizontal
            ? (model - Source.ViewOriginModel.X) * Source.ViewScale
            : (Source.ViewOriginModel.Y - model) * Source.ViewScale;
    }

    /// <summary>Model coordinate at a pixel position on this ruler's axis.</summary>
    public double PixelToModel(double pixel)
    {
        return Orientation == CadRulerOrientation.Horizontal
            ? Source.ViewOriginModel.X + pixel / Source.ViewScale
            : Source.ViewOriginModel.Y - pixel / Source.ViewScale;
    }

    public override void Render(DrawingContext context)
    {
        context.FillRectangle(AppTheme.RulerChrome, new Rect(Bounds.Size));
        if (Source.ViewScale <= 0)
            return;

        var rangePx = Orientation == CadRulerOrientation.Horizontal ? Bounds.Width : Bounds.Height;
        var minPixel = 0.0;
        var maxPixel = rangePx;

        var firstModel = PixelToModel(minPixel);
        var lastModel = PixelToModel(maxPixel);
        var startModel = Math.Min(firstModel, lastModel);
        var endModel = Math.Max(firstModel, lastModel);

        var step = TickStepMm;
        var firstTick = Math.Floor(startModel / step) * step;
        var pen = new Pen(AppTheme.RulerTick, 1);
        for (var m = firstTick; m <= endModel; m += step)
        {
            var px = ModelToPixel(m);
            if (px < minPixel || px > maxPixel)
                continue;
            var isMajor = Math.Abs(m / step) % 5 < 0.001;
            DrawTick(context, pen, px, isMajor, Math.Round(m, 2));
        }
    }

    private void DrawTick(DrawingContext context, Pen pen, double pixel, bool isMajor, double label)
    {
        var tickLength = isMajor ? 10 : 5;
        if (Orientation == CadRulerOrientation.Horizontal)
        {
            context.DrawLine(pen, new Point(pixel, Bounds.Height - tickLength), new Point(pixel, Bounds.Height));
            if (isMajor)
                DrawText(context, label.ToString("0"), pixel + 3, Bounds.Height - 13);
        }
        else
        {
            context.DrawLine(pen, new Point(Bounds.Width - tickLength, pixel), new Point(Bounds.Width, pixel));
            if (isMajor)
                DrawText(context, label.ToString("0"), 3, pixel - 7);
        }
    }

    private static void DrawText(DrawingContext context, string text, double x, double y)
    {
        var formatted = new FormattedText(
            text,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            9,
            AppTheme.RulerTick);
        context.DrawText(formatted, new Point(x, y));
    }

    private static double PickStep(double scale)
    {
        var minPxPerTick = 45.0;
        foreach (var candidate in StepCandidates)
        {
            if (candidate * scale >= minPxPerTick)
                return candidate;
        }

        return StepCandidates[^1];
    }
}
