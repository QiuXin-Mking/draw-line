using Avalonia.Controls;
using LeatherNesting.Desktop.Composition;
using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Modules.CadCanvas;
using LeatherNesting.Desktop.Modules.Pieces;
using LeatherNesting.Desktop.Shell;
using Xunit;

namespace LeatherNesting.Desktop.Tests.DesignSystem;

[Collection("Avalonia UI")]
public sealed class CloneSurfaceColorTests
{
    [Fact]
    public void Fixed_shell_reuses_chrome_roles_across_top_panes_canvas_and_status()
    {
        var shell = new AppShellView(DesktopComposition.CreateShellViewModel());
        var title = Assert.IsType<Border>(shell.TopCommands.ProductTitle.Parent);

        Assert.Same(AppTheme.ApplicationTitle, title.Background);
        Assert.Same(AppTheme.ToolbarSurface, shell.TopCommands.Background);
        Assert.Same(AppTheme.ToolbarSurface, shell.TopCommands.ToolbarScrollViewer.Background);
        Assert.All(shell.PersistentPaneHosts, host =>
        {
            Assert.Same(AppTheme.PanelSurface, host.Background);
            Assert.Same(AppTheme.HeaderSurface, host.Header.Background);
            Assert.Same(AppTheme.ClassicBorderNeutral, host.BorderBrush);
        });
        Assert.Same(AppTheme.CanvasBlack, shell.CanvasSurface.Background);
        Assert.Same(AppTheme.RulerChrome, shell.VerticalRuler.Background);
        Assert.Same(AppTheme.RulerChrome, shell.HorizontalRuler.Background);
        Assert.Same(AppTheme.StatusSurface, shell.StatusBar.Background);

        var statusRow = Assert.IsType<Grid>(shell.StatusBar.Child);
        Assert.All(
            statusRow.Children.OfType<TextBlock>().Where(text => text != shell.StatusDemoText),
            text => Assert.Same(AppTheme.PrimaryText, text.Foreground));
    }

    [Fact]
    public void Piece_cards_progress_and_property_focus_keep_separate_shared_state_roles()
    {
        var state = OrderPiecePanelState.CreateImage27Demo();
        var card = new PieceCardView(state, state.Pieces[0]);
        var progress = new ProgressSummaryView(state);
        state.LoadImage13PropertyDemo();
        var properties = new PiecePropertiesView(state);
        var progressBars = Assert.IsType<StackPanel>(progress.Content).Children.OfType<ProgressBar>().ToArray();

        Assert.Same(AppTheme.PieceCardCyan, card.Background);
        Assert.Equal(2, progressBars.Length);
        Assert.All(progressBars, bar => Assert.Same(AppTheme.ProgressCyan, bar.Foreground));
        Assert.Same(AppTheme.ClassicFocus, properties.FirstSingleSetEditor.BorderBrush);
        Assert.Same(AppTheme.PanelSurface, properties.Background);
    }

    [Fact]
    public void Cad_host_reuses_panel_canvas_and_header_roles()
    {
        var host = new CadWorkspaceHost(new CadHostState());
        var fileOperationRow = Assert.IsType<StackPanel>(host.Children[0]);
        var drawingToolRow = Assert.IsType<StackPanel>(host.Children[1]);
        var properties = new CadPropertyPane(new CadHostState());

        Assert.Same(AppTheme.PanelSurface, fileOperationRow.Background);
        Assert.Same(AppTheme.HeaderSurface, drawingToolRow.Background);
        Assert.Same(AppTheme.CanvasBlack, host.Canvas.Background);
        Assert.Same(AppTheme.PanelSurface, properties.Background);
    }
}
