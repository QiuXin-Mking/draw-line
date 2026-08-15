using Avalonia.Input;

namespace LeatherNesting.Desktop.Shell;

/// <summary>Routes CAD canvas key presses to shortcut commands via the shared catalog.
/// Returns whether the key was consumed so the canvas can avoid swallowing unrelated keys.</summary>
public sealed class CadShortcutRouter
{
    private readonly Action<CadShortcutCommand, string> _execute;

    public CadShortcutRouter(Action<CadShortcutCommand, string> execute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    }

    /// <summary>Matches the pressed key against the catalog; on hit invokes the command and returns true.</summary>
    public bool HandleKeyDown(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        var binding = CadShortcutCatalog.Find(e.Key, e.KeyModifiers);
        if (binding is null)
            return false;

        _execute(binding.Command, binding.Label);
        e.Handled = true;
        return true;
    }
}
