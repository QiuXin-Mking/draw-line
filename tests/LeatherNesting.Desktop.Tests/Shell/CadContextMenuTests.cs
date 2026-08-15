using Avalonia.Controls;
using Avalonia.Interactivity;
using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Modules.CadCanvas;
using LeatherNesting.Desktop.Modules.Contracts;
using LeatherNesting.Desktop.Shell;
using LeatherNesting.Desktop.Workspace;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Shell;

[Collection("Avalonia UI")]
public sealed class CadContextMenuTests
{
    private static readonly string[] ExpectedLabels =
    [
        "手动排版（F5）", "添加分界", "删除分界", "撤销（Ctrl+Z）", "返回（Ctrl+Y）",
        "取消（Esc）", "移动", "旋转", "剪切（Ctrl+X）", "复制（Ctrl+C）",
        "粘贴（Ctrl+V）", "全选（Ctrl+A）", "反选（Shift+A）", "删除（Del）", "删除外部",
        "清空全部", "镜像（Ctrl+M）", "组合模块（Ctrl+G）", "取消组合（Shift+G）", "导到订单（Ctrl+T）",
        "组合裁片（Ctrl+Shift+G）",
    ];

    private static readonly string[] ExpectedDisabledLabels = ["删除分界", "粘贴（Ctrl+V）"];

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "CTX-001")]
    public void Contract_keeps_the_required_21_items_in_the_reference_order_without_separators()
    {
        Assert.Equal(ExpectedLabels, ShellContextMenu.Entries.OfType<ShellMenuCommand>().Select(entry => entry.Label));
        Assert.Equal(21, ShellContextMenu.Entries.Count);
        Assert.DoesNotContain(ShellContextMenu.Entries, entry => entry is ShellMenuSeparator);
        Assert.All(ShellContextMenu.Entries.OfType<ShellMenuCommand>(), command =>
        {
            Assert.Equal("M03", command.TargetModuleId);
            Assert.True(command.IsPlaceholderAction);
        });
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "CTX-002")]
    public void Delete_partition_and_paste_are_disabled_and_every_other_item_is_enabled()
    {
        var commands = ShellContextMenu.Entries.OfType<ShellMenuCommand>().ToArray();

        Assert.Equal(ExpectedDisabledLabels, commands.Where(command => !command.IsEnabled).Select(command => command.Label));
        Assert.All(
            commands.Where(command => !ExpectedDisabledLabels.Contains(command.Label)),
            command => Assert.True(command.IsEnabled));
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "CTX-003")]
    public void Workspace_host_attaches_a_context_menu_with_the_contract_items()
    {
        var host = new CadWorkspaceHost(new CadHostState());

        Assert.NotNull(host.Drawing.ContextMenu);
        var items = host.Drawing.ContextMenu!.Items.OfType<MenuItem>().ToArray();
        Assert.Equal(ExpectedLabels, items.Select(item => item.Header));
        Assert.Equal(
            ExpectedDisabledLabels,
            items.Where(item => !item.IsEnabled).Select(item => item.Header));
        Assert.All(items, item => Assert.Same(AppTheme.PrimaryText, item.Foreground));
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "CTX-004")]
    public void Context_menu_activation_routes_to_the_canvas_and_stays_an_honest_todo()
    {
        var workspace = new InMemoryWorkspaceSession();
        var viewModel = new AppShellViewModel(
            [
                new TestModule("M01", 1),
                new TestModule("M03", 3),
            ],
            workspace,
            workspace);
        var host = new CadWorkspaceHost(viewModel.CadHost, null, viewModel.ActivateContextCommand);

        var items = host.Drawing.ContextMenu!.Items.OfType<MenuItem>().ToArray();
        var undo = items.Single(item => Equals(item.Header, "撤销（Ctrl+Z）"));
        undo.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        Assert.Equal("M03", viewModel.CurrentModule!.Id);
        Assert.Contains("撤销", workspace.Snapshot.TodoHint);
        Assert.Contains(TodoBadge.StandardText, workspace.Snapshot.TodoHint);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "CTX-005")]
    public void Activate_context_command_delegates_to_the_shared_menu_activation_path()
    {
        var workspace = new InMemoryWorkspaceSession();
        var viewModel = new AppShellViewModel(
            [
                new TestModule("M01", 1),
                new TestModule("M03", 3),
            ],
            workspace,
            workspace);

        var undo = ShellContextMenu.Entries.OfType<ShellMenuCommand>()
            .Single(command => Equals(command.Label, "撤销（Ctrl+Z）"));
        viewModel.ActivateContextCommand(undo);

        Assert.Equal("M03", viewModel.CurrentModule!.Id);
        Assert.Contains("撤销", workspace.Snapshot.TodoHint);
    }

    private sealed class TestModule(string id, int order) : IDesktopModule
    {
        public DesktopModuleMetadata Metadata { get; } = new(id, id, "Test", order);

        public Func<Control> CreateView => () => new Border();
    }
}
