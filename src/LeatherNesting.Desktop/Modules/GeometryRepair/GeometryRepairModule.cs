using Avalonia.Controls;
using LeatherNesting.Desktop.Modules.Contracts;

namespace LeatherNesting.Desktop.Modules.GeometryRepair;

/// <summary>M04: contour diagnostics and geometry-repair workbench.</summary>
public sealed class GeometryRepairModule : IDesktopModule
{
    public DesktopModuleMetadata Metadata { get; } = new("M04", "几何修复", "CAD 工作台", 4);

    public Func<Control> CreateView => () => new GeometryRepairView();
}
