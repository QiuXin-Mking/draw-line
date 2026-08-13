using Avalonia.Controls;

namespace LeatherNesting.Desktop.Modules.Contracts;

/// <summary>
/// Declares a desktop module that can be discovered and displayed by the shell.
/// Concrete modules own their definition; the composition root supplies any view dependencies.
/// </summary>
public interface IDesktopModule
{
    DesktopModuleMetadata Metadata { get; }

    Func<Control> CreateView { get; }
}
