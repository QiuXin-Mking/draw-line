using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.DesignSystem.CadTools;
using LeatherNesting.Desktop.Modules.CadCanvas.Toolbar;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Modules.CadCanvas.Toolbar;

[Collection("Avalonia UI")]
public sealed class CadToolbarViewTests
{
    [Fact]
    [Trait("Stage", "S3")]
    [Trait("TestId", "AC-CAD-T03")]
    public void Toolbar_has_fill_and_255_leading_controls_before_27_single_line_buttons()
    {
        var view = new CadToolbarView();

        Assert.Equal(Orientation.Horizontal, view.Orientation);
        Assert.Equal(HorizontalAlignment.Right, view.HorizontalAlignment);
        Assert.True(view.FillCheckBox.IsChecked);
        Assert.Same(AppTheme.DangerText, view.FillLabel.Foreground);
        Assert.Equal("填充", view.FillLabel.Text);
        Assert.Equal("255", view.FillValueInput.Text);
        Assert.Equal(27, view.Buttons.Count);
        Assert.Equal(29, view.Buttons.Count + 2);

        Assert.Same(view.FillCheckBox, view.Children[0]);
        Assert.Same(view.FillLabel, view.Children[1]);
        Assert.Same(view.FillValueInput, view.Children[2]);
        Assert.DoesNotContain(view.Children, child => child is WrapPanel);
    }

    [Fact]
    [Trait("Stage", "S3")]
    [Trait("TestId", "AC-CAD-T05")]
    public void Buttons_follow_catalog_metadata_and_compact_square_visual_contract()
    {
        var view = new CadToolbarView();

        Assert.Same(CadToolCatalog.All, view.Definitions);
        Assert.Equal(CadToolCatalog.All.Select(tool => tool.ControlId), view.Buttons.Select(button => button.Name));

        foreach (var pair in view.Definitions.Zip(view.Buttons))
        {
            Assert.Equal(24, pair.Second.Width);
            Assert.Equal(24, pair.Second.Height);
            Assert.Equal(new CornerRadius(0), pair.Second.CornerRadius);
            Assert.InRange(pair.Second.Padding.Left, 1, 2);
            Assert.InRange(pair.Second.Padding.Top, 1, 2);
            Assert.IsType<Viewbox>(pair.Second.Content);
            Assert.Equal(pair.First.Tooltip, ToolTip.GetTip(pair.Second));
            Assert.Equal(pair.First.Label, AutomationProperties.GetName(pair.Second));
            Assert.Equal(pair.First.ControlId, AutomationProperties.GetAutomationId(pair.Second));
            Assert.True(pair.Second.Focusable);
        }
    }

    [Fact]
    [Trait("Stage", "S3")]
    [Trait("TestId", "AC-CAD-T03")]
    public void Four_separators_are_inserted_exactly_at_catalog_group_boundaries()
    {
        var view = new CadToolbarView();

        Assert.Equal(4, view.Separators.Count);
        Assert.All(view.Separators, separator => Assert.Equal(24, separator.Height));

        var visualSequence = view.Children.Skip(3).ToArray();
        var expected = new List<Control>();
        for (var index = 0; index < view.Buttons.Count; index++)
        {
            if (index > 0 && view.Definitions[index - 1].Group != view.Definitions[index].Group)
                expected.Add(view.Separators[expected.Count(control => control is Border)]);
            expected.Add(view.Buttons[index]);
        }

        Assert.Equal(expected, visualSequence);
    }

    [Fact]
    [Trait("Stage", "S3")]
    public void Presentation_api_controls_visibility_active_enabled_and_stable_click_callback()
    {
        CadToolDefinition? invoked = null;
        var view = new CadToolbarView(definition => invoked = definition);
        var visible = new[] { CadToolCommandKey.Select, CadToolCommandKey.Undo, CadToolCommandKey.Settings };

        view.SetVisibleKeys(visible);
        view.SetActiveKey(CadToolCommandKey.Select);
        view.SetEnabledKeys([CadToolCommandKey.Select, CadToolCommandKey.Settings]);

        Assert.Equal(visible, view.VisibleDefinitions.Select(tool => tool.CommandKey));
        Assert.Equal(visible, view.Buttons.Where(button => button.IsVisible).Select(CommandFor));
        Assert.Same(AppTheme.SelectionSurface, Button(view, CadToolCommandKey.Select).Background);
        Assert.Same(AppTheme.ClassicFocus, Button(view, CadToolCommandKey.Select).BorderBrush);
        Assert.Equal("当前工具", AutomationProperties.GetItemStatus(Button(view, CadToolCommandKey.Select)));
        Assert.Null(AutomationProperties.GetItemStatus(Button(view, CadToolCommandKey.Settings)));
        Assert.False(Button(view, CadToolCommandKey.Undo).IsEnabled);
        Assert.Same(AppTheme.DisabledSurface, Button(view, CadToolCommandKey.Undo).Background);
        Assert.True(Button(view, CadToolCommandKey.Settings).IsEnabled);
        Assert.Single(view.Separators, separator => separator.IsVisible);

        Button(view, CadToolCommandKey.Settings).RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        Assert.Same(Tool(CadToolCommandKey.Settings), invoked);
    }

    private static Button Button(CadToolbarView view, CadToolCommandKey key) =>
        view.Buttons[Tool(key).Order - 1];

    private static CadToolCommandKey CommandFor(Button button) =>
        CadToolCatalog.All.Single(tool => tool.ControlId == button.Name).CommandKey;

    private static CadToolDefinition Tool(CadToolCommandKey key) =>
        CadToolCatalog.All.Single(tool => tool.CommandKey == key);
}
