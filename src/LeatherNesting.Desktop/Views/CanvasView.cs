using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using LeatherNesting.Geometry;

namespace LeatherNesting.Desktop.Views;

/// <summary>CAD canvas: draws loops (mm, Y-up) into pixels (Y-down), with zoom/pan and click/drag selection.</summary>
public sealed class CanvasView : Control
{
    private static readonly IPen OuterPen = new Pen(Brushes.Navy, 1.5);
    private static readonly IPen HolePen = new Pen(Brushes.OrangeRed, 1.5);
    private static readonly IPen SelectedPen = new Pen(Brushes.DodgerBlue, 3);

    private IReadOnlyList<Loop2D> _loops = [];
    private double _scale = 10.0; // pixels per mm
    private Point _offset;
    private bool _fitPending = true;
    private Point _lastPointer;
    private Size _lastSize;
    private Point2D _pressModel = Point2D.Origin;
    private Point _pressPixel;
    private bool _pressedOnPiece;

    /// <summary>Loop currently highlighted (selected) on the canvas.</summary>
    public string? SelectedLoopId { get; set; }

    /// <summary>Invoked on a click (no drag) with the model-space point.</summary>
    public Action<Point2D>? OnClick { get; set; }

    /// <summary>Invoked on a drag release with the model-space delta.</summary>
    public Action<Point2D>? OnDrag { get; set; }

    /// <summary>Updates the loops to draw; re-fits the view unless <paramref name="refit"/> is false.</summary>
    public void SetData(IReadOnlyList<Loop2D>? loops, bool refit = true)
    {
        _loops = loops ?? [];
        _fitPending = refit;
        InvalidateVisual();
    }

    /// <summary>Converts a pixel point to model-space millimetres.</summary>
    public Point2D ToModel(Point pixel) => new((pixel.X - _offset.X) / _scale, (_offset.Y - pixel.Y) / _scale);

    public override void Render(DrawingContext context)
    {
        context.FillRectangle(Brushes.White, new Rect(Bounds.Size));

        if (_loops.Count == 0)
            return;

        if (_fitPending && Bounds.Width > 0 && Bounds.Height > 0)
        {
            FitToView();
            _fitPending = false;
        }

        foreach (var loop in _loops)
        {
            var pen = loop.StableId == SelectedLoopId ? SelectedPen : loop.Role == LoopRole.Outer ? OuterPen : HolePen;
            foreach (var segment in FlattenLoop(loop))
                context.DrawLine(pen, ToPixel(segment.Start), ToPixel(segment.End));
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = base.ArrangeOverride(finalSize);
        if (_lastSize != finalSize)
        {
            _lastSize = finalSize;
            _fitPending = true;
            InvalidateVisual();
        }
        return result;
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (_loops.Count == 0)
            return;
        var factor = e.Delta.Y > 0 ? 1.1 : 1 / 1.1;
        var cursor = e.GetPosition(this);
        var mmX = (cursor.X - _offset.X) / _scale;
        var mmY = (_offset.Y - cursor.Y) / _scale;
        _scale *= factor;
        _offset = new Point(cursor.X - mmX * _scale, cursor.Y + mmY * _scale);
        InvalidateVisual();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var position = e.GetPosition(this);
        _lastPointer = position;
        _pressPixel = position;
        _pressModel = ToModel(position);
        _pressedOnPiece = _loops.Any(l => l.ContainsPoint(_pressModel));
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;
        var position = e.GetPosition(this);
        if (_pressedOnPiece)
        {
            _lastPointer = position;
            return; // dragging a piece: resolve on release
        }
        _offset = new Point(_offset.X + (position.X - _lastPointer.X), _offset.Y + (position.Y - _lastPointer.Y));
        _lastPointer = position;
        InvalidateVisual();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (!_pressedOnPiece)
            return;
        var pixelDelta = e.GetPosition(this) - _pressPixel;
        if (pixelDelta.X * pixelDelta.X + pixelDelta.Y * pixelDelta.Y < 16)
            OnClick?.Invoke(_pressModel);
        else
            OnDrag?.Invoke(new Point2D(pixelDelta.X / _scale, -pixelDelta.Y / _scale));
        _pressedOnPiece = false;
    }

    private Point ToPixel(Point2D p) => new(_offset.X + p.X * _scale, _offset.Y - p.Y * _scale);

    private void FitToView()
    {
        var (minX, minY, maxX, maxY) = ComputeBounds();
        var width = maxX - minX;
        var height = maxY - minY;
        if (width <= 0 || height <= 0 || Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            _scale = 10.0;
            _offset = new Point(0, 0);
            return;
        }

        const double margin = 0.9;
        _scale = Math.Min(Bounds.Width / width, Bounds.Height / height) * margin;
        var cx = (minX + maxX) / 2;
        var cy = (minY + maxY) / 2;
        _offset = new Point(Bounds.Width / 2 - cx * _scale, Bounds.Height / 2 + cy * _scale);
    }

    private (double MinX, double MinY, double MaxX, double MaxY) ComputeBounds()
    {
        var minX = double.MaxValue; var minY = double.MaxValue;
        var maxX = double.MinValue; var maxY = double.MinValue;
        foreach (var loop in _loops)
        foreach (var segment in FlattenLoop(loop))
        {
            minX = Math.Min(minX, Math.Min(segment.Start.X, segment.End.X));
            minY = Math.Min(minY, Math.Min(segment.Start.Y, segment.End.Y));
            maxX = Math.Max(maxX, Math.Max(segment.Start.X, segment.End.X));
            maxY = Math.Max(maxY, Math.Max(segment.Start.Y, segment.End.Y));
        }
        return (minX, minY, maxX, maxY);
    }

    private static IReadOnlyList<LineSegment2D> FlattenLoop(Loop2D loop)
    {
        var segments = new List<LineSegment2D>();
        foreach (var curve in loop.Curves)
        {
            switch (curve)
            {
                case LineSegment2D line:
                    segments.Add(line);
                    break;
                case Polyline2D polyline:
                    for (var i = 0; i < polyline.Points.Count - 1; i++)
                        segments.Add(new LineSegment2D(polyline.Points[i], polyline.Points[i + 1]));
                    break;
                case CircularArc2D arc:
                    segments.AddRange(FlattenArc(arc));
                    break;
            }
        }
        return segments;
    }

    private static IReadOnlyList<LineSegment2D> FlattenArc(CircularArc2D arc)
    {
        var chordLength = arc.StartPoint.DistanceTo(arc.EndPoint);
        var steps = (int)Math.Max(2, Math.Ceiling(chordLength / 0.1));
        var segments = new List<LineSegment2D>();
        var previous = arc.StartPoint;
        for (var i = 1; i <= steps; i++)
        {
            var point = arc.PointAt((double)i / steps);
            segments.Add(new LineSegment2D(previous, point));
            previous = point;
        }
        return segments;
    }
}
