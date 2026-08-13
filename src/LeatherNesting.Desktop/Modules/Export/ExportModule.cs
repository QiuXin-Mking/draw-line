using Avalonia.Controls;
using LeatherNesting.Desktop.Modules.Contracts;

namespace LeatherNesting.Desktop.Modules.Export;

/// <summary>M11 definition discovered directly from its owning module directory.</summary>
public sealed class ExportModule : IDesktopModule
{
    public DesktopModuleMetadata Metadata { get; } = new("M11", "导出", "输出", 11);

    public Func<Control> CreateView { get; } = static () => new ExportView();
}
