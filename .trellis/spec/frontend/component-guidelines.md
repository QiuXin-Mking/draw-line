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

<!-- How styles are applied (CSS modules, styled-components, Tailwind, etc.) -->

(To be filled by the team)

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
