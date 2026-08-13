using Avalonia.Controls;
using LeatherNesting.Desktop.Modules.Contracts;

namespace LeatherNesting.Desktop.Modules.Validation;

/// <summary>M10 definition discovered from its owning module directory.</summary>
public sealed class ValidationModule : IDesktopModule
{
    public DesktopModuleMetadata Metadata { get; } = new("M10", "校验", "排样", 10);

    public Func<Control> CreateView { get; } = static () => new ValidationView();
}
