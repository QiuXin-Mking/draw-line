using Avalonia.Controls;
using LeatherNesting.Desktop.Modules.Contracts;

namespace LeatherNesting.Desktop.Modules.Administration;

/// <summary>M12 declaration kept beside its view for assembly discovery.</summary>
public sealed class AdministrationModule : IDesktopModule
{
    public DesktopModuleMetadata Metadata { get; } = new("M12", "管理", "管理", 12);

    public Func<Control> CreateView => static () => new AdministrationView();
}
