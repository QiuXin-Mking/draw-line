using Avalonia.Controls;
using LeatherNesting.Desktop.Modules.Contracts;

namespace LeatherNesting.Desktop.Modules.NestingReview;

/// <summary>M09 module definition discovered by the desktop shell.</summary>
public sealed class NestingReviewModule : IDesktopModule
{
    public DesktopModuleMetadata Metadata { get; } = new("M09", "排样复核", "排样", 9);
    public Func<Control> CreateView { get; } = static () => new NestingReviewView();
}
