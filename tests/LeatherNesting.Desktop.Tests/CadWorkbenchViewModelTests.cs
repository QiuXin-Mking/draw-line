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

    [Fact]
    public void Changed_reports_each_observable_workbench_transition()
    {
        var vm = new CadWorkbenchViewModel();
        var changes = 0;
        vm.Changed += (_, _) => changes++;

        vm.LoadLoops([OpenContour()]);
        Assert.Equal(1, changes);
        vm.SelectPiece(new Point2D(10, 10));
        Assert.Equal(2, changes);
        vm.PreviewClose();
        Assert.Equal(3, changes);
        vm.Cancel();
        Assert.Equal(4, changes);
        vm.PreviewClose();
        vm.Commit();
        vm.Undo();
        vm.Redo();

        Assert.Equal(8, changes);
    }

    [Fact]
    public void Loading_new_loops_clears_the_previous_selection()
    {
        var vm = new CadWorkbenchViewModel();
        vm.LoadLoops([ClosedRectangle()]);
        vm.SelectPiece(new Point2D(10, 10));
        Assert.Equal("rect", vm.SelectedLoopId);

        vm.LoadLoops([OpenContour()]);

        Assert.Null(vm.SelectedLoopId);
    }

    [Fact]
    public void ClearSelection_notifies_only_when_selection_changes()
    {
        var vm = new CadWorkbenchViewModel();
        vm.LoadLoops([ClosedRectangle()]);
        vm.SelectPiece(new Point2D(10, 10));
        var changes = 0;
        vm.Changed += (_, _) => changes++;

        vm.ClearSelection();
        vm.ClearSelection();

        Assert.Equal(1, changes);
        Assert.Null(vm.SelectedLoopId);
    }

    [Fact]
    public void Invalid_transactions_do_not_claim_success()
    {
        var vm = new CadWorkbenchViewModel();
        var changes = 0;
        vm.Changed += (_, _) => changes++;

        vm.Commit();
        Assert.Equal(WorkbenchState.Ready, vm.State);
        Assert.Contains("session", Assert.Single(vm.ProblemMessages));
        vm.Undo();
        Assert.Equal(WorkbenchState.Ready, vm.State);
        Assert.Contains("session", Assert.Single(vm.ProblemMessages));
        vm.Redo();
        Assert.Equal(WorkbenchState.Ready, vm.State);
        Assert.Contains("session", Assert.Single(vm.ProblemMessages));
        Assert.Equal(3, changes);
    }

    [Fact]
    public void Committed_session_accepts_the_next_edit_preview()
    {
        var vm = new CadWorkbenchViewModel();
        vm.LoadLoops([OpenContour()]);
        vm.PreviewClose();
        vm.Commit();

        Assert.True(vm.CanPreview);
        vm.SelectPiece(new Point2D(10, 10));
        vm.RotateSelected(15);

        Assert.Equal(WorkbenchState.Previewing, vm.State);
    }
}
