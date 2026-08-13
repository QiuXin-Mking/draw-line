using Avalonia.Controls;
using LeatherNesting.Desktop.Modules.Contracts;

namespace LeatherNesting.Desktop.Modules.ProcessFeatures;

/// <summary>M05: process-feature and grading-rule demo module.</summary>
public sealed class ProcessFeaturesModule : IDesktopModule
{
    public DesktopModuleMetadata Metadata { get; } = new("M05", "工艺特征", "CAD 工作台", 5);

    public Func<Control> CreateView => () => new ProcessFeaturesView();
}
