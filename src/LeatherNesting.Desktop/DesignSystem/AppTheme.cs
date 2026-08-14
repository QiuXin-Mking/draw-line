using Avalonia.Media;

namespace LeatherNesting.Desktop.DesignSystem;

/// <summary>Screenshot-evidence palette for the fixed classic workstation.</summary>
public static class AppTheme
{
    public const double TitleBarHeight = 26;
    public const double MenuBarHeight = 30;
    public const double ToolbarHeight = 76;
    public const double ToolbarButtonWidth = 82;
    public const double ToolbarIconSize = 34;
    public const double ClassicHeaderHeight = 23;
    public const double StatusBarHeight = 24;

    // Application chrome sampled from the image-27 workstation evidence.
    public static IBrush ApplicationTitle { get; } = Brush(0x1B, 0x30, 0x30);
    public static IBrush MenuSurface { get; } = Brush(0xFE, 0xFE, 0xFF);
    public static IBrush ToolbarSurface { get; } = Brush(0xEE, 0xF0, 0xF2);
    public static IBrush PanelSurface { get; } = Brush(0xFF, 0xFF, 0xFF);
    public static IBrush HeaderSurface { get; } = Brush(0xD9, 0xD9, 0xD9);
    public static IBrush StatusSurface { get; } = Brush(0xF0, 0xF0, 0xF0);
    public static IBrush ClassicBorderNeutral { get; } = Brush(0x80, 0x80, 0x80);
    public static IBrush PrimaryText { get; } = Brush(0x20, 0x20, 0x20);
    public static IBrush TitleText { get; } = Brush(0xFF, 0xFF, 0xFF);
    public static IBrush MutedText { get; } = Brush(0x68, 0x68, 0x68);
    public static IBrush DisabledText { get; } = Brush(0x99, 0x99, 0x99);
    public static IBrush DisabledSurface { get; } = Brush(0xF0, 0xF0, 0xF0);

    // Interaction roles remain distinct from workstation and geometry semantics.
    public static IBrush ToolbarHoverSurface { get; } = Brush(0xDC, 0xEA, 0xEC);
    public static IBrush ClassicFocus { get; } = Brush(0x2B, 0x7D, 0x87);
    public static IBrush SelectionSurface { get; } = Brush(0xB8, 0xE3, 0xF3);
    public static IBrush WarningText { get; } = Brush(0xA8, 0x63, 0x16);
    public static IBrush DangerText { get; } = Brush(0xB5, 0x31, 0x31);

    // Workstation roles.
    public static IBrush ToolbarIconTeal { get; } = Brush(0x46, 0x95, 0x89);
    public static IBrush PieceCardCyan { get; } = Brush(0x98, 0xD4, 0xEF);
    public static IBrush ProgressCyan { get; } = Brush(0x51, 0xB2, 0xC4);
    public static IBrush CanvasBlack { get; } = Brush(0x00, 0x00, 0x00);
    public static IBrush RulerChrome { get; } = Brush(0x32, 0x32, 0x32);
    public static IBrush RulerTick { get; } = Brush(0xD8, 0xD8, 0xD8);

    // Canvas geometry roles must never be reused as application chrome.
    public static IBrush MaterialBoundary { get; } = Brush(0xFF, 0x00, 0x00);
    public static IBrush GeometryOuterContour { get; } = Brush(0xFF, 0xFF, 0xFF);
    public static IBrush GeometryInternalLine { get; } = Brush(0x32, 0xCD, 0x32);
    public static IBrush GeometrySelectionFill { get; } = Brush(0x12, 0x68, 0x70, 0x99);

    // Compatibility aliases for legacy modules. Clone surfaces use the semantic roles above.
    public static IBrush WorkspaceBackground => RulerChrome;
    public static IBrush NavBackground => ApplicationTitle;
    public static IBrush NavForeground => RulerTick;
    public static IBrush Accent => ClassicFocus;
    public static IBrush Surface => PanelSurface;
    public static IBrush SurfaceBorder => ClassicBorderNeutral;
    public static IBrush TodoAmber => WarningText;
    public static IBrush TextPrimary => PrimaryText;
    public static IBrush TextMuted => MutedText;
    public static IBrush MenuBackground => MenuSurface;
    public static IBrush ToolbarBackground => ToolbarSurface;
    public static IBrush ToolbarHover => ToolbarHoverSurface;
    public static IBrush ToolbarBorder => ClassicBorderNeutral;
    public static IBrush ToolbarIcon => ToolbarIconTeal;
    public static IBrush ToolbarAccent => ProgressCyan;
    public static IBrush ToolbarDanger => DangerText;
    public static IBrush ToolbarWarning => WarningText;
    public static IBrush ClassicTitleBackground => ApplicationTitle;
    public static IBrush ClassicPanelBackground => PanelSurface;
    public static IBrush ClassicHeaderBackground => HeaderSurface;
    public static IBrush ClassicBorder => ClassicBorderNeutral;
    public static IBrush CadCanvasBackground => CanvasBlack;
    public static IBrush RulerBackground => RulerChrome;
    public static IBrush RulerForeground => RulerTick;
    public static IBrush DemoPanelBackground => PieceCardCyan;

    private static IBrush Brush(byte red, byte green, byte blue, byte alpha = 0xFF) =>
        new SolidColorBrush(Color.FromArgb(alpha, red, green, blue));
}
