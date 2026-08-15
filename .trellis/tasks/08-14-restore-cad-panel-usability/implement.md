# Restore CAD Panel Usability Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Restore a real, testable CAD editing loop inside the persistent five-pane workstation.

**Architecture:** Keep one `CadWorkbenchViewModel` inside the shared `CadHostState`, project its geometry into the shell-owned `CanvasView`, and bind only the supported core controls to that same session. M02 remains the confirmation gate; unsupported reference controls become honestly disabled.

**Tech Stack:** .NET 10, C# 14, Avalonia, xUnit, existing `LeatherNesting.Application.CadEditing` command/session layer.

**Spec:** `.trellis/tasks/08-14-restore-cad-panel-usability/design.md`

## Global Constraints

- Preserve the persistent five-pane workstation and semantic `AppTheme` colors.
- Do not change DXF reader/writer behavior, geometry algorithms, or `ProjectDocument` schema.
- Do not touch `05-图片/` or files owned by parallel DXF work.
- “Commit” means commit to the undoable CAD session, not save to project storage.
- Every supported UI action must change observable session state; unsupported actions are disabled and marked TODO.

---

### Task 1: Make the workbench state observable and transaction-safe

**Files:**
- Modify: `src/LeatherNesting.Desktop/ViewModels/CadWorkbenchViewModel.cs`
- Test: `tests/LeatherNesting.Desktop.Tests/CadWorkbenchViewModelTests.cs`

**Interfaces:**
- Produces: `event EventHandler? Changed`, `void ClearSelection()`, and reliable state notifications from all mutating methods.
- Preserves: existing public preview, transform, commit, cancel, undo, and redo methods.

- [x] Add failing tests proving `Changed` fires after load/select/preview/commit/cancel/undo/redo, `LoadLoops` clears selection, and invalid/no-op transactions do not report a successful state.
- [x] Run `dotnet test tests/LeatherNesting.Desktop.Tests/LeatherNesting.Desktop.Tests.csproj --no-restore --filter FullyQualifiedName~CadWorkbenchViewModelTests` and confirm the new tests fail for missing notification/selection contracts.
- [x] Add a private `NotifyChanged()` and call it once per completed public transition; add `ClearSelection()` and reset selection in `LoadLoops`.
- [x] Make `Commit`, `Undo`, and `Redo` consume `CadCommandResult.Success` before changing state; surface result diagnostics through `ProblemMessages`.

Implementation shape:

```csharp
public event EventHandler? Changed;

public void ClearSelection()
{
    if (SelectedLoopId is null) return;
    SelectedLoopId = null;
    Changed?.Invoke(this, EventArgs.Empty);
}

private void Complete(CadCommandResult result, WorkbenchState successState)
{
    _problemMessages.Clear();
    _problemMessages.AddRange(result.Diagnostics);
    if (result.Success) _state = successState;
    Changed?.Invoke(this, EventArgs.Empty);
}
```

- [x] Re-run the focused tests and require all `CadWorkbenchViewModelTests` to pass.

### Task 2: Make `CadHostState` the single CAD session owner

**Files:**
- Modify: `src/LeatherNesting.Desktop/Modules/CadCanvas/CadHostState.cs`
- Test: `tests/LeatherNesting.Desktop.Tests/Shell/CadHostEvidenceTests.cs`

**Interfaces:**
- Produces: `CadWorkbenchViewModel Workbench { get; }`, `Loops` as a projection of `Workbench.CurrentLoops`, and `void ReportError(string message)`.
- Consumes: Task 1 `Changed` event and `ClearSelection()` behavior.

- [x] Add failing tests proving confirmed import loads `Workbench`, `Loops` and `Workbench.CurrentLoops` reference the same projected collection, workbench changes raise host `Changed`, and `Clear()` resets geometry/selection/status.
- [x] Run the `CadHostEvidenceTests` filter and confirm failures show the current duplicate `_loops` storage.
- [x] Replace `_loops` ownership with one private Workbench instance; subscribe once to its `Changed` event and forward a host refresh.
- [x] In `LoadConfirmedImport`, copy the external list once, load it into Workbench, then derive file/status fields; suppress duplicate change notifications if needed so one user transition produces one host refresh.

Implementation shape:

```csharp
public CadWorkbenchViewModel Workbench { get; } = new();
public IReadOnlyList<Loop2D> Loops => Workbench.CurrentLoops ?? [];

public CadHostState()
{
    Workbench.Changed += (_, _) => RefreshFromWorkbench();
}

public void LoadConfirmedImport(string path, IReadOnlyList<Loop2D> loops)
{
    var snapshot = loops.ToArray();
    FileName = Path.GetFileName(path);
    Workbench.LoadLoops(snapshot);
}

public void ReportError(string message)
{
    StatusMessage = message;
    Changed?.Invoke(this, EventArgs.Empty);
}
```

- [x] Re-run focused host tests and require them to pass.

### Task 3: Replace the read-only evidence canvas with the interactive shared canvas

**Files:**
- Modify: `src/LeatherNesting.Desktop/Views/CanvasView.cs`
- Modify: `src/LeatherNesting.Desktop/Shell/CadWorkspaceHost.cs`
- Test: `tests/LeatherNesting.Desktop.Tests/Shell/CadHostEvidenceTests.cs`
- Test: `tests/LeatherNesting.Desktop.Tests/Shell/ShellFrameTests.cs`

**Interfaces:**
- Produces: `void CanvasView.Refit()`, configurable canvas/pen properties, and an exposed `CanvasView Drawing` contract on `CadWorkspaceHost` for UI assertions.
- Consumes: `CadHostState.Workbench.SelectPiece`, `MoveSelected`, `SelectedLoopId`, and `CurrentLoops`.

- [x] Add failing UI contract tests asserting the shell canvas contains `CanvasView`, range zoom requests refit, imported loops are supplied to it, click selection updates highlight, and dragging a selected loop creates a preview.
- [x] Run the focused shell tests and confirm they fail because `CadEvidenceCanvas` is read-only.
- [x] Add `CanvasView.Refit()` that sets `_fitPending = true` and invalidates rendering without changing loops.
- [x] Replace `_drawing` with `CanvasView`; wire `OnClick` to selection and `OnDrag` to preview movement; refresh `SelectedLoopId` and call `SetData(..., refit: false)` on host change.
- [x] Wire “范围缩放” to `CanvasView.Refit`; wire “选择” to an honest selection-mode status; set draw polyline, rectangle, and delete buttons disabled with TODO tooltips.

Implementation shape:

```csharp
public IBrush CanvasBrush { get; set; } = Brushes.White;
public IPen OuterContourPen { get; set; } = new Pen(Brushes.Navy, 1.5);
public IPen InternalContourPen { get; set; } = new Pen(Brushes.OrangeRed, 1.5);
public IPen SelectionPen { get; set; } = new Pen(Brushes.DodgerBlue, 3);

public void Refit()
{
    _fitPending = true;
    InvalidateVisual();
}

Drawing = new CanvasView
{
    CanvasBrush = AppTheme.CanvasBlack,
    OuterContourPen = new Pen(AppTheme.GeometryOuterContour, 1.5),
    InternalContourPen = new Pen(AppTheme.GeometryInternalLine, 1.5),
    SelectionPen = new Pen(AppTheme.ClassicFocus, 3),
};
Drawing.OnClick = point => state.Workbench.SelectPiece(point);
Drawing.OnDrag = delta => state.Workbench.MoveSelected(delta);
```

- [x] Re-run the focused shell tests and require them to pass.

### Task 4: Connect the core property and transaction controls

**Files:**
- Modify: `src/LeatherNesting.Desktop/Shell/CadPropertyPane.cs`
- Test: `tests/LeatherNesting.Desktop.Tests/Shell/CadHostEvidenceTests.cs`
- Test: `tests/LeatherNesting.Desktop.Tests/DesignSystem/CloneSurfaceColorTests.cs`

**Interfaces:**
- Consumes: `CadHostState.Workbench` and its `Changed` event.
- Produces: connected controls for close contour, offset direction/distance, rotate +15°, session commit, cancel, undo, redo, and clear selection; exposes named controls or a stable lookup for tests.

- [x] Add failing tests that load deterministic geometry and assert each supported button changes tool/session/geometry state; assert invalid offset text preserves geometry and emits a diagnostic; assert unsupported buttons are disabled and visibly marked TODO.
- [x] Run the focused `CadHostEvidenceTests` and confirm the current `ReportUnsupported` wiring fails the behavior assertions.
- [x] Add a top “CAD 会话” group with state/diagnostic text and buttons whose enabled state is recomputed on Workbench `Changed`.
- [x] Bind close contour to `PreviewClose`; parse offset with invariant/current culture fallback, reject non-finite/zero values, then call `PreviewOffset(distance, selectedDirection)`; bind rotate, commit, cancel, undo, redo, and clear selection directly to Workbench.
- [x] Convert every unimplemented action button to disabled TODO and disable orphan input controls that cannot affect a supported command.

Implementation shape:

```csharp
private readonly Dictionary<string, Button> _actions = new(StringComparer.Ordinal);

private void PreviewOffset(CadHostState state)
{
    if (!double.TryParse(_offsetDistance.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out var distance) ||
        !double.IsFinite(distance) || distance == 0)
    {
        state.ReportError("内缩值必须是非零有限数值（mm）。");
        return;
    }

    state.Workbench.SelectTool(CadToolMode.Offset);
    state.Workbench.PreviewOffset(Math.Abs(distance),
        _outside.IsChecked == true ? OffsetDirection.Outside : OffsetDirection.Inside);
}

private void RefreshActions(CadHostState state)
{
    _actions["提交到 CAD 会话"].IsEnabled = state.Workbench.CanCommit;
    _actions["取消预览"].IsEnabled = state.Workbench.CanCancel;
    _actions["撤销"].IsEnabled = state.Workbench.CanUndo;
    _actions["重做"].IsEnabled = state.Workbench.CanRedo;
}
```

- [x] Re-run the focused host and color-contract tests and require them to pass.

### Task 5: Prove M02-to-M03 shared-session integration

**Files:**
- Modify: `src/LeatherNesting.Desktop/Shell/AppShellView.cs` only if event/order corrections are required
- Modify: `src/LeatherNesting.Desktop/Composition/DesktopComposition.cs` only if composition exposure is required
- Test: `tests/LeatherNesting.Desktop.Tests/UiDemoIntegrationTests.cs`
- Test: `tests/LeatherNesting.Desktop.Tests/Shell/TopCommandAreaTests.cs`
- Test: `tests/LeatherNesting.Desktop.Tests/Modules/Import/ImportCoordinatorTests.cs`

**Interfaces:**
- Consumes: the single `CadHostState` shared by `ImportCoordinator` and `AppShellViewModel`.
- Produces: confirmed import -> visible/editable main CAD session with no secondary geometry copy.

- [x] Add an integration test that inspects and confirms deterministic DXF geometry, observes the shell return to M03, selects the imported loop in the persistent canvas, previews an edit, commits it, and undoes it through the same host state.
- [x] Run the integration-focused tests and confirm any remaining routing/state-copy failure is reproducible.
- [x] Make the minimum composition or event-order correction necessary; no composition correction was required after the shared host session was connected.
- [x] Replace obsolete tests that require “unsupported select” or a read-only evidence canvas with assertions for honest disabled tools and real core behavior.

Integration assertion shape:

```csharp
coordinator.ConfirmMillimetres();
Assert.Same(cad.Loops, cad.Workbench.CurrentLoops);
cad.Workbench.SelectPiece(new Point2D(10, 10));
cad.Workbench.MoveSelected(new Point2D(5, 0));
Assert.True(cad.Workbench.CanCommit);
cad.Workbench.Commit();
cad.Workbench.Undo();
Assert.Equal(originalSignature, GeometrySignature(cad.Loops));
```

- [x] Re-run all Desktop tests and require zero failures (246 passed, 0 failed, 0 skipped after final review fixes).

### Task 6: Native interaction verification and quality gate

**Files:**
- Create: `.trellis/tasks/08-14-restore-cad-panel-usability/verification-ledger.md`
- Create: `.trellis/tasks/08-14-restore-cad-panel-usability/native-1366x768.png`
- Modify: `.trellis/tasks/08-14-restore-cad-panel-usability/implement.md` to check completed steps and record exact results

**Interfaces:**
- Produces: auditable native evidence for every acceptance criterion.

- [x] Run `dotnet test tests/LeatherNesting.Desktop.Tests/LeatherNesting.Desktop.Tests.csproj --no-restore` and record total/pass/fail counts.
- [x] Run `dotnet build LeatherNesting.sln --no-restore`, task-scoped `dotnet format --verify-no-changes`, and `git diff --check`; record exact outputs.
- [ ] Launch the native app at a 1366×768 client area and verify import confirmation, full view, wheel zoom, blank pan, click selection, drag preview, close/offset preview, session commit/cancel, undo/redo, and disabled TODO controls.
- [x] Capture the final native screenshot and write a ledger mapping R1-R10 and every acceptance criterion to an automated test or manual observation.
- [x] Review the diff for accidental DXF, project persistence, `05-图片/`, or unrelated task changes before handing off to `trellis-check`.

Native note: launch, viewport, screenshot, and layout visibility passed. Full mouse-driven import/edit replay remains unchecked because macOS denied Accessibility automation (`osascript` error `-1719`); the exact limitation and equivalent automated interaction evidence are recorded in `verification-ledger.md`.
