using LeatherNesting.Desktop.ViewModels;
using LeatherNesting.Geometry;
using Xunit;

namespace LeatherNesting.Desktop.Tests;

public sealed class CadWorkbenchViewModelTests
{
    private static Loop2D ClosedRectangle() => new("rect", LoopRole.Outer, [
        new LineSegment2D(new(0, 0), new(100, 0)),
        new LineSegment2D(new(100, 0), new(100, 50)),
        new LineSegment2D(new(100, 50), new(0, 50)),
        new LineSegment2D(new(0, 50), new(0, 0)),
    ]);

    private static Loop2D OpenContour() => new("open", LoopRole.Outer, [
        new LineSegment2D(new(0.05, 0), new(100, 0)),
        new LineSegment2D(new(100, 0), new(100, 50)),
        new LineSegment2D(new(100, 50), new(0, 50)),
        new LineSegment2D(new(0, 50), new(0, 0)),
    ]);

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-UI-001")]
    public void Tool_modes_are_mutually_exclusive()
    {
        var vm = new CadWorkbenchViewModel();

        vm.LoadLoops([]);
        vm.SelectTool(CadToolMode.BoundaryRepair);
        Assert.Equal(CadToolMode.BoundaryRepair, vm.ToolMode);

        vm.SelectTool(CadToolMode.Offset);
        Assert.Equal(CadToolMode.Offset, vm.ToolMode);

        Assert.NotEqual(CadToolMode.BoundaryRepair, vm.ToolMode);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-UI-001")]
    public void Preview_commit_cancel_states_are_correct()
    {
        var vm = new CadWorkbenchViewModel();
        vm.LoadLoops([ClosedRectangle()]);

        Assert.Equal(WorkbenchState.Ready, vm.State);
        Assert.False(vm.CanCommit);
        Assert.False(vm.CanCancel);

        vm.SelectTool(CadToolMode.BoundaryRepair);
        vm.PreviewClose();

        Assert.Equal(WorkbenchState.Previewing, vm.State);
        Assert.True(vm.CanCommit);
        Assert.True(vm.CanCancel);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-UI-001")]
    public void Cancel_returns_to_ready_state()
    {
        var vm = new CadWorkbenchViewModel();
        vm.LoadLoops([ClosedRectangle()]);

        vm.SelectTool(CadToolMode.BoundaryRepair);
        vm.PreviewClose();
        vm.Cancel();

        Assert.Equal(WorkbenchState.Ready, vm.State);
        Assert.False(vm.CanCommit);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-UI-001")]
    public void Commit_transitions_to_committed_state()
    {
        var vm = new CadWorkbenchViewModel();
        vm.LoadLoops([ClosedRectangle()]);

        vm.SelectTool(CadToolMode.BoundaryRepair);
        vm.PreviewClose();
        vm.Commit();

        Assert.Equal(WorkbenchState.Committed, vm.State);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-UI-001")]
    public void PreviewClose_bridges_open_contour()
    {
        var vm = new CadWorkbenchViewModel();
        vm.LoadLoops([OpenContour()]);

        Assert.Equal(4, vm.CurrentLoops![0].Curves.Count);

        vm.PreviewClose();

        Assert.Equal(WorkbenchState.Previewing, vm.State);
        // Closing bridged the 0.05 gap, adding one more curve.
        Assert.Equal(5, vm.CurrentLoops![0].Curves.Count);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-UI-001")]
    public void Commit_then_undo_restores_contour()
    {
        var vm = new CadWorkbenchViewModel();
        vm.LoadLoops([OpenContour()]);

        vm.PreviewClose();
        vm.Commit();
        Assert.Equal(5, vm.CurrentLoops![0].Curves.Count);

        vm.Undo();
        Assert.Equal(WorkbenchState.Ready, vm.State);
        Assert.Equal(4, vm.CurrentLoops![0].Curves.Count);
    }
}
