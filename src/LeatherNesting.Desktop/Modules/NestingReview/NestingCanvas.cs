using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using LeatherNesting.Desktop.DesignSystem;

namespace LeatherNesting.Desktop.Modules.NestingReview;

/// <summary>Read-only material overview. Geometry is illustrative and does not perform nesting validation.</summary>
public sealed class NestingCanvas : Control
{
    public NestingMaterialPageDemo? Material { get; set; }
    public string? SelectedInstanceId { get; set; }
    public bool ShowCollisionOverlay { get; set; }

    public event EventHandler<string>? InstanceSelected;

    protected override void OnPointerPressed(Avalonia.Input.PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (Material is null)
            return;

        var point = e.GetPosition(this);
        var transform = CreateTransform(Material);
        var selected = Material.Instances.LastOrDefault(instance => InstanceRect(instance, transform).Contains(point));
        if (selected is not null)
            InstanceSelected?.Invoke(this, selected.Id);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.FillRectangle(AppTheme.WorkspaceBackground, Bounds);
        if (Material is null || Bounds.Width < 1 || Bounds.Height < 1)
            return;

        var transform = CreateTransform(Material);
        var materialRect = new Rect(transform.OffsetX, transform.OffsetY,
            Material.WidthMillimetres * transform.Scale, Material.LengthMillimetres * transform.Scale);
        context.FillRectangle(new SolidColorBrush(Color.FromRgb(0x3B, 0x43, 0x49)), materialRect);
        context.DrawRectangle(new Pen(AppTheme.NavForeground, 1), materialRect);

        foreach (var instance in Material.Instances)
        {
            var rect = InstanceRect(instance, transform);
            var isSelected = StringComparer.Ordinal.Equals(instance.Id, SelectedInstanceId);
            context.FillRectangle(isSelected ? AppTheme.Accent : new SolidColorBrush(Color.FromRgb(0x54, 0x8A, 0x68)), rect, 5);
            context.DrawRectangle(new Pen(isSelected ? Brushes.White : AppTheme.NavForeground, isSelected ? 3 : 1), rect, 5);
            var label = new FormattedText(instance.PieceCode, System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, Typeface.Default, 12, Brushes.White);
            context.DrawText(label, new Point(rect.X + 6, rect.Y + 6));
        }

        if (ShowCollisionOverlay && Material.Instances.Count >= 2)
        {
            var target = InstanceRect(Material.Instances[1], transform);
            var overlay = target.Translate(new Vector(-target.Width * .35, target.Height * .2));
            context.FillRectangle(new SolidColorBrush(Color.FromArgb(0x88, 0xE0, 0x45, 0x45)), overlay, 5);
            context.DrawRectangle(new Pen(Brushes.OrangeRed, 3), overlay, 5);
        }
    }

    private CanvasTransform CreateTransform(NestingMaterialPageDemo material)
    {
        const double padding = 22;
        var scale = Math.Min((Math.Max(Bounds.Width, 80) - padding * 2) / material.WidthMillimetres,
            (Math.Max(Bounds.Height, 80) - padding * 2) / material.LengthMillimetres);
        scale = Math.Max(scale, 0.02);
        return new CanvasTransform(scale, padding, padding);
    }

    private static Rect InstanceRect(NestingInstanceDemo instance, CanvasTransform transform) =>
        new(transform.OffsetX + instance.X * transform.Scale,
            transform.OffsetY + instance.Y * transform.Scale,
            instance.Width * transform.Scale,
            instance.Height * transform.Scale);

    private sealed record CanvasTransform(double Scale, double OffsetX, double OffsetY);
}
