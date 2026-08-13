using Avalonia.Controls;
using LeatherNesting.Desktop.Composition;
using LeatherNesting.Desktop.Modules.Import;
using LeatherNesting.Desktop.Modules.Contracts;
using LeatherNesting.Desktop.Shell;
using Xunit;

namespace LeatherNesting.Desktop.Tests;

public sealed class UiDemoIntegrationTests
{
    private static readonly string[] ExpectedModuleIds =
        Enumerable.Range(1, 12).Select(number => $"M{number:00}").ToArray();

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T13-001")]
    public void Composition_discovers_exactly_M01_through_M12_in_stable_order()
    {
        var shell = DesktopComposition.CreateShellViewModel();

        Assert.Equal(ExpectedModuleIds, shell.Modules.Select(module => module.Id));
        Assert.Equal(12, shell.Modules.Select(module => module.Id).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T13-002")]
    public void Every_discovered_module_has_its_own_view_factory_without_compatibility_placeholders()
    {
        var modules = DesktopModuleDiscovery.Discover(typeof(DesktopComposition).Assembly);

        Assert.Equal(ExpectedModuleIds, modules.Select(module => module.Metadata.Id));
        Assert.All(modules, module => Assert.NotNull(module.CreateView));
        Assert.All(modules, module => Assert.EndsWith("Module", module.GetType().Name, StringComparison.Ordinal));
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T13-003")]
    public void Navigating_away_and_back_preserves_every_module_control_instance()
    {
        var workspace = DesktopComposition.CreateWorkspace(LeatherNesting.Desktop.Demo.DemoScenarioFactory.Summary);
        var modules = DesktopModuleDiscovery.Discover(typeof(DesktopComposition).Assembly)
            .Select(module => new CacheProbeModule(module.Metadata))
            .Cast<IDesktopModule>()
            .ToArray();
        var shell = new AppShellViewModel(modules, workspace, workspace);
        var firstPass = new Dictionary<string, Control>(StringComparer.Ordinal);

        foreach (var module in shell.Modules)
        {
            shell.Select(module);
            firstPass.Add(module.Id, shell.CurrentView!);
        }

        foreach (var module in shell.Modules.Reverse())
        {
            shell.Select(module);
            Assert.Same(firstPass[module.Id], shell.CurrentView);
        }

        Assert.All(modules.Cast<CacheProbeModule>(), module => Assert.Equal(1, module.CreateCount));
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T13-004")]
    public void M02_keeps_the_real_import_module_factory_entry()
    {
        var shell = DesktopComposition.CreateShellViewModel();
        var module = DesktopModuleDiscovery.Discover(typeof(DesktopComposition).Assembly)
            .Single(module => module.Metadata.Id == "M02");

        Assert.True(shell.Modules.Single(item => item.Id == "M02").HasRealLogic);
        Assert.IsType<ImportModule>(module);
        Assert.NotNull(module.CreateView);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "T13-005")]
    public void Main_window_source_declares_the_1366_by_768_acceptance_viewport()
    {
        var sourcePath = Path.Combine(
            FindRepositoryRoot(),
            "src",
            "LeatherNesting.Desktop",
            "Views",
            "MainWindow.cs");
        var source = File.ReadAllText(sourcePath);

        Assert.Contains("Width = 1366;", source, StringComparison.Ordinal);
        Assert.Contains("Height = 768;", source, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "LeatherNesting.sln")))
            directory = directory.Parent;

        return directory?.FullName ?? throw new InvalidOperationException("Could not locate the repository root from the test output directory.");
    }

    private sealed class CacheProbeModule(DesktopModuleMetadata metadata) : IDesktopModule
    {
        public int CreateCount { get; private set; }

        public DesktopModuleMetadata Metadata { get; } = metadata;

        public Func<Control> CreateView => () =>
        {
            CreateCount++;
            return new Border();
        };
    }
}
