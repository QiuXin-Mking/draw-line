# Component Guidelines

> How components are built in this project.

---

## Overview

<!--
Document your project's component conventions here.

Questions to answer:
- What component patterns do you use?
- How are props defined?
- How do you handle composition?
- What accessibility standards apply?
-->

(To be filled by the team)

---

## Component Structure

<!-- Standard structure of a component file -->

(To be filled by the team)

---

## Props Conventions

<!-- How props should be defined and typed -->

(To be filled by the team)

---

## Styling Patterns

The fixed workstation and its classic modal dialogs use semantic brushes from
`AppTheme`. Screenshot-evidence colors are centralized there; cloned surfaces
must not introduce local near-match chrome colors.

Keep these role families separate even when two roles currently have the same
RGB value:

- Application chrome: title, menu, toolbar, panel, header, status, border, and text.
- Interaction state: hover, classic focus, selection, warning, danger, and disabled.
- Workstation surfaces: piece-card cyan, progress cyan, canvas black, and ruler chrome.
- CAD geometry: material boundary, outer contour, internal line, and selection fill.

Clone components consume the explicit semantic names such as `PanelSurface`,
`PieceCardCyan`, and `GeometryInternalLine`. Compatibility aliases such as
`ClassicPanelBackground` remain only so untouched legacy modules continue to
compile; new clone code must not depend on them.

```csharp
Background = AppTheme.PanelSurface;
BorderBrush = AppTheme.ClassicBorderNeutral;

// Geometry semantics stay independent from application chrome.
context.DrawGeometry(null, new Pen(AppTheme.GeometryInternalLine, 1), geometry);
```

Raw colors remain valid for evidenced per-item geometry or layer swatches whose
meaning is data-specific. They are forbidden for cloned window chrome, pane
surfaces, focus state, progress, rulers, or shared canvas semantics.

The fixed workstation requests the light Avalonia control theme explicitly.
Text placed on evidence-locked light surfaces also sets a semantic foreground;
do not rely on an operating-system theme to supply menu or status text colors.
This prevents macOS dark mode from producing white labels or dark default inputs
inside the cloned classic-light client.

Shared `AppTheme` palette brushes must use `ImmutableSolidColorBrush`, rather
than mutable dispatcher-owned `SolidColorBrush`. Palette contract tests use
`ISolidColorBrush` and join the Avalonia UI collection, so the full parallel
suite cannot read a mutable brush from another dispatcher thread.

---

## Form Field Layout

A form field's label (key) and input (value) sit on the **same horizontal line** — never stack the label above the editor. Fix the label column to a constant width and give every input the same width so the fields read as one vertically-aligned grid.

```csharp
private const double LabelWidth = 80;

private static Control Field(string label, Control editor, TextBlock? error = null)
{
    var caption = new TextBlock
    {
        Text = label,
        FontSize = 12,
        Foreground = AppTheme.PrimaryText,
        Width = LabelWidth,                       // 固定宽度 → 每行输入框起点对齐
        VerticalAlignment = VerticalAlignment.Center,
    };
    editor.VerticalAlignment = VerticalAlignment.Center;
    var line = new StackPanel
    {
        Orientation = Orientation.Horizontal,     // key 与 value 同一行
        Spacing = 8,
        Children = { caption, editor },
    };
    if (error is null) return line;
    return new StackPanel { Orientation = Orientation.Vertical, Spacing = 2, Children = { line, error } };
}
```

**Wrong** — vertical stack (label above input) misaligns and wastes vertical space:

```csharp
// Don't:
var panel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 };
panel.Children.Add(new TextBlock { Text = label });
panel.Children.Add(editor);
```

**Why**: a fixed label column keeps every input's left edge aligned; consistent input widths read as one grid. Error text goes below the line so it never shifts the key/value row.

---

## Accessibility

<!-- A11y requirements and patterns -->

(To be filled by the team)

---

## Common Mistakes

### Shell command shortcuts

Shell toolbar commands are navigation shortcuts, not alternate implementations of module behavior. Describe each shortcut with a label, icon key, target module ID, and placeholder flag; route activation through `AppShellViewModel.Select`. Actions that are not implemented must then publish the standard TODO notice instead of invoking or imitating a module state transition.

```csharp
new ShellToolbarCommand("开始排版", ToolbarIconKey.StartNesting, "M08", true);

Select(targetModule);
if (command.IsPlaceholderAction)
    ShowTodo(command.Label);
```

The one exception is a shell command that opens a modal dialog (e.g. 「新建排版」→「版型设置」). Mark it with `Launch = ShellCommandLaunch.NewBoardSettings` and drop the placeholder flag, then raise a dedicated `AppShellViewModel` event that the shell View subscribes to and renders via `ShowDialog(owner)`. The ViewModel must not open the window itself: showing a dialog needs a `Window` owner, which only the View layer (`TopLevel.GetTopLevel(this)`) can supply.

```csharp
new ShellToolbarCommand("新建排版", ToolbarIconKey.NewLayout, "M01", false,
    Launch: ShellCommandLaunch.NewBoardSettings);

// AppShellViewModel: if (command.Launch == ShellCommandLaunch.NewBoardSettings)
//     BoardSettingsRequested?.Invoke(this, EventArgs.Empty); // return, no navigation/TODO
// AppShellView: _viewModel.BoardSettingsRequested += (_, _) => OpenBoardSettings();
```

For cross-platform toolbar artwork, use Avalonia vector shapes or geometry. Do not use font glyphs or network-loaded bitmaps: installed fonts differ between Windows and macOS, and a missing glyph turns an important command into an unreadable placeholder.

Required tests assert command label/order, unique icon keys, target module IDs, icon-before-label composition, TODO behavior for placeholder actions, and horizontal access at narrow widths.

### Persistent workstation shell

The nesting workstation is a persistent multi-pane surface, not a router that replaces the whole body for each business capability. The shell owns stable host regions; modules contribute content to those hosts without embedding a second complete page, toolbar, inspector, status bar, or ruler set.

```csharp
// Shell geometry remains stable while child content changes.
BodyColumns = "13*,74*,13*";
LeftRows = "20*,60*,20*";
RightRows = "62*,38*";
```

Wrong: placing a complete `CadCanvasView` (including its own surrounding UI) inside the center canvas host. Correct: keep the current module selected/cached, and bind only its canvas/content contract into the shell's existing center surface.

Tests must assert host count/order and prevent nested scroll viewers, duplicate rulers, duplicate toolbars, or full module pages in the center canvas.

### Collapsible side rails: persistent edge chrome lives outside the body geometry

When a side rail (e.g. the left 订单组/裁片列表 column) needs a collapse-to-edge control, the trigger is persistent shell chrome: place it in the **outer layout grid** as a fixed `Auto` column, not inside the body's `13*,74*,13*` columns. This keeps the five-region body geometry and its tests stable.

Collapse must free the canvas width, so zero the body's left column **explicitly** — do not rely on Avalonia collapsing a star column to 0 when its content is hidden.

```csharp
// Outer grid: "Auto,*"  →  strip column 0 (row 1), bodyLayer column 1.
// BodyGrid stays "13*,74*,13*".
public void ToggleLeftRail()
{
    _leftRailCollapsed = !_leftRailCollapsed;
    LeftRail.IsVisible = !_leftRailCollapsed;
    LeftRailColumn.Width = _leftRailCollapsed ? new GridLength(0) : new GridLength(13, GridUnitType.Star);
}
```

Pitfall: structural tests that pin the outer grid (e.g. `Assert.Single(layout.ColumnDefinitions)`, `Assert.Equal(3, layout.Children.Count)`) must be updated to the new column/child count when persistent chrome is added. Assert the new structure explicitly (Auto strip + Star workspace) rather than deleting the guard.

### Code-built menus: use `ItemsSource`, not `Items.Add`

When building `Menu` / `ContextMenu` in code, populate the item collection via
`ItemsSource = items` on a pre-built `IEnumerable`. Do not call `Items.Add(...)`
per item: `ItemCollection.Add` runs an Avalonia dispatcher `VerifyAccess`, which
throws `InvalidOperationException` ("different thread owns it") in headless tests
that construct the control outside the UI-thread collection. `ItemsSource` avoids
that path. Existing menu construction mirrors `TopCommandArea.CreateCommandItem`.

```csharp
// Correct: one ItemsSource assignment, dispatcher-safe in tests.
var items = ShellContextMenu.Entries.OfType<ShellMenuCommand>()
    .Select(command => new MenuItem { Header = command.Label, IsEnabled = command.IsEnabled });
return new ContextMenu { ItemsSource = items };

// Wrong: Items.Add throws in non-UI-thread test construction.
var menu = new ContextMenu();
menu.Items.Add(new MenuItem { Header = "…" });
```

Tests must assert item label/order and disabled states off the source contract
plus the host's materialized `MenuItems` / `ContextMenu`.

### Headless tests: `PointerEventArgs.GetPosition` returns the origin

In Avalonia headless tests (no visual root mounted), `PointerEventArgs.GetPosition`
returns `(0,0)` regardless of the position passed to the constructor, because it
resolves through `RootVisual.TranslatePoint`, which is a no-op without a mounted
root. Do not assert real coordinates by constructing a `PointerMovedEventArgs` and
reading `GetPosition`.

Correct pattern: extract the coordinate formatting into a testable seam on the
host (e.g. `UpdateCoordinates(Point2D)`) and assert that seam directly; keep the
pointer handler as a thin `GetPosition → seam` bridge. Assert pointer-exit
clearing by raising the real routed event with the matching `RoutedEvent`
(`InputElement.PointerExitedEvent`, not `PointerMovedEvent`).

```csharp
// Assert format via the seam, not via a raised pointer event.
host.UpdateCoordinates(new Point2D(5.5, -2.5));
Assert.Equal("X 5.50 mm · Y -2.50 mm", host.CoordinateText);
```

Also note Avalonia 12 constructor signatures changed: `Pointer(int, PointerType, bool)`,
`PointerEventArgs(RoutedEvent, object, IPointer, Visual?, Point, ulong, PointerPointProperties, KeyModifiers)`,
and `KeyModifiers` (not `InputModifiers`). Probe via reflection before trusting older examples.

### Avalonia `Key` enum names differ from user-visible labels

Avalonia's `Key` enum uses full names, not the shortcut labels: `Esc` → `Key.Escape`,
`Enter` → `Key.Return`, `Del` → `Key.Delete`. Function keys and arrows match
(`F5`, `Up`, `Space`). When modeling a shortcut table, probe `Enum.Parse<Key>` first
or write it against the enum directly; a bare `"Esc"` will not resolve.

For a single source of truth, keep the mapping contract (`Key` + `KeyModifiers` → command)
in one static catalog and drive menu labels, routing, and tests from it — as
`CadShortcutCatalog` does for the AXTNester §8.3 shortcut table. `KeyEventArgs` in
Avalonia 12 has a parameterless constructor with writable `Key` / `KeyModifiers`,
so unit tests can raise key events without mounting a window.
