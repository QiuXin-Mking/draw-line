namespace LeatherNesting.Desktop.Modules.CadCanvas.Toolbar;

public enum CadToolCommandKey
{
    ExportToOrder,
    Select,
    Refit,
    DrawPolyline,
    DrawRectangle,
    DrawCircle,
    DrawLine,
    TextAnnotation,
    Dimension,
    EditNodeOrFillet,
    HolePattern,
    DrawSpline,
    Notch,
    SharpCornerContour,
    CloseContour,
    RoundContour,
    SmoothCurve,
    UvCurveDirection,
    SharpenCorner,
    EraseSegment,
    RegionOrdering,
    Transform,
    Undo,
    Redo,
    Cancel,
    Delete,
    Settings,
}

public enum CadToolGroup
{
    A,
    B,
    C,
    D,
    E,
}

public enum CadToolIconKey
{
    ExportToOrder,
    Select,
    Refit,
    DrawPolyline,
    DrawRectangle,
    DrawCircle,
    DrawLine,
    TextAnnotation,
    Dimension,
    EditNodeOrFillet,
    HolePattern,
    DrawSpline,
    Notch,
    SharpCornerContour,
    CloseContour,
    RoundContour,
    SmoothCurve,
    UvCurveDirection,
    SharpenCorner,
    EraseSegment,
    RegionOrdering,
    Transform,
    Undo,
    Redo,
    Cancel,
    Delete,
    Settings,
}

public enum CadToolConfidence
{
    Confirmed,
    High,
    Medium,
    Low,
}

[Flags]
public enum CadToolbarMode
{
    None = 0,
    CadEdit = 1,
    NestingReview = 2,
}

public enum CadToolImplementationState
{
    Implemented,
    Partial,
    Todo,
}

/// <summary>Immutable metadata for one stable CAD toolbar command.</summary>
public sealed record CadToolDefinition(
    int Order,
    string ControlId,
    CadToolCommandKey CommandKey,
    string Label,
    string Tooltip,
    CadToolGroup Group,
    CadToolIconKey IconKey,
    CadToolConfidence Confidence,
    CadToolbarMode SupportedModes,
    CadToolImplementationState ImplementationState,
    string? Shortcut);
