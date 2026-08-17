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
        var area = new TopCommandArea(_ => { }, _ => { });

        Assert.Equal(ExpectedMenuLabels, area.MenuItems.Select(item => item.Header));

        var fileMenu = area.MenuItems.Single(item => Equals(item.Header, "文件"));
        Assert.Equal(
            ShellTopMenu.FileMenu.OfType<ShellMenuCommand>().Select(command => command.Label).ToArray(),
            fileMenu.Items.OfType<MenuItem>().Select(item => item.Header));
        Assert.All(fileMenu.Items.OfType<MenuItem>(), item => Assert.True(item.IsEnabled));

        var editMenu = area.MenuItems.Single(item => Equals(item.Header, "编辑"));
        Assert.Equal(
            ShellTopMenu.EditMenu.OfType<ShellMenuCommand>().Select(command => command.Label).ToArray(),
            editMenu.Items.OfType<MenuItem>().Select(item => item.Header));
        Assert.Equal(4, editMenu.Items.OfType<Separator>().Count());

        var operationMenu = area.MenuItems.Single(item => Equals(item.Header, "操作"));
        Assert.Equal(
            ShellTopMenu.OperationMenu.OfType<ShellMenuCommand>().Select(command => command.Label).ToArray(),
            operationMenu.Items.OfType<MenuItem>().Select(item => item.Header));
        Assert.Empty(operationMenu.Items.OfType<Separator>());
        var testItem = operationMenu.Items.OfType<MenuItem>().Single(item => Equals(item.Header, "测试项"));
        Assert.False(testItem.IsEnabled);
        Assert.All(
            operationMenu.Items.OfType<MenuItem>().Where(item => !Equals(item.Header, "测试项")),
            item => Assert.True(item.IsEnabled));

        var drawMenu = area.MenuItems.Single(item => Equals(item.Header, "绘制"));
        Assert.Equal(
            ShellTopMenu.DrawMenu.OfType<ShellMenuCommand>().Select(command => command.Label).ToArray(),
            drawMenu.Items.OfType<MenuItem>().Select(item => item.Header));

        var databaseMenu = area.MenuItems.Single(item => Equals(item.Header, "数据库"));
        Assert.Equal(
            ShellTopMenu.DatabaseMenu.OfType<ShellMenuCommand>().Select(command => command.Label).ToArray(),
            databaseMenu.Items.OfType<MenuItem>().Select(item => item.Header));

        var toolsMenu = area.MenuItems.Single(item => Equals(item.Header, "工具"));
        Assert.Equal(
            ShellTopMenu.ToolsMenu.OfType<ShellMenuCommand>().Select(command => command.Label).ToArray(),
            toolsMenu.Items.OfType<MenuItem>().Select(item => item.Header));

        var settingsMenu = area.MenuItems.Single(item => Equals(item.Header, "设置"));
        Assert.Equal(
            ShellTopMenu.SettingsMenu.Select(entry => entry switch
            {
                ShellMenuCommand command => command.Label,
                ShellMenuSubmenu submenu => submenu.Label,
                _ => string.Empty,
            }).ToArray(),
            settingsMenu.Items.OfType<MenuItem>().Select(item => item.Header).ToArray());

        var languageSubmenu = settingsMenu.Items.OfType<MenuItem>().Single(item => Equals(item.Header, "语言"));
        Assert.Equal(
            new[] { "中文", "英文" },
            languageSubmenu.Items.OfType<MenuItem>().Select(item => item.Header).ToArray());

        var helpMenu = area.MenuItems.Single(item => Equals(item.Header, "帮助"));
        Assert.Equal(
            ShellTopMenu.HelpMenu.OfType<ShellMenuCommand>().Select(command => command.Label).ToArray(),
            helpMenu.Items.OfType<MenuItem>().Select(item => item.Header));
        Assert.Empty(helpMenu.Items.OfType<Separator>());
        Assert.All(helpMenu.Items.OfType<MenuItem>(), item => Assert.True(item.IsEnabled));
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
        var area = new TopCommandArea(_ => { }, _ => { });

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
        var area = new TopCommandArea(viewModel.ActivateToolbarCommand, viewModel.ActivateMenuCommand);

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
    [Trait("TestId", "TOP-008")]
    public void File_menu_import_stays_placeholder_and_new_layout_opens_board_settings()
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
        var area = new TopCommandArea(viewModel.ActivateToolbarCommand, viewModel.ActivateMenuCommand);

        var fileMenu = area.MenuItems.Single(item => Equals(item.Header, "文件"));
        var importProgress = fileMenu.Items.Cast<MenuItem>()
            .Single(item => Equals(item.Header, "导入排版进度(axn)"));
        importProgress.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        Assert.Equal("M02", viewModel.CurrentModule!.Id);
        Assert.Contains("导入排版进度", workspace.Snapshot.TodoHint);

        var newLayout = fileMenu.Items.Cast<MenuItem>()
            .Single(item => Equals(item.Header, "新建排版"));
        var boardSettingsRequested = false;
        viewModel.BoardSettingsRequested += (_, _) => boardSettingsRequested = true;
        newLayout.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        Assert.True(boardSettingsRequested);
        Assert.Equal("M02", viewModel.CurrentModule!.Id);
        Assert.DoesNotContain("新建排版", workspace.Snapshot.TodoHint);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "TOP-009")]
    public void Edit_menu_commands_route_to_the_canvas_and_separators_group_actions()
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
        var area = new TopCommandArea(viewModel.ActivateToolbarCommand, viewModel.ActivateMenuCommand);

        var editMenu = area.MenuItems.Single(item => Equals(item.Header, "编辑"));
        var undo = editMenu.Items.OfType<MenuItem>()
            .Single(item => Equals(item.Header, "撤销(Ctrl+Z)"));
        undo.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        Assert.Equal("M03", viewModel.CurrentModule!.Id);
        Assert.Contains("撤销", workspace.Snapshot.TodoHint);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "TOP-010")]
    public void Operation_menu_commands_navigate_through_the_shell_and_keep_test_item_disabled()
    {
        var workspace = new InMemoryWorkspaceSession();
        var viewModel = new AppShellViewModel(
            [
                new TestModule("M01", 1),
                new TestModule("M02", 2),
                new TestModule("M03", 3),
                new TestModule("M05", 5),
                new TestModule("M07", 7),
                new TestModule("M08", 8),
                new TestModule("M11", 11),
                new TestModule("M12", 12),
            ],
            workspace,
            workspace);
        var area = new TopCommandArea(viewModel.ActivateToolbarCommand, viewModel.ActivateMenuCommand);

        var operationMenu = area.MenuItems.Single(item => Equals(item.Header, "操作"));
        var startNesting = operationMenu.Items.OfType<MenuItem>()
            .Single(item => Equals(item.Header, "开始排版"));
        startNesting.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        Assert.Equal("M08", viewModel.CurrentModule!.Id);
        Assert.Contains("开始排版", workspace.Snapshot.TodoHint);

        var testItem = operationMenu.Items.OfType<MenuItem>().Single(item => Equals(item.Header, "测试项"));
        Assert.False(testItem.IsEnabled);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "TOP-011")]
    public void Draw_database_and_tools_menus_navigate_to_their_target_modules()
    {
        var workspace = new InMemoryWorkspaceSession();
        var viewModel = new AppShellViewModel(
            [
                new TestModule("M01", 1),
                new TestModule("M02", 2),
                new TestModule("M03", 3),
                new TestModule("M05", 5),
                new TestModule("M07", 7),
                new TestModule("M08", 8),
                new TestModule("M11", 11),
                new TestModule("M12", 12),
            ],
            workspace,
            workspace);
        var area = new TopCommandArea(viewModel.ActivateToolbarCommand, viewModel.ActivateMenuCommand);

        var drawMenu = area.MenuItems.Single(item => Equals(item.Header, "绘制"));
        var drawHole = drawMenu.Items.OfType<MenuItem>().Single(item => Equals(item.Header, "绘制孔"));
        drawHole.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        Assert.Equal("M03", viewModel.CurrentModule!.Id);
        Assert.Contains("绘制孔", workspace.Snapshot.TodoHint);

        var databaseMenu = area.MenuItems.Single(item => Equals(item.Header, "数据库"));
        var orderManagement = databaseMenu.Items.OfType<MenuItem>().Single(item => Equals(item.Header, "订单管理"));
        orderManagement.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        Assert.Equal("M01", viewModel.CurrentModule!.Id);
        Assert.Contains("订单管理", workspace.Snapshot.TodoHint);

        var toolsMenu = area.MenuItems.Single(item => Equals(item.Header, "工具"));
        var cadTools = toolsMenu.Items.OfType<MenuItem>().Single(item => Equals(item.Header, "CAD工具"));
        cadTools.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        Assert.Equal("M02", viewModel.CurrentModule!.Id);
        Assert.Contains("CAD工具", workspace.Snapshot.TodoHint);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "TOP-012")]
    public void Settings_menu_navigates_commands_but_language_choices_stay_placeholder_only()
    {
        var workspace = new InMemoryWorkspaceSession();
        var viewModel = new AppShellViewModel(
            [
                new TestModule("M01", 1),
                new TestModule("M02", 2),
                new TestModule("M03", 3),
                new TestModule("M05", 5),
                new TestModule("M07", 7),
                new TestModule("M08", 8),
                new TestModule("M11", 11),
                new TestModule("M12", 12),
            ],
            workspace,
            workspace);
        var area = new TopCommandArea(viewModel.ActivateToolbarCommand, viewModel.ActivateMenuCommand);

        var settingsMenu = area.MenuItems.Single(item => Equals(item.Header, "设置"));
        var zoomExtent = settingsMenu.Items.OfType<MenuItem>().Single(item => Equals(item.Header, "范围缩放"));
        zoomExtent.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        Assert.Equal("M03", viewModel.CurrentModule!.Id);
        Assert.Contains("范围缩放", workspace.Snapshot.TodoHint);

        var settingsWindow = settingsMenu.Items.OfType<MenuItem>().Single(item => Equals(item.Header, "设置窗口"));
        settingsWindow.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        Assert.Equal("M12", viewModel.CurrentModule!.Id);
        Assert.Contains("设置窗口", workspace.Snapshot.TodoHint);

        var languageSubmenu = settingsMenu.Items.OfType<MenuItem>().Single(item => Equals(item.Header, "语言"));
        var chinese = languageSubmenu.Items.OfType<MenuItem>().Single(item => Equals(item.Header, "中文"));
        chinese.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        Assert.Contains("中文", workspace.Snapshot.TodoHint);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "TOP-013")]
    public void Help_menu_commands_navigate_to_the_management_module()
    {
        var workspace = new InMemoryWorkspaceSession();
        var viewModel = new AppShellViewModel(
            [
                new TestModule("M01", 1),
                new TestModule("M02", 2),
                new TestModule("M03", 3),
                new TestModule("M05", 5),
                new TestModule("M07", 7),
                new TestModule("M08", 8),
                new TestModule("M11", 11),
                new TestModule("M12", 12),
            ],
            workspace,
            workspace);
        var area = new TopCommandArea(viewModel.ActivateToolbarCommand, viewModel.ActivateMenuCommand);

        var helpMenu = area.MenuItems.Single(item => Equals(item.Header, "帮助"));
        var about = helpMenu.Items.OfType<MenuItem>().Single(item => Equals(item.Header, "关于..."));
        about.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        Assert.Equal("M12", viewModel.CurrentModule!.Id);
        Assert.Contains("关于", workspace.Snapshot.TodoHint);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "TOP-005")]
    public void Shell_uses_the_persistent_three_column_workstation_body_without_legacy_module_nav()
    {
        var shell = new AppShellView(DesktopComposition.CreateShellViewModel());
        var layout = Assert.IsType<Grid>(shell.Content);

        // 外层 = 常驻左缘折叠细条（Auto）+ 工作区（Star）；身体区仍为 13*,74*,13* 三列工作台。
        Assert.Equal(2, layout.ColumnDefinitions.Count);
        Assert.Equal(GridUnitType.Auto, layout.ColumnDefinitions[0].Width.GridUnitType);
        Assert.Equal(GridLength.Star, layout.ColumnDefinitions[1].Width);
        Assert.Equal(4, layout.Children.Count);
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
