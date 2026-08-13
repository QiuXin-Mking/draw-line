using Avalonia.Controls;
using LeatherNesting.Desktop.Modules.Contracts;

namespace LeatherNesting.Desktop.Modules.Materials;

/// <summary>M07 declaration kept beside its view so the shell can discover it without a registration edit.</summary>
public sealed class MaterialsModule : IDesktopModule
{
    public DesktopModuleMetadata Metadata { get; } = new("M07", "材料", "数据", 7);

    public Func<Control> CreateView => static () => new MaterialsView();
}
