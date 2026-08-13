using Avalonia.Controls;

namespace LeatherNesting.Desktop.Shell;

/// <summary>Describes one of the 12 navigable modules of the demo shell.</summary>
public sealed record ModuleDescriptor(
    string Id,
    string Title,
    string Group,
    bool HasRealLogic,
    Func<Control> CreateView);
