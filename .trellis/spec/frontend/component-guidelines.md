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
