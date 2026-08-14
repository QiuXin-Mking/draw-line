# Technical Design

## Approach decision

Use one evidence-locked palette in `AppTheme`, then migrate the fixed cloned shell and cloned dialogs to semantic brush roles. This is preferred over per-component sampling because repeated near-colors caused the current green/warm drift. A modernized palette is rejected because it conflicts with the parent 1:1 decision.

## Palette boundaries

- Chrome tokens: title, menu, toolbar, panel, header, status, border, primary/muted/disabled text.
- Interaction tokens: toolbar hover, classic focus, selection, warning, danger.
- Workstation tokens: piece-card cyan, progress cyan, canvas black, ruler charcoal/foreground.
- Geometry semantic tokens: material boundary red, outer contour white, internal line green, selection fill. These remain separate from chrome accents.

Exact target values begin with the PRD table. Values may change only when a native-size screenshot comparison demonstrates compression or crop bias; every change must be recorded in the fidelity ledger.

## Component migration

1. Replace the current biased `AppTheme` brushes with semantic evidence tokens while retaining compatibility aliases where untouched modules depend on old names.
2. Update `TopCommandArea`, `AppShellView`, `ClassicPaneHost`, CAD hosts, order/piece panels and property panes to consume semantic roles.
3. Replace raw chrome colors inside cloned components. Do not mechanically replace geometry colors or legacy-module local themes outside the visible fixed workstation.
4. Future modal dialogs consume the same panel, header, border, text, focus and disabled roles.

## Verification design

- Contract tests assert exact core RGB values and reject the old biased values.
- Component tests assert repeated surfaces share the same brush instances/values.
- Run all tests and build.
- Launch the Avalonia shell at 1366×768, capture it with the screenshot skill, inspect with `view_image`, and compare against the corresponding cropped reference.
- Maintain a fidelity ledger with evidence color, rendered color, mismatch and repair for at least five surface groups.

## Compatibility and rollback

- No data/state/API contracts change.
- Compatibility aliases keep non-clone modules compiling while clone surfaces move to explicit semantic names.
- Rollback is limited to the theme and clone-surface changes; geometry rendering and business state remain untouched.
