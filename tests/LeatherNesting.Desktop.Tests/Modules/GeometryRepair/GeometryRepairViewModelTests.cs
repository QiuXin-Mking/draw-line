using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Modules.Contracts;
using LeatherNesting.Desktop.Modules.GeometryRepair;
using LeatherNesting.Desktop.ViewModels;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Modules.GeometryRepair;

public sealed class GeometryRepairViewModelTests
{
    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T04")]
    public void Demo_starts_with_actionable_issues_and_grouped_tools()
    {
        var viewModel = new GeometryRepairViewModel();

        Assert.Contains(viewModel.Issues, issue => issue.Severity == RepairIssueSeverity.Blocking && issue.ObjectId == "OPEN-001");
        Assert.Contains(viewModel.Issues, issue => issue.Kind == "自交风险");
        Assert.Equal("OPEN-001", viewModel.SelectedIssue.ObjectId);
        Assert.Equal(new[] { "轮廓修复", "偏移", "节点", "剪断" }, viewModel.ToolGroups.Select(group => group.Name));
        Assert.Contains(viewModel.ToolGroups.SelectMany(group => group.Tools), tool => tool.Action == RepairToolAction.CloseContour && tool.IsConnected);
        Assert.Contains(viewModel.ToolGroups.SelectMany(group => group.Tools), tool => tool.Action == RepairToolAction.MoveNode && !tool.IsConnected);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T04")]
    public void Close_preview_exposes_workbench_state_and_geometry_difference()
    {
        var viewModel = new GeometryRepairViewModel();

        Assert.True(viewModel.Preview(RepairToolAction.CloseContour));

        Assert.Equal(WorkbenchState.Previewing, viewModel.State);
        Assert.True(viewModel.CanCommit);
        Assert.True(viewModel.CanCancel);
        Assert.Equal(4, viewModel.Difference.BeforeCurveCount);
        Assert.Equal(5, viewModel.Difference.AfterCurveCount);
        Assert.Equal(1, viewModel.Difference.AddedCurveCount);
        Assert.Contains("新增", viewModel.Difference.TopologyChange);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T04")]
    public void Cancel_returns_to_ready_and_clears_the_pending_difference()
    {
        var viewModel = new GeometryRepairViewModel();
        Assert.True(viewModel.Preview(RepairToolAction.CloseContour));

        viewModel.CancelPreview();

        Assert.Equal(WorkbenchState.Ready, viewModel.State);
        Assert.False(viewModel.CanCommit);
        Assert.Equal(0, viewModel.Difference.AddedCurveCount);
        Assert.Contains("已取消", viewModel.Feedback);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T04")]
    public void Committed_session_operation_can_be_undone_and_redone()
    {
        var viewModel = new GeometryRepairViewModel();
        Assert.True(viewModel.Preview(RepairToolAction.CloseContour));

        Assert.True(viewModel.CommitPreview());
        Assert.True(viewModel.CanUndo);
        Assert.True(viewModel.Undo());
        Assert.True(viewModel.CanRedo);
        Assert.Equal(4, viewModel.Difference.AfterCurveCount);

        Assert.True(viewModel.Redo());
        Assert.Equal(5, viewModel.Difference.AfterCurveCount);
    }

    [Theory]
    [InlineData(RepairToolAction.InsertNode)]
    [InlineData(RepairToolAction.MoveNode)]
    [InlineData(RepairToolAction.DeleteNode)]
    [InlineData(RepairToolAction.BreakAtPoint)]
    [InlineData(RepairToolAction.RemoveSegment)]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T04")]
    public void Gesture_dependent_tools_are_todo_and_do_not_change_session_geometry(RepairToolAction action)
    {
        var viewModel = new GeometryRepairViewModel();
        var before = viewModel.GeometrySignature;

        Assert.False(viewModel.Preview(action));

        Assert.Equal(before, viewModel.GeometrySignature);
        Assert.Equal(WorkbenchState.Ready, viewModel.State);
        Assert.Contains(TodoBadge.StandardText, viewModel.Feedback);
    }

    [Theory]
    [InlineData(RepairTodoAction.BatchRepair)]
    [InlineData(RepairTodoAction.PersistToProject)]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T04")]
    public void Project_level_todo_actions_never_change_workbench_or_demo_geometry(RepairTodoAction action)
    {
        var viewModel = new GeometryRepairViewModel();
        var before = viewModel.GeometrySignature;

        viewModel.InvokeTodo(action);

        Assert.Equal(before, viewModel.GeometrySignature);
        Assert.Equal(WorkbenchState.Ready, viewModel.State);
        Assert.Contains(TodoBadge.StandardText, viewModel.Feedback);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T04")]
    public void Module_definition_registers_M04_in_the_CAD_group()
    {
        IDesktopModule module = new GeometryRepairModule();

        Assert.Equal("M04", module.Metadata.Id);
        Assert.Equal("几何修复", module.Metadata.Title);
        Assert.Equal("CAD 工作台", module.Metadata.Group);
        Assert.Equal(4, module.Metadata.Order);
        Assert.IsType<GeometryRepairView>(module.CreateView());
    }
}
