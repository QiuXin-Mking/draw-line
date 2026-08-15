namespace LeatherNesting.Desktop.Modules.CadCanvas.Toolbar;

/// <summary>
/// Routes stable CAD command keys to the shared host and keeps toolbar presentation in sync.
/// </summary>
public sealed class CadToolbarController
{
    private readonly CadHostState _host;
    private readonly Action _refit;

    public CadToolbarController(CadHostState host, Action refit)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(refit);

        _host = host;
        _refit = refit;
        State = new CadToolbarState();
        View = new CadToolbarView(tool => TryExecute(tool.CommandKey));
        State.Changed += (_, _) => ProjectState();
        ProjectState();
    }

    public CadToolbarState State { get; }

    public CadToolbarView View { get; }

    public bool TryExecute(CadToolCommandKey command)
    {
        var definition = CadToolCatalog.All.SingleOrDefault(tool => tool.CommandKey == command)
            ?? throw new ArgumentOutOfRangeException(nameof(command), command, "Unknown CAD tool command key.");

        if (!State.TryExecute(command))
        {
            return false;
        }

        if (command == CadToolCommandKey.Refit)
        {
            _refit();
        }
        else if (definition.ImplementationState == CadToolImplementationState.Todo)
        {
            _host.ReportUnsupported(definition.Label);
        }

        return true;
    }

    public bool HandleEscape() => TryExecute(CadToolCommandKey.Cancel);

    private void ProjectState()
    {
        View.SetVisibleKeys(State.VisibleTools.Select(tool => tool.CommandKey));
        View.SetActiveKey(State.ActiveTool);
        View.SetEnabledKeys(CadToolCatalog.All
            .Where(tool => State.CanExecute(tool.CommandKey))
            .Select(tool => tool.CommandKey));
    }
}
