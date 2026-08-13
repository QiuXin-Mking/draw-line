using Avalonia.Controls;
using LeatherNesting.Desktop.Modules.Contracts;

namespace LeatherNesting.Desktop.Modules.Pieces;

/// <summary>M06 module definition, discovered directly by the desktop shell.</summary>
public sealed class PiecesModule : IDesktopModule
{
    public DesktopModuleMetadata Metadata { get; } = new("M06", "裁片", "数据", 6);
    public Func<Control> CreateView { get; } = static () => new PiecesView();
}
