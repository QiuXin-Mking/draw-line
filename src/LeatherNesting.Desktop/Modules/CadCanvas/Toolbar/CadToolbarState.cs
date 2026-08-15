namespace LeatherNesting.Desktop.Modules.CadCanvas.Toolbar;

/// <summary>Pure interaction state for the contextual CAD toolbar.</summary>
public sealed class CadToolbarState
{
    public CadToolbarMode Mode { get; private set; } = CadToolbarMode.CadEdit;

    public CadToolCommandKey ActiveTool { get; private set; } = CadToolCommandKey.Select;

    public bool HasUndo { get; private set; }

    public bool HasRedo { get; private set; }

    public bool HasSelection { get; private set; }

    public bool HasPendingStep { get; private set; }

    public IReadOnlyList<CadToolDefinition> VisibleTools => CadToolCatalog.All
        .Where(tool => tool.SupportedModes.HasFlag(Mode))
        .ToArray();

    public event EventHandler? Changed;

    public void SetMode(CadToolbarMode mode)
    {
        if (mode is not (CadToolbarMode.CadEdit or CadToolbarMode.NestingReview))
        {
            throw new ArgumentOutOfRangeException(nameof(mode), mode, "A single supported toolbar mode is required.");
        }

        if (Mode == mode)
        {
            return;
        }

        Mode = mode;
        if (!IsVisible(ActiveTool))
        {
            ActiveTool = CadToolCommandKey.Select;
            HasPendingStep = false;
        }

        OnChanged();
    }

    public void SetAvailability(bool hasUndo, bool hasRedo, bool hasSelection)
    {
        if (HasUndo == hasUndo && HasRedo == hasRedo && HasSelection == hasSelection)
        {
            return;
        }

        HasUndo = hasUndo;
        HasRedo = hasRedo;
        HasSelection = hasSelection;
        OnChanged();
    }

    public void SetPendingStep(bool hasPendingStep)
    {
        if (HasPendingStep == hasPendingStep)
        {
            return;
        }

        HasPendingStep = hasPendingStep;
        OnChanged();
    }

    public bool CanExecute(CadToolCommandKey command)
    {
        if (!IsVisible(command))
        {
            return false;
        }

        return command switch
        {
            CadToolCommandKey.Undo => HasUndo,
            CadToolCommandKey.Redo => HasRedo,
            CadToolCommandKey.Delete => HasSelection,
            CadToolCommandKey.Cancel => HasPendingStep || ActiveTool != CadToolCommandKey.Select,
            _ => true,
        };
    }

    public bool TryExecute(CadToolCommandKey command)
    {
        if (!CanExecute(command))
        {
            return false;
        }

        if (command == CadToolCommandKey.Cancel)
        {
            CancelCurrentStepOrTool();
            return true;
        }

        if (IsMomentary(command))
        {
            return true;
        }

        if (ActiveTool == command && !HasPendingStep)
        {
            return true;
        }

        ActiveTool = command;
        HasPendingStep = false;
        OnChanged();
        return true;
    }

    public bool HandleEscape() => TryExecute(CadToolCommandKey.Cancel);

    private bool IsVisible(CadToolCommandKey command) => CadToolCatalog.All.Any(tool =>
        tool.CommandKey == command && tool.SupportedModes.HasFlag(Mode));

    private static bool IsMomentary(CadToolCommandKey command) => command is
        CadToolCommandKey.ExportToOrder or
        CadToolCommandKey.Refit or
        CadToolCommandKey.Undo or
        CadToolCommandKey.Redo or
        CadToolCommandKey.Delete or
        CadToolCommandKey.Settings;

    private void CancelCurrentStepOrTool()
    {
        if (HasPendingStep)
        {
            HasPendingStep = false;
        }
        else
        {
            ActiveTool = CadToolCommandKey.Select;
        }

        OnChanged();
    }

    private void OnChanged() => Changed?.Invoke(this, EventArgs.Empty);
}
