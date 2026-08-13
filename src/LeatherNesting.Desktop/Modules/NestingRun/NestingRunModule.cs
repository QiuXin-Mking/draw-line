using Avalonia.Controls;
using LeatherNesting.Desktop.Modules.Contracts;

namespace LeatherNesting.Desktop.Modules.NestingRun;

/// <summary>M08 module definition, discovered directly by the desktop shell.</summary>
public sealed class NestingRunModule : IDesktopModule
{
    public DesktopModuleMetadata Metadata { get; } = new("M08", "排样运行", "排样", 8);

    public Func<Control> CreateView { get; } = static () => new NestingRunView();
}
