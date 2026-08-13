# Technical Design

## Scope

Refactor `AppShellView` into the persistent five-pane workstation frame proven by image 27. The shell owns geometry and host contracts only; child panes initially use deterministic evidence-labelled content and are replaced by later children without changing layout.

## Layout Contract

- Rows: menu/large toolbar, body, status.
- Body columns: compact left rail (~13%), flexible black center canvas (~74%), compact right rail (~13%).
- Left rows: order/group host, piece-list host, progress-summary host.
- Right rows: layout-candidate host and output-information host.
- Top command labels/order reuse the completed toolbar contract; product brand replaces competitor identity.

Expose named host controls and a deterministic reference-frame model so tests and later children bind to stable regions. Preserve `AppShellViewModel.Select` caching and Workspace snapshot behavior behind the shell.

## Visual Strategy

Use explicit Avalonia dimensions, one-pixel borders, compact typography and classic Windows colors derived from the evidence. Avoid framework-default card margins. The central placeholder is a real black ruler canvas surface, not a modern dashboard page.

## Validation and Rollback

Contract tests assert tree geometry, region order, ratios, labels, default CAD selection and stable hosts. Existing module discovery/import tests must pass. Changes remain within Shell/DesignSystem and Shell tests, so the prior shell can be reverted independently.
