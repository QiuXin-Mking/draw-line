using LeatherNesting.Desktop.ViewModels;
using LeatherNesting.Geometry;
using Xunit;

namespace LeatherNesting.Desktop.Tests;

public sealed class CadWorkbenchViewModelTests
{
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

        // Only one tool active at a time
        Assert.NotEqual(CadToolMode.BoundaryRepair, vm.ToolMode);
    }

    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-UI-001")]
    public void Preview_commit_cancel_states_are_correct()
    {
        var vm = new CadWorkbenchViewModel();
        vm.LoadLoops([]);

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
        vm.LoadLoops([]);

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
        var loop = new Loop2D("l1", LoopRole.Outer, [
            new LineSegment2D(new(0, 0), new(10, 0)),
            new LineSegment2D(new(10, 0), new(10, 10)),
            new LineSegment2D(new(10, 10), new(0, 10)),
            new LineSegment2D(new(0, 10), new(0, 0)),
        ]);

        vm.LoadLoops([loop]);
        vm.SelectTool(CadToolMode.BoundaryRepair);
        vm.PreviewClose();
        vm.Commit();

        Assert.Equal(WorkbenchState.Committed, vm.State);
    }
}