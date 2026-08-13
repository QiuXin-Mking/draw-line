using Avalonia.Controls;
using LeatherNesting.Desktop.Composition;
using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Modules.Contracts;
using LeatherNesting.Desktop.Workspace;

namespace LeatherNesting.Desktop.Shell;

/// <summary>Navigation state for the demo shell: the 12 modules and the currently selected one.</summary>
public sealed class AppShellViewModel
{
    private readonly IWorkspaceSession _workspace;
    private readonly IWorkspaceCommands _commands;
    private readonly Dictionary<string, Control> _views = new(StringComparer.Ordinal);

    public AppShellViewModel()
    {
        var composed = DesktopComposition.CreateShellViewModel();
        Modules = composed.Modules;
        _workspace = composed._workspace;
        _commands = composed._commands;
        _workspace.SnapshotChanged += OnWorkspaceSnapshotChanged;
    }

    public AppShellViewModel(IEnumerable<IDesktopModule> modules, IWorkspaceSession workspace, IWorkspaceCommands commands)
    {
        ArgumentNullException.ThrowIfNull(modules);
        _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
        _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        Modules = DesktopModuleCatalog.CreateValidated(modules)
            .Select(module => new ModuleDescriptor(
                module.Metadata.Id,
                module.Metadata.Title,
                module.Metadata.Group,
                IsRealLogic(module),
                module.CreateView))
            .ToArray();
        _workspace.SnapshotChanged += OnWorkspaceSnapshotChanged;
    }

    public IReadOnlyList<ModuleDescriptor> Modules { get; }

    public ModuleDescriptor? CurrentModule { get; private set; }

    public Control? CurrentView { get; private set; }

    public WorkspaceSnapshot Snapshot => _workspace.Snapshot;

    public event EventHandler<WorkspaceSnapshot>? SnapshotChanged;

    public void Select(ModuleDescriptor module)
    {
        ArgumentNullException.ThrowIfNull(module);
        Show(module);
        if (!StringComparer.Ordinal.Equals(_workspace.Snapshot.ActiveModuleId, module.Id))
            _commands.NavigateTo(module.Id);
    }

    public void ShowTodo(string command)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        _commands.ShowTodo($"命令「{command}」{TodoBadge.StandardText}");
    }

    public void ActivateToolbarCommand(ShellToolbarCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        var module = Modules.SingleOrDefault(candidate =>
            StringComparer.Ordinal.Equals(candidate.Id, command.TargetModuleId));
        if (module is null)
            throw new InvalidOperationException($"Toolbar command '{command.Label}' targets missing module '{command.TargetModuleId}'.");

        Select(module);
        if (command.IsPlaceholderAction)
            ShowTodo(command.Label);
    }

    private void OnWorkspaceSnapshotChanged(object? sender, WorkspaceSnapshot snapshot)
    {
        var target = Modules.FirstOrDefault(module => StringComparer.Ordinal.Equals(module.Id, snapshot.ActiveModuleId));
        if (target is not null) Show(target);
        SnapshotChanged?.Invoke(this, snapshot);
    }

    private void Show(ModuleDescriptor module)
    {
        CurrentModule = module;
        if (!_views.TryGetValue(module.Id, out var view))
        {
            view = module.CreateView();
            _views.Add(module.Id, view);
        }
        CurrentView = view;
    }

    private static bool IsRealLogic(IDesktopModule module) => module.Metadata.Id == "M02";
}
