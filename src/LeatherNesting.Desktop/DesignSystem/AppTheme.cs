using Avalonia.Media;

namespace LeatherNesting.Desktop.DesignSystem;

/// <summary>Central colour/brush palette for the demo shell. Dark CAD workspace + light admin accents.</summary>
public static class AppTheme
{
    public static IBrush WorkspaceBackground { get; } = new SolidColorBrush(Color.FromRgb(0x1E, 0x24, 0x2B));
    public static IBrush NavBackground { get; } = new SolidColorBrush(Color.FromRgb(0x2A, 0x33, 0x3D));
    public static IBrush NavForeground { get; } = new SolidColorBrush(Color.FromRgb(0xE6, 0xEA, 0xEF));
    public static IBrush Accent { get; } = new SolidColorBrush(Color.FromRgb(0x4F, 0x9D, 0xF2));
    public static IBrush Surface { get; } = new SolidColorBrush(Color.FromRgb(0xF5, 0xF7, 0xFA));
    public static IBrush SurfaceBorder { get; } = new SolidColorBrush(Color.FromRgb(0xD5, 0xDC, 0xE3));
    public static IBrush TodoAmber { get; } = new SolidColorBrush(Color.FromRgb(0xC8, 0x7A, 0x1E));
    public static IBrush TextPrimary { get; } = new SolidColorBrush(Color.FromRgb(0x20, 0x28, 0x32));
    public static IBrush TextMuted { get; } = new SolidColorBrush(Color.FromRgb(0x6B, 0x76, 0x83));
}
