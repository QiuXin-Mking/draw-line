using System.Reflection;
using Avalonia.Controls;
using LeatherNesting.Application;
using LeatherNesting.Desktop.Adapters.Import;
using LeatherNesting.Desktop.Demo;
using LeatherNesting.Desktop.Modules;
using LeatherNesting.Desktop.Modules.Contracts;
using LeatherNesting.Desktop.Modules.Import;
using LeatherNesting.Desktop.Modules.Projects;
using LeatherNesting.Desktop.Shell;
using LeatherNesting.Desktop.Workspace;
using LeatherNesting.Infrastructure.Dxf;
using LeatherNesting.Infrastructure.Projects;

namespace LeatherNesting.Desktop.Composition;

/// <summary>Single composition root for desktop services and module factories.</summary>
public static class DesktopComposition
{
    public static AppShellViewModel CreateShellViewModel()
    {
        var workspace = CreateWorkspace(DemoScenarioFactory.Summary);
        return new AppShellViewModel(CreateModules(workspace, workspace), workspace, workspace);
    }

    public static InMemoryWorkspaceSession CreateWorkspace(IDemoProjectSummaryProvider demo) =>
        new(new WorkspaceSnapshot(
            new WorkspaceProjectSummary(demo.Summary.ProjectNumber, demo.Summary.ProjectName, demo.Summary.ProjectNumber, demo.Summary.Status),
            null,
            null,
            $"{DemoScenario.DemoMarker} · 演示数据",
            null));

    public static IReadOnlyList<IDesktopModule> CreateModules(IWorkspaceSession workspace, IWorkspaceCommands commands)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(commands);

        var importCoordinator = CreateImportCoordinator(workspace, commands);
        return DesktopModuleDiscovery.CreateCatalog(
            typeof(DesktopComposition).Assembly,
            CreateCompatibilityModules(importCoordinator));
    }

    private static IImportCoordinator CreateImportCoordinator(IWorkspaceSession workspace, IWorkspaceCommands commands)
    {
        var geometryReader = new AsciiImportGeometryReader();
        return new ImportCoordinator(
            new ImportDxfUseCase(new AsciiDxfReader()),
            new ZipProjectStore(),
            geometryReader,
            workspace,
            commands,
            new CadImportWorkbenchFactory(geometryReader));
    }

    private static IEnumerable<IDesktopModule> CreateCompatibilityModules(IImportCoordinator importCoordinator) =>
    [
        CompatibilityModule.Create("M01", "项目与订单", "项目", 1, true, () => new ProjectsView()),
        CompatibilityModule.Create("M02", "DXF 导入", "项目", 2, true, () => new ImportView(importCoordinator)),
        CompatibilityModule.Create("M03", "CAD 画布", "CAD 工作台", 3, false),
        CompatibilityModule.Create("M04", "几何修复", "CAD 工作台", 4, false),
        CompatibilityModule.Create("M05", "工艺特征", "CAD 工作台", 5, false),
        CompatibilityModule.Create("M06", "裁片", "数据", 6, false),
        CompatibilityModule.Create("M07", "材料", "数据", 7, false),
        CompatibilityModule.Create("M08", "排样运行", "排样", 8, false),
        CompatibilityModule.Create("M09", "排样复核", "排样", 9, false),
        CompatibilityModule.Create("M10", "校验", "排样", 10, false),
        CompatibilityModule.Create("M11", "导出", "输出", 11, false),
        CompatibilityModule.Create("M12", "管理", "管理", 12, false),
    ];

    private sealed class CompatibilityModule : IDesktopModule
    {
        private CompatibilityModule(string id, string title, string group, int order, bool hasRealLogic, Func<Control> createView)
        {
            Metadata = new DesktopModuleMetadata(id, title, group, order);
            HasRealLogic = hasRealLogic;
            CreateView = createView;
        }

        public DesktopModuleMetadata Metadata { get; }

        public bool HasRealLogic { get; }

        public Func<Control> CreateView { get; }

        public static CompatibilityModule Create(string id, string title, string group, int order, bool hasRealLogic, Func<Control>? createView = null)
        {
            var factory = createView ?? new Func<Control>(() => new ModulePlaceholderView(id, title));
            return new(id, title, group, order, hasRealLogic, factory);
        }
    }
}
