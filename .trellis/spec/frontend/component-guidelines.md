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
