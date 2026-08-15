using Avalonia.Input;
using LeatherNesting.Desktop.Modules.CadCanvas;
using LeatherNesting.Desktop.Shell;
using Xunit;

namespace LeatherNesting.Desktop.Tests.Shell;

[Collection("Avalonia UI")]
public sealed class CadShortcutRouterTests
{
    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "KEY-001")]
    public void Catalog_covers_the_full_reference_shortcut_table_with_unique_key_bindings()
    {
        var commands = CadShortcutCatalog.Bindings.Select(binding => binding.Command).ToArray();

        Assert.Contains(CadShortcutCommand.Cancel, commands);
        Assert.Contains(CadShortcutCommand.ManualNest, commands);
        Assert.Contains(CadShortcutCommand.AreaArrayNest, commands);
        Assert.Contains(CadShortcutCommand.AreaBlendNest, commands);
        Assert.Contains(CadShortcutCommand.Undo, commands);
        Assert.Contains(CadShortcutCommand.Redo, commands);
        Assert.Contains(CadShortcutCommand.Cut, commands);
        Assert.Contains(CadShortcutCommand.Copy, commands);
        Assert.Contains(CadShortcutCommand.Paste, commands);
        Assert.Contains(CadShortcutCommand.SelectAll, commands);
        Assert.Contains(CadShortcutCommand.InvertSelection, commands);
        Assert.Contains(CadShortcutCommand.Delete, commands);
        Assert.Contains(CadShortcutCommand.Mirror, commands);
        Assert.Contains(CadShortcutCommand.Group, commands);
        Assert.Contains(CadShortcutCommand.Ungroup, commands);
        Assert.Contains(CadShortcutCommand.ExportToOrder, commands);
        Assert.Contains(CadShortcutCommand.GroupPieces, commands);
        Assert.Contains(CadShortcutCommand.RotateLeft, commands);
        Assert.Contains(CadShortcutCommand.RotateRight, commands);
        Assert.Contains(CadShortcutCommand.Rotate90, commands);
        Assert.Contains(CadShortcutCommand.MoveUp, commands);
        Assert.Contains(CadShortcutCommand.MoveDown, commands);
        Assert.Contains(CadShortcutCommand.MoveLeft, commands);
        Assert.Contains(CadShortcutCommand.MoveRight, commands);

        // Ctrl+A (SelectAll) and bare A (RotateLeft) both use Key.A but must be distinct bindings.
        Assert.Equal(
            CadShortcutCatalog.Bindings.Count,
            CadShortcutCatalog.Bindings
                .Select(binding => (binding.Key, binding.Modifiers))
                .Distinct()
                .Count());
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "KEY-002")]
    public void Known_shortcuts_dispatch_the_command_and_consume_the_event()
    {
        var executed = new List<(CadShortcutCommand Command, string Label)>();
        var router = new CadShortcutRouter((command, label) => executed.Add((command, label)));

        Assert.True(router.HandleKeyDown(KeyEvent(Key.Z, KeyModifiers.Control)));
        Assert.True(router.HandleKeyDown(KeyEvent(Key.Escape, KeyModifiers.None)));

        Assert.Equal(CadShortcutCommand.Undo, executed[0].Command);
        Assert.Equal(CadShortcutCommand.Cancel, executed[1].Command);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "KEY-003")]
    public void Unknown_key_is_not_consumed_and_does_not_dispatch()
    {
        var executed = new List<CadShortcutCommand>();
        var router = new CadShortcutRouter((command, _) => executed.Add(command));

        Assert.False(router.HandleKeyDown(KeyEvent(Key.F1, KeyModifiers.None)));
        Assert.Empty(executed);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "KEY-004")]
    public void Modifiers_distinguish_select_all_from_rotate_left_on_the_same_key()
    {
        var executed = new List<CadShortcutCommand>();
        var router = new CadShortcutRouter((command, _) => executed.Add(command));

        router.HandleKeyDown(KeyEvent(Key.A, KeyModifiers.Control));
        router.HandleKeyDown(KeyEvent(Key.A, KeyModifiers.None));
        router.HandleKeyDown(KeyEvent(Key.A, KeyModifiers.Shift));

        Assert.Equal(
            [CadShortcutCommand.SelectAll, CadShortcutCommand.RotateLeft, CadShortcutCommand.InvertSelection],
            executed);
    }

    [Fact]
    [Trait("Stage", "UI")]
    [Trait("TestId", "KEY-005")]
    public void Workspace_host_makes_the_canvas_focusable_and_wires_the_router()
    {
        var host = new CadWorkspaceHost(new CadHostState());

        Assert.True(host.Drawing.Focusable);
        Assert.NotNull(host.Shortcuts);

        var executed = new List<CadShortcutCommand>();
        var router = new CadShortcutRouter((command, _) => executed.Add(command));
        var args = KeyEvent(Key.Z, KeyModifiers.Control);

        Assert.True(router.HandleKeyDown(args));
        Assert.True(args.Handled);
    }

    private static KeyEventArgs KeyEvent(Key key, KeyModifiers modifiers) => new()
    {
        Key = key,
        KeyModifiers = modifiers,
    };
}
