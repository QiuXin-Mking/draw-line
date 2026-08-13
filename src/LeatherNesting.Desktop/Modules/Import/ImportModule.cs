using Avalonia.Controls;
using LeatherNesting.Desktop.Modules.Contracts;

namespace LeatherNesting.Desktop.Modules.Import;

/// <summary>M02 definition discovered from its owning directory while dependencies remain composition-owned.</summary>
public sealed class ImportModule : IDesktopModule
{
    private static IImportCoordinator? coordinator;

    public DesktopModuleMetadata Metadata { get; } = new("M02", "DXF 导入", "项目", 2);

    public Func<Control> CreateView => CreateImportView;

    internal static void BindCoordinator(IImportCoordinator value) =>
        coordinator = value ?? throw new ArgumentNullException(nameof(value));

    private static Control CreateImportView() => new ImportView(
        coordinator ?? throw new InvalidOperationException("M02 Import coordinator must be configured by Desktop composition before creating its view."));
}
