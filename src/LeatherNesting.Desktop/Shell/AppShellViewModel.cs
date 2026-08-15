using Avalonia.Controls;
using LeatherNesting.Desktop.Composition;
using LeatherNesting.Desktop.DesignSystem;
using LeatherNesting.Desktop.Modules.Contracts;
using LeatherNesting.Desktop.Modules.CadCanvas;
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
        CadHost = composed.CadHost;
        _workspace.SnapshotChanged += OnWorkspaceSnapshotChanged;
    }

    public AppShellViewModel(
        IEnumerable<IDesktopModule> modules,
        IWorkspaceSession workspace,
        IWorkspaceCommands commands,
        CadHostState? cadHost = null)
    {
        ArgumentNullException.ThrowIfNull(modules);
        _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
        _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        CadHost = cadHost ?? new CadHostState();
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

    public CadHostState CadHost { get; }

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

    public void ActivateMenuCommand(ShellMenuCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.NavigateToModule)
        {
            var module = Modules.SingleOrDefault(candidate =>
                StringComparer.Ordinal.Equals(candidate.Id, command.TargetModuleId));
            if (module is null)
                throw new InvalidOperationException($"Menu command '{command.Label}' targets missing module '{command.TargetModuleId}'.");

            Select(module);
        }

        if (command.IsPlaceholderAction)
            ShowTodo(command.Label);
    }

    /// <summary>CAD 右键菜单命令激活入口（接线位）。
    /// 本任务全部命令统一走占位 TODO（路由 M03 + 状态栏提示），不伪造成功。
    /// 后续接线映射（替换本方法体即可，无需改动 View 层）：
    ///   撤销 → Workbench.Undo()
    ///   返回 → Workbench.Redo()
    ///   取消 → Workbench.Cancel()（预览中）/ ClearSelection()（选中后）
    ///   移动 → Workbench.MoveSelected(delta)
    ///   旋转 → Workbench.RotateSelected(degrees)</summary>
    public void ActivateContextCommand(ShellMenuCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        ActivateMenuCommand(command);
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
