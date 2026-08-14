using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Composition;
using LeatherNesting.Desktop.Modules.Contracts;
using LeatherNesting.Desktop.Shell;
using LeatherNesting.Desktop.Workspace;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Shell;

[Collection("Avalonia UI")]
public sealed class TopCommandAreaTests
{
    private static readonly string[] ExpectedMenuLabels =
        ["文件", "编辑", "操作", "绘制", "数据库", "工具", "设置", "帮助"];

    private static readonly string[] ExpectedToolbarLabels =
    [
        "新建排版", "订单管理", "CAD工具", "开始排版", "停止排版",
        "取消排版", "范围缩放", "设置窗口", "等宽长条", "发送切割",
    ];

    private static readonly string[] ExpectedTargetModuleIds =
        ["M01", "M01", "M02", "M08", "M08", "M08", "M03", "M12", "M05", "M11"];

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "TOP-001")]
    public void Top_command_contract_keeps_the_required_labels_order_icons_and_module_routes()
    {
        Assert.Equal(ExpectedMenuLabels, ShellTopMenu.Labels);
        Assert.Equal(ExpectedToolbarLabels, ShellToolbar.Commands.Select(command => command.Label));
        Assert.Equal(ExpectedTargetModuleIds, ShellToolbar.Commands.Select(command => command.TargetModuleId));
        Assert.Equal(10, ShellToolbar.Commands.Select(command => command.Icon).Distinct().Count());
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "TOP-002")]
    public void Every_toolbar_icon_is_an_independent_vector_control()
    {
        var icons = ShellToolbar.Commands
            .Select(command => ToolbarIconFactory.Create(command.Icon))
            .ToArray();

        Assert.All(icons, icon => Assert.NotEmpty(icon.Shapes));
        Assert.All(icons.SelectMany(icon => icon.Shapes), shape => Assert.True(shape.Stroke is not null || shape.Fill is not null));
        Assert.Equal(10, icons.Select(icon => icon.Key).Distinct().Count());
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "TOP-003")]
    public void Top_area_builds_expandable_todo_menus_and_a_horizontally_accessible_icon_toolbar()
    {
        var area = new TopCommandArea(_ => { });

        Assert.Equal(ExpectedMenuLabels, area.MenuItems.Select(item => item.Header));
        Assert.All(area.MenuItems, item =>
        {
            var placeholder = Assert.Single(item.Items.Cast<object>());
            Assert.Equal(ShellTopMenu.PlaceholderText, Assert.IsType<MenuItem>(placeholder).Header);
        });
        Assert.Equal(ExpectedToolbarLabels, area.CommandButtons.Select(button => button.Descriptor.Label));
        Assert.All(area.CommandButtons, button =>
        {
            Assert.NotNull(button.Icon);
            Assert.Equal(button.Descriptor.Icon, button.Icon.Key);
            var content = Assert.IsType<StackPanel>(button.Content);
            Assert.Same(button.Icon, content.Children[0]);
            Assert.Equal(button.Descriptor.Label, Assert.IsType<TextBlock>(content.Children[1]).Text);
        });
        Assert.Equal(ScrollBarVisibility.Auto, area.ToolbarScrollViewer.HorizontalScrollBarVisibility);
        Assert.Equal(ScrollBarVisibility.Disabled, area.ToolbarScrollViewer.VerticalScrollBarVisibility);
        Assert.Equal(0, area.ToolbarScrollViewer.MinWidth);
        Assert.True(
            area.CommandButtons.Sum(button => button.Width) <= 1366 - 220,
            "The complete toolbar must fit in the 1366px acceptance viewport; narrower viewports use horizontal scrolling.");
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "TOP-007")]
    public void Menu_labels_use_explicit_dark_text_on_the_light_menu_surface()
    {
        var area = new TopCommandArea(_ => { });

        Assert.All(area.MenuItems, item => Assert.Same(AppTheme.PrimaryText, item.Foreground));
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "TOP-004")]
    public void Toolbar_clicks_navigate_through_the_shell_and_placeholder_actions_remain_honest()
    {
        var workspace = new InMemoryWorkspaceSession();
        var viewModel = new AppShellViewModel(
            [
                new TestModule("M01", 1),
                new TestModule("M02", 2),
                new TestModule("M03", 3),
                new TestModule("M05", 5),
                new TestModule("M08", 8),
                new TestModule("M11", 11),
                new TestModule("M12", 12),
            ],
            workspace,
            workspace);
        var area = new TopCommandArea(viewModel.ActivateToolbarCommand);

        var orderManagement = area.CommandButtons.Single(button => button.Descriptor.Label == "订单管理");
        orderManagement.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.Equal("M01", viewModel.CurrentModule!.Id);
        Assert.Null(workspace.Snapshot.TodoHint);

        var cadTools = area.CommandButtons.Single(button => button.Descriptor.Label == "CAD工具");
        cadTools.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.Equal("M02", viewModel.CurrentModule!.Id);
        Assert.Null(workspace.Snapshot.TodoHint);

        var startNesting = area.CommandButtons.Single(button => button.Descriptor.Label == "开始排版");
        startNesting.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.Equal("M08", viewModel.CurrentModule!.Id);
        Assert.Contains(TodoBadge.StandardText, workspace.Snapshot.TodoHint);
        Assert.Contains("开始排版", workspace.Snapshot.TodoHint);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "TOP-005")]
    public void Shell_uses_the_persistent_three_column_workstation_body_without_legacy_module_nav()
    {
        var shell = new AppShellView(DesktopComposition.CreateShellViewModel());
        var layout = Assert.IsType<Grid>(shell.Content);

        Assert.Single(layout.ColumnDefinitions);
        Assert.Equal(GridLength.Star, layout.ColumnDefinitions[0].Width);
        Assert.Equal(3, layout.Children.Count);
        Assert.DoesNotContain(layout.Children, child => Grid.GetRowSpan(child) == 3);
        Assert.Equal(1, Grid.GetColumn(shell.CanvasSurface));
        Assert.Equal(0, Grid.GetRow(shell.WorkspaceContent));
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "TOP-006")]
    public void Shell_starts_on_the_M03_CAD_canvas_in_the_main_workspace()
    {
        var viewModel = DesktopComposition.CreateShellViewModel();
        var shell = new AppShellView(viewModel);

        Assert.Equal("M03", viewModel.CurrentModule!.Id);
        Assert.Same(viewModel.CurrentView, shell.WorkspaceContent.Content);
    }

    private sealed class TestModule(string id, int order) : IDesktopModule
    {
        public DesktopModuleMetadata Metadata { get; } = new(id, id, "Test", order);

        public Func<Control> CreateView => () => new Border();
    }
}
