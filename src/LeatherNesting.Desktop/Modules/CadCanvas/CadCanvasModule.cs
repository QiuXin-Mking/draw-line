using Avalonia.Controls;
using LeatherNesting.Desktop.Modules.Contracts;

namespace LeatherNesting.Desktop.Modules.CadCanvas;

/// <summary>M03 declaration kept beside its view for assembly discovery.</summary>
public sealed class CadCanvasModule : IDesktopModule
{
    public DesktopModuleMetadata Metadata { get; } = new("M03", "CAD 画布", "CAD 工作台", 3);

    public Func<Control> CreateView => static () => new CadCanvasView();
}
