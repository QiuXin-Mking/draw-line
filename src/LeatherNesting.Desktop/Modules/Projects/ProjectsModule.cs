using Avalonia.Controls;
using LeatherNesting.Desktop.Demo;
using LeatherNesting.Desktop.Modules.Contracts;

namespace LeatherNesting.Desktop.Modules.Projects;

/// <summary>M01 definition discovered from the module's owning directory.</summary>
public sealed class ProjectsModule : IDesktopModule
{
    public DesktopModuleMetadata Metadata { get; } = new("M01", "项目与订单", "项目", 1);

    public Func<Control> CreateView { get; } = static () =>
        new ProjectsView(new ProjectsViewModel(DemoScenarioFactory.Projects));
}
