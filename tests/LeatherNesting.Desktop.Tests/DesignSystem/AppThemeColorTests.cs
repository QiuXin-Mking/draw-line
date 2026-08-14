using Avalonia.Media;
using LeatherNesting.Desktop.DesignSystem;
using Xunit;

namespace LeatherNesting.Desktop.Tests.DesignSystem;

public sealed class AppThemeColorTests
{
    [Theory]
    [InlineData(nameof(AppTheme.ApplicationTitle), 0x1B, 0x30, 0x30)]
    [InlineData(nameof(AppTheme.TitleText), 0xFF, 0xFF, 0xFF)]
    [InlineData(nameof(AppTheme.MenuSurface), 0xFE, 0xFE, 0xFF)]
    [InlineData(nameof(AppTheme.ToolbarSurface), 0xEE, 0xF0, 0xF2)]
    [InlineData(nameof(AppTheme.PanelSurface), 0xFF, 0xFF, 0xFF)]
    [InlineData(nameof(AppTheme.HeaderSurface), 0xD9, 0xD9, 0xD9)]
    [InlineData(nameof(AppTheme.StatusSurface), 0xF0, 0xF0, 0xF0)]
    [InlineData(nameof(AppTheme.ClassicBorderNeutral), 0x80, 0x80, 0x80)]
    [InlineData(nameof(AppTheme.ToolbarIconTeal), 0x46, 0x95, 0x89)]
    [InlineData(nameof(AppTheme.PieceCardCyan), 0x98, 0xD4, 0xEF)]
    [InlineData(nameof(AppTheme.ProgressCyan), 0x51, 0xB2, 0xC4)]
    [InlineData(nameof(AppTheme.CanvasBlack), 0x00, 0x00, 0x00)]
    [InlineData(nameof(AppTheme.RulerChrome), 0x32, 0x32, 0x32)]
    public void Evidence_palette_keeps_sampled_workstation_colors(string propertyName, byte red, byte green, byte blue)
    {
        var property = typeof(AppTheme).GetProperty(propertyName);
        Assert.NotNull(property);
        var brush = Assert.IsType<SolidColorBrush>(property.GetValue(null));

        Assert.Equal(Color.FromRgb(red, green, blue), brush.Color);
    }

    [Fact]
    public void Neutral_chrome_surfaces_have_no_green_or_warm_temperature_bias()
    {
        AssertNeutral(AppTheme.ClassicPanelBackground);
        AssertNeutral(AppTheme.ClassicHeaderBackground);
        AssertNeutral(AppTheme.ClassicBorder);
        AssertNeutral(AppTheme.RulerBackground);
    }

    [Fact]
    public void Interaction_workstation_and_geometry_roles_do_not_collapse_into_one_accent()
    {
        Assert.NotSame(AppTheme.ClassicFocus, AppTheme.ToolbarIconTeal);
        Assert.NotSame(AppTheme.ClassicFocus, AppTheme.ProgressCyan);
        Assert.NotSame(AppTheme.SelectionSurface, AppTheme.PieceCardCyan);
        Assert.NotSame(AppTheme.MaterialBoundary, AppTheme.DangerText);
        Assert.NotSame(AppTheme.GeometryOuterContour, AppTheme.PanelSurface);
        Assert.NotSame(AppTheme.GeometryInternalLine, AppTheme.ToolbarIconTeal);
        Assert.NotSame(AppTheme.GeometrySelectionFill, AppTheme.SelectionSurface);
    }

    [Fact]
    public void Legacy_aliases_preserve_the_same_brush_instances()
    {
        Assert.Same(AppTheme.ApplicationTitle, AppTheme.ClassicTitleBackground);
        Assert.Same(AppTheme.MenuSurface, AppTheme.MenuBackground);
        Assert.Same(AppTheme.ToolbarSurface, AppTheme.ToolbarBackground);
        Assert.Same(AppTheme.PanelSurface, AppTheme.ClassicPanelBackground);
        Assert.Same(AppTheme.HeaderSurface, AppTheme.ClassicHeaderBackground);
        Assert.Same(AppTheme.ClassicBorderNeutral, AppTheme.ClassicBorder);
        Assert.Same(AppTheme.ToolbarIconTeal, AppTheme.ToolbarIcon);
        Assert.Same(AppTheme.PieceCardCyan, AppTheme.DemoPanelBackground);
        Assert.Same(AppTheme.ProgressCyan, AppTheme.ToolbarAccent);
        Assert.Same(AppTheme.CanvasBlack, AppTheme.CadCanvasBackground);
        Assert.Same(AppTheme.RulerChrome, AppTheme.RulerBackground);
        Assert.Same(AppTheme.RulerTick, AppTheme.RulerForeground);
    }

    private static void AssertNeutral(IBrush brush)
    {
        var color = Assert.IsType<SolidColorBrush>(brush).Color;
        Assert.Equal(color.R, color.G);
        Assert.Equal(color.G, color.B);
    }
}
