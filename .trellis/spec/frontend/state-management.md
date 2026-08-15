# State Management

> How state is managed in this project.

---

## Overview

<!--
Document your project's state management conventions here.

Questions to answer:
- What state management solution do you use?
- How is local vs global state decided?
- How do you handle server state?
- What are the patterns for derived state?
-->

(To be filled by the team)

---

## State Categories

<!-- Local state, global state, server state, URL state -->

(To be filled by the team)

---

## When to Use Global State

<!-- Criteria for promoting state to global -->

(To be filled by the team)

---

## Server State

<!-- How server data is cached and synchronized -->

(To be filled by the team)

---

## Common Mistakes

## Scenario: Confirmed DXF geometry and editing in the fixed CAD host

### 1. Scope / Trigger

- Trigger: M02 imports geometry through the application/infrastructure boundary, while the persistent desktop shell renders and edits that geometry without replacing its five-pane layout.
- The composition root creates one `CadHostState` and passes the same instance to `ImportCoordinator` and `AppShellViewModel`. That host owns the only Desktop `CadWorkbenchViewModel` for the confirmed file.

### 2. Signatures

```csharp
Task ImportCoordinator.InspectAsync(string path, CancellationToken cancellationToken);
void ImportCoordinator.ConfirmMillimetres();
void ImportCoordinator.CancelImport();
void CadHostState.LoadConfirmedImport(string path, IReadOnlyList<Loop2D> loops);
void CadHostState.ReportError(string message);
void CadHostState.ReportUnsupported(string action);
void CadWorkbenchViewModel.SelectPiece(Point2D point);
void CadWorkbenchViewModel.MoveSelected(Point2D delta);
void CadWorkbenchViewModel.Commit();
void CadWorkbenchViewModel.Cancel();
void CadWorkbenchViewModel.Undo();
void CadWorkbenchViewModel.Redo();
```

### 3. Contracts

- `InspectAsync` prepares both the DXF import decision and the closed-loop projection, but does not publish either to the fixed canvas.
- `ConfirmMillimetres` commits `UnitDecision.ConfirmedMillimetres`, publishes the prepared loops once, clears pending preparation, and updates the workspace project.
- `CancelImport` clears pending inspection only; it must not replace or clear the currently displayed confirmed geometry.
- `LoadConfirmedImport` copies the external loop list once, loads that snapshot into `CadHostState.Workbench`, derives `FileName`, clears the demo flag, and raises host `Changed` once. `CadHostState.Loops` projects `Workbench.CurrentLoops`; it must not keep a second geometry collection.
- The shell-owned `CanvasView` and `CadPropertyPane` both consume the same `CadHostState.Workbench`. A module-local or view-local Workbench is not valid for the persistent host.
- A successful preview changes `CurrentLoops` but does not enter the undo stack. While `CanCommit` is true, pointer selection, drag movement, offset inputs, and every action except commit/cancel are disabled. This prevents a second preview from replacing the pending transaction.
- A drag transform is valid only when pointer press began inside the loop that was already selected. Pressing an unselected loop selects it; that gesture must not move the previously selected loop.
- `Commit` records the pending command in the session undo stack. It does not write `ProjectDocument` or an output DXF, and UI copy must say so.
- Unsupported draw/save/edit commands are disabled and carry the standard TODO tooltip. They must not look enabled and then merely change `StatusMessage`.
- Workbench changes flow outward through `CadWorkbenchViewModel.Changed` -> `CadHostState.Changed` -> canvas/property refresh. Notification suppression around `LoadLoops` uses `try/finally` so an exception cannot permanently silence the host.

### 4. Validation & Error Matrix

| Condition | Required behavior |
| --- | --- |
| Blank import path passed to `LoadConfirmedImport` | Throw `ArgumentException`; preserve current host state |
| Null loop collection | Throw `ArgumentNullException`; preserve current host state |
| Confirm without a project or preparation | Throw `InvalidOperationException`; publish nothing |
| Inspection cancelled | Clear pending inspection; keep confirmed canvas geometry |
| Confirmed DXF has zero closed loops | Publish an empty snapshot and an explicit no-displayable-loop status |
| Blank `ReportError` / `ReportUnsupported` text | Throw `ArgumentException`; preserve geometry and transaction state |
| Preview requested with no session or no required selection | Publish a diagnostic; preserve geometry and transaction state |
| Second preview, select, or drag attempted while a preview is pending | Reject or disable the action; keep the original pending preview intact |
| Drag starts on an unselected loop while another loop is selected | Select the pressed loop only; do not move either loop in that gesture |
| Non-left pointer press or lost pointer capture | Do not select or transform; clear gesture state safely |
| Invalid/non-finite/zero offset input | Publish a validation error; do not invoke the offset command |
| Unsupported CAD action | Keep its control disabled with a TODO tooltip; preserve geometry |

### 5. Good/Base/Bad Cases

- Good: inspect a DXF, confirm millimetres, select an imported loop, preview an offset or drag, commit it to the CAD session, then undo it through the same host state.
- Base: confirm a valid DXF with zero closed loops, show the honest empty-result status, and keep editing controls disabled.
- Bad: publish geometry during inspection, create another Workbench for the fixed host, allow selection/drag during a pending preview, or present session commit as project persistence.

### 6. Tests Required

- Composition test: the importer and fixed shell observe the same `CadHostState` and Workbench instance.
- Import test: inspection alone leaves the fixed host unchanged; confirmation publishes file name and loads `Workbench.CurrentLoops` once.
- Cancellation/error tests: pending state clears while previously confirmed loops remain unchanged.
- Transaction tests: preview enables only commit/cancel; commit enables undo; cancel restores pre-preview geometry; undo/redo round-trip the geometry signature.
- Pointer tests: a drag moves only the loop already selected at pointer press; dragging an unselected loop selects without moving the stale selection; non-left and capture-loss paths do not transform.
- Control-state tests: no-geometry and pending-preview states disable selection/offset controls; unsupported controls are disabled and marked TODO.
- Shell test: Workbench geometry/selection changes update the existing center host without creating a second page, ruler, toolbar, inspector, or geometry copy.

### 7. Wrong vs Correct

#### Wrong

```csharp
// Two editors diverge, and a second preview can replace the pending transaction.
var fixedHost = new CadHostState();
var hiddenWorkbench = new CadWorkbenchViewModel();
fixedHost.LoadConfirmedImport(path, loops);
hiddenWorkbench.LoadLoops(loops);
fixedHost.Workbench.MoveSelected(delta);
fixedHost.Workbench.PreviewOffset(8, OffsetDirection.Inside);
```

#### Correct

```csharp
var cadHost = new CadHostState();
var coordinator = new ImportCoordinator(/* ... */, cadHost: cadHost);
var shell = new AppShellViewModel(/* ... */, cadHost);
await coordinator.InspectAsync(path, cancellationToken);
coordinator.ConfirmMillimetres();

cadHost.Workbench.SelectPiece(point);
cadHost.Workbench.MoveSelected(delta); // creates one pending preview
cadHost.Workbench.Commit();            // records it in the session undo stack
cadHost.Workbench.Undo();              // returns to the pre-drag geometry
```
