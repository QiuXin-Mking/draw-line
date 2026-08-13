using LeatherNesting.Desktop.Modules;
using LeatherNesting.Desktop.Modules.Import;

namespace LeatherNesting.Desktop.Shell;

/// <summary>Navigation state for the demo shell: the 12 modules and the currently selected one.</summary>
public sealed class AppShellViewModel
{
    public IReadOnlyList<ModuleDescriptor> Modules { get; } = CreateModules();
    public ModuleDescriptor? CurrentModule { get; set; }

    public void Select(ModuleDescriptor module) => CurrentModule = module;

    private static IReadOnlyList<ModuleDescriptor> CreateModules() =>
    [
        new("M01", "项目与订单", "项目", false, () => new ModulePlaceholderView("M01", "项目与订单")),
        new("M02", "DXF 导入", "项目", true, () => new ImportView()),
        new("M03", "CAD 画布", "CAD 工作台", false, () => new ModulePlaceholderView("M03", "CAD 画布")),
        new("M04", "几何修复", "CAD 工作台", false, () => new ModulePlaceholderView("M04", "几何修复")),
        new("M05", "工艺特征", "CAD 工作台", false, () => new ModulePlaceholderView("M05", "工艺特征")),
        new("M06", "裁片", "数据", false, () => new ModulePlaceholderView("M06", "裁片")),
        new("M07", "材料", "数据", false, () => new ModulePlaceholderView("M07", "材料")),
        new("M08", "排样运行", "排样", false, () => new ModulePlaceholderView("M08", "排样运行")),
        new("M09", "排样复核", "排样", false, () => new ModulePlaceholderView("M09", "排样复核")),
        new("M10", "校验", "排样", false, () => new ModulePlaceholderView("M10", "校验")),
        new("M11", "导出", "输出", false, () => new ModulePlaceholderView("M11", "导出")),
        new("M12", "管理", "管理", false, () => new ModulePlaceholderView("M12", "管理")),
    ];
}
