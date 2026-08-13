using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Composition;
using LeatherNesting.Desktop.Modules.Contracts;
using LeatherNesting.Desktop.Shell;
using LeatherNesting.Desktop.Workspace;
using Avalonia.Controls;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Shell;

public sealed class ShellTests
{
    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T00")]
    public void Shell_registers_exactly_twelve_modules()
    {
        var viewModel = new AppShellViewModel();
        Assert.Equal(12, viewModel.Modules.Count);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T00")]
    public void Module_ids_are_unique_and_ordered()
    {
        var viewModel = new AppShellViewModel();
        var ids = viewModel.Modules.Select(m => m.Id).ToList();
        Assert.Equal(12, ids.Distinct().Count());
        Assert.Equal("M01", ids[0]);
        Assert.Equal("M12", ids[^1]);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T00")]
    public void Import_module_has_real_logic()
    {
        var viewModel = new AppShellViewModel();
        var import = viewModel.Modules.Single(m => m.Id == "M02");
        Assert.True(import.HasRealLogic);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T00")]
    public void Todo_badge_has_readable_text()
    {
        Assert.Contains("TODO", TodoBadge.StandardText);
        Assert.True(TodoBadge.StandardText.Length > 10);
    }

    [Fact]
    [Trait("TestId", "F05-001")]
    public void Selecting_a_module_again_reuses_its_cached_control()
    {
        var workspace = new InMemoryWorkspaceSession();
        var firstModule = new TestModule("M01", 1);
        var secondModule = new TestModule("M02", 2);
        var viewModel = new AppShellViewModel([firstModule, secondModule], workspace, workspace);

        viewModel.Select(viewModel.Modules[0]);
        var firstView = viewModel.CurrentView;
        viewModel.Select(viewModel.Modules[1]);
        viewModel.Select(viewModel.Modules[0]);

        Assert.Same(firstView, viewModel.CurrentView);
        Assert.Equal(1, firstModule.CreateCount);
    }

    [Fact]
    [Trait("TestId", "F05-002")]
    public void Workspace_navigation_updates_the_shell_selection()
    {
        var workspace = new InMemoryWorkspaceSession();
        var viewModel = new AppShellViewModel([new TestModule("M01", 1), new TestModule("M02", 2)], workspace, workspace);

        workspace.NavigateTo("M02");

        Assert.Equal("M02", viewModel.CurrentModule!.Id);
    }

    [Fact]
    [Trait("TestId", "F05-003")]
    public void Composition_provides_all_twelve_stably_ordered_modules()
    {
        var viewModel = DesktopComposition.CreateShellViewModel();

        Assert.Equal(Enumerable.Range(1, 12).Select(number => $"M{number:00}"), viewModel.Modules.Select(module => module.Id));
    }

    private sealed class TestModule(string id, int order) : IDesktopModule
    {
        public int CreateCount { get; private set; }

        public DesktopModuleMetadata Metadata { get; } = new(id, id, "Test", order);

        public Func<Control> CreateView => () =>
        {
            CreateCount++;
            return new TextBlock();
        };
    }
}
