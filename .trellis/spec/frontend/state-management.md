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

## Scenario: Confirmed DXF geometry projection into the fixed CAD host

### 1. Scope / Trigger

- Trigger: M02 imports geometry through the application/infrastructure boundary, while the persistent desktop shell renders that geometry without replacing its five-pane layout.
- The composition root creates one `CadHostState` and passes the same instance to `ImportCoordinator` and `AppShellViewModel`.

### 2. Signatures

```csharp
Task ImportCoordinator.InspectAsync(string path, CancellationToken cancellationToken);
void ImportCoordinator.ConfirmMillimetres();
void ImportCoordinator.CancelImport();
void CadHostState.LoadConfirmedImport(string path, IReadOnlyList<Loop2D> loops);
void CadHostState.ReportUnsupported(string action);
```

### 3. Contracts

- `InspectAsync` prepares both the DXF import decision and the closed-loop projection, but does not publish either to the fixed canvas.
- `ConfirmMillimetres` commits `UnitDecision.ConfirmedMillimetres`, publishes the prepared loops once, clears pending preparation, and updates the workspace project.
- `CancelImport` clears pending inspection only; it must not replace or clear the currently displayed confirmed geometry.
- `LoadConfirmedImport` stores an immutable loop snapshot, derives `FileName` from the path, clears the demo flag, updates status, and raises `Changed` once.
- Unsupported draw/select/save/edit commands may update `StatusMessage`, but must not mutate `Loops`.

### 4. Validation & Error Matrix

| Condition | Required behavior |
| --- | --- |
| Blank import path passed to `LoadConfirmedImport` | Throw `ArgumentException`; preserve current host state |
| Null loop collection | Throw `ArgumentNullException`; preserve current host state |
| Confirm without a project or preparation | Throw `InvalidOperationException`; publish nothing |
| Inspection cancelled | Clear pending inspection; keep confirmed canvas geometry |
| Confirmed DXF has zero closed loops | Publish an empty snapshot and an explicit no-displayable-loop status |
| Unsupported CAD action | Show the standard TODO notice; preserve geometry |

### 5. Good/Base/Bad Cases

- Good: inspect a DXF, confirm millimetres, then render the same loop snapshot in the shell-owned canvas.
- Base: confirm a valid DXF with zero closed loops and show the honest empty-result status.
- Bad: publish geometry during inspection, on file selection, or after cancellation; this bypasses the unit confirmation gate.

### 6. Tests Required

- Composition test: the importer and fixed shell observe the same `CadHostState` instance.
- Import test: inspection alone leaves the fixed host unchanged; confirmation publishes file name and loops.
- Cancellation/error tests: pending state clears while previously confirmed loops remain unchanged.
- Unsupported-action test: status changes to the standard TODO text while loop references/count remain unchanged.
- Shell test: geometry changes update the existing center host without creating a second page, ruler, toolbar, or inspector.

### 7. Wrong vs Correct

#### Wrong

```csharp
// Publishes unconfirmed geometry and creates divergent host state.
var coordinator = new ImportCoordinator(/* ... */, new CadHostState());
var shell = new AppShellViewModel(/* ... */, new CadHostState());
await coordinator.InspectAsync(path, cancellationToken);
shell.CadHost.LoadConfirmedImport(path, loops);
```

#### Correct

```csharp
var cadHost = new CadHostState();
var coordinator = new ImportCoordinator(/* ... */, cadHost: cadHost);
var shell = new AppShellViewModel(/* ... */, cadHost);
await coordinator.InspectAsync(path, cancellationToken);
coordinator.ConfirmMillimetres();
```
