using LeatherNesting.Desktop.Modules.CadCanvas.Toolbar;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Modules.CadCanvas.Toolbar;

public sealed class CadToolbarStateTests
{
    [Fact]
    [Trait("TestId", "AC-CAD-T04")]
    public void Defaults_to_edit_mode_with_select_active_and_all_tools_visible()
    {
        var state = new CadToolbarState();

        Assert.Equal(CadToolbarMode.CadEdit, state.Mode);
        Assert.Equal(CadToolCommandKey.Select, state.ActiveTool);
        Assert.False(state.HasUndo);
        Assert.False(state.HasRedo);
        Assert.False(state.HasSelection);
        Assert.False(state.HasPendingStep);
        Assert.Equal(Enumerable.Range(1, 27), state.VisibleTools.Select(tool => tool.Order));
    }

    [Fact]
    [Trait("TestId", "AC-CAD-T04")]
    public void Review_mode_projects_only_the_six_common_tools_in_catalog_order()
    {
        var state = new CadToolbarState();

        state.SetMode(CadToolbarMode.NestingReview);

        Assert.Equal(
            [
                CadToolCommandKey.Select,
                CadToolCommandKey.Undo,
                CadToolCommandKey.Redo,
                CadToolCommandKey.Cancel,
                CadToolCommandKey.Delete,
                CadToolCommandKey.Settings,
            ],
            state.VisibleTools.Select(tool => tool.CommandKey));
        Assert.Equal(
            ["CAD-05", "CAD-26", "CAD-27", "CAD-28", "CAD-29", "CAD-30"],
            state.VisibleTools.Select(tool => tool.ControlId));
    }

    [Fact]
    public void Availability_drives_history_selection_and_cancel_commands()
    {
        var state = new CadToolbarState();

        Assert.False(state.CanExecute(CadToolCommandKey.Undo));
        Assert.False(state.CanExecute(CadToolCommandKey.Redo));
        Assert.False(state.CanExecute(CadToolCommandKey.Delete));
        Assert.False(state.CanExecute(CadToolCommandKey.Cancel));
        Assert.True(state.CanExecute(CadToolCommandKey.Select));
        Assert.True(state.CanExecute(CadToolCommandKey.Settings));

        state.SetAvailability(hasUndo: true, hasRedo: true, hasSelection: true);
        state.SetPendingStep(true);

        Assert.True(state.CanExecute(CadToolCommandKey.Undo));
        Assert.True(state.CanExecute(CadToolCommandKey.Redo));
        Assert.True(state.CanExecute(CadToolCommandKey.Delete));
        Assert.True(state.CanExecute(CadToolCommandKey.Cancel));
    }

    [Fact]
    public void Commands_that_are_not_visible_in_the_current_mode_cannot_execute()
    {
        var state = new CadToolbarState();
        state.SetMode(CadToolbarMode.NestingReview);

        Assert.False(state.CanExecute(CadToolCommandKey.DrawRectangle));
        Assert.False(state.TryExecute(CadToolCommandKey.DrawRectangle));
        Assert.Equal(CadToolCommandKey.Select, state.ActiveTool);
    }

    [Fact]
    [Trait("TestId", "AC-CAD-T06")]
    public void Selecting_a_persistent_tool_replaces_the_active_tool_and_clears_pending_step()
    {
        var state = new CadToolbarState();
        state.SetPendingStep(true);

        var executed = state.TryExecute(CadToolCommandKey.DrawRectangle);

        Assert.True(executed);
        Assert.Equal(CadToolCommandKey.DrawRectangle, state.ActiveTool);
        Assert.False(state.HasPendingStep);

        state.TryExecute(CadToolCommandKey.DrawCircle);

        Assert.Equal(CadToolCommandKey.DrawCircle, state.ActiveTool);
    }

    [Theory]
    [InlineData(CadToolCommandKey.ExportToOrder)]
    [InlineData(CadToolCommandKey.Refit)]
    [InlineData(CadToolCommandKey.Undo)]
    [InlineData(CadToolCommandKey.Redo)]
    [InlineData(CadToolCommandKey.Delete)]
    [InlineData(CadToolCommandKey.Settings)]
    public void Momentary_commands_never_replace_the_active_tool(CadToolCommandKey command)
    {
        var state = new CadToolbarState();
        state.TryExecute(CadToolCommandKey.DrawPolyline);
        state.SetAvailability(hasUndo: true, hasRedo: true, hasSelection: true);

        var executed = state.TryExecute(command);

        Assert.True(executed);
        Assert.Equal(CadToolCommandKey.DrawPolyline, state.ActiveTool);
    }

    [Fact]
    [Trait("TestId", "AC-CAD-T09")]
    public void Cancel_and_escape_clear_a_pending_step_before_returning_to_select()
    {
        var state = new CadToolbarState();
        state.TryExecute(CadToolCommandKey.DrawPolyline);
        state.SetPendingStep(true);

        Assert.True(state.TryExecute(CadToolCommandKey.Cancel));
        Assert.Equal(CadToolCommandKey.DrawPolyline, state.ActiveTool);
        Assert.False(state.HasPendingStep);

        Assert.True(state.HandleEscape());
        Assert.Equal(CadToolCommandKey.Select, state.ActiveTool);

        Assert.False(state.HandleEscape());
        Assert.Equal(CadToolCommandKey.Select, state.ActiveTool);
    }

    [Fact]
    [Trait("TestId", "AC-CAD-T09")]
    public void Cancel_never_changes_selection_availability()
    {
        var state = new CadToolbarState();
        state.SetAvailability(hasUndo: false, hasRedo: false, hasSelection: true);
        state.TryExecute(CadToolCommandKey.DrawRectangle);

        state.HandleEscape();

        Assert.True(state.HasSelection);
        Assert.True(state.CanExecute(CadToolCommandKey.Delete));
    }

    [Fact]
    public void Switching_mode_resets_an_invisible_active_tool_and_pending_step_once()
    {
        var state = new CadToolbarState();
        state.TryExecute(CadToolCommandKey.DrawSpline);
        state.SetPendingStep(true);
        var changes = 0;
        state.Changed += (_, _) => changes++;

        state.SetMode(CadToolbarMode.NestingReview);

        Assert.Equal(CadToolbarMode.NestingReview, state.Mode);
        Assert.Equal(CadToolCommandKey.Select, state.ActiveTool);
        Assert.False(state.HasPendingStep);
        Assert.Equal(1, changes);
    }

    [Fact]
    public void Every_effective_transition_raises_changed_once_and_no_op_transitions_raise_none()
    {
        var state = new CadToolbarState();
        var changes = 0;
        state.Changed += (_, _) => changes++;

        state.SetMode(CadToolbarMode.CadEdit);
        state.SetAvailability(hasUndo: false, hasRedo: false, hasSelection: false);
        state.SetPendingStep(false);
        state.TryExecute(CadToolCommandKey.Select);
        state.HandleEscape();
        Assert.Equal(0, changes);

        state.SetAvailability(hasUndo: true, hasRedo: true, hasSelection: true);
        Assert.Equal(1, changes);

        state.TryExecute(CadToolCommandKey.DrawLine);
        Assert.Equal(2, changes);

        state.SetPendingStep(true);
        Assert.Equal(3, changes);

        state.TryExecute(CadToolCommandKey.Select);
        Assert.Equal(4, changes);
    }
}
