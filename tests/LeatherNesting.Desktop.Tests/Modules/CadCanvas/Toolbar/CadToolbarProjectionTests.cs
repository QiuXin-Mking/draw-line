using Avalonia.Automation;
using Avalonia.Controls;
using LeatherNesting.Desktop.Modules.CadCanvas.Toolbar;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Modules.CadCanvas.Toolbar;

[Collection("Avalonia UI")]
public sealed class CadToolbarProjectionTests
{
    [Fact]
    [Trait("Stage", "S4")]
    [Trait("TestId", "AC-CAD-T04")]
    public void State_projects_review_visibility_active_and_availability_through_command_keys()
    {
        var state = new CadToolbarState();
        var view = new CadToolbarView(definition => state.TryExecute(definition.CommandKey));
        state.Changed += (_, _) => Project(state, view);
        Project(state, view);

        state.SetMode(CadToolbarMode.NestingReview);
        state.SetAvailability(hasUndo: true, hasRedo: false, hasSelection: false);

        Assert.Equal(state.VisibleTools.Select(tool => tool.ControlId),
            view.Buttons.Where(button => button.IsVisible).Select(AutomationProperties.GetAutomationId));
        Assert.Equal(6, view.VisibleDefinitions.Count);
        Assert.Equal(CadToolCommandKey.Select, view.ActiveKey);
        Assert.True(Button(view, CadToolCommandKey.Undo).IsEnabled);
        Assert.False(Button(view, CadToolCommandKey.Redo).IsEnabled);
        Assert.False(Button(view, CadToolCommandKey.Delete).IsEnabled);
        Assert.False(Button(view, CadToolCommandKey.Cancel).IsEnabled);

        state.SetMode(CadToolbarMode.CadEdit);
        Button(view, CadToolCommandKey.DrawRectangle).RaiseEvent(
            new Avalonia.Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        Assert.Equal(CadToolCommandKey.DrawRectangle, state.ActiveTool);
        Assert.Equal(CadToolCommandKey.DrawRectangle, view.ActiveKey);
        Assert.Equal("CAD-08", AutomationProperties.GetAutomationId(
            Button(view, CadToolCommandKey.DrawRectangle)));
    }

    private static void Project(CadToolbarState state, CadToolbarView view)
    {
        view.SetVisibleKeys(state.VisibleTools.Select(tool => tool.CommandKey));
        view.SetActiveKey(state.ActiveTool);
        view.SetEnabledKeys(state.VisibleTools
            .Where(tool => state.CanExecute(tool.CommandKey))
            .Select(tool => tool.CommandKey));
    }

    private static Button Button(CadToolbarView view, CadToolCommandKey key)
    {
        var definition = CadToolCatalog.All.Single(tool => tool.CommandKey == key);
        return view.Buttons[definition.Order - 1];
    }
}
