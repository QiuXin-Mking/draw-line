# CAD panel root-cause evidence

## Regression boundary

- `git log` and `git blame` identify `74ade97 feat(desktop): clone CAD and order piece panels` as the change that introduced the persistent fixed CAD host and restricted the module overlay to M02.
- The later color-fidelity commit changes theme brushes and the window theme; it does not change CAD routing or commands.

## Reachability

- `src/LeatherNesting.Desktop/Shell/AppShellView.cs:82` returns from M02 to M03 after confirmed loops arrive.
- `src/LeatherNesting.Desktop/Shell/AppShellView.cs:210` assigns the selected module view but makes the overlay visible only for M02. The discovered M03 `CadCanvasView` therefore exists in `CurrentView` but is hidden.
- `tests/LeatherNesting.Desktop.Tests/Shell/ShellFrameTests.cs:101` intentionally locks this behavior by asserting the full M03 page is hidden and not embedded in the center surface.

## Fixed center host

- `src/LeatherNesting.Desktop/Shell/CadWorkspaceHost.cs:17` instantiates `CadEvidenceCanvas`, whose class comment explicitly calls it a read-only projection.
- `src/LeatherNesting.Desktop/Views/CadEvidenceCanvas.cs:16` exposes only `SetData`; `Refit` at line 22 only invalidates the visual. It has no pointer interaction or command session.
- `src/LeatherNesting.Desktop/Shell/CadWorkspaceHost.cs:71` wires range zoom to repaint, while polyline, rectangle, select, and delete all call `CadHostState.ReportUnsupported`.
- `tests/LeatherNesting.Desktop.Tests/Shell/CadHostEvidenceTests.cs:60` currently asserts unsupported controls only change TODO status and preserve geometry; green tests therefore confirm the non-functional behavior.

## Property pane

- `src/LeatherNesting.Desktop/Shell/CadPropertyPane.cs:24` creates the visible property controls.
- `src/LeatherNesting.Desktop/Shell/CadPropertyPane.cs:67` routes every action created by `AddButton` to `ReportUnsupported`.
- The input helpers at lines 75-120 store initial strings/check states for tests but do not publish user changes to CAD state.

## Existing reusable capability

- `src/LeatherNesting.Desktop/Views/CanvasView.cs:35` accepts loop data; lines 79-127 implement pointer-centered wheel zoom, blank-space pan, click selection callbacks, and drag callbacks.
- `src/LeatherNesting.Desktop/ViewModels/CadWorkbenchViewModel.cs` already owns selection, transform, repair/offset preview, commit/cancel, and undo/redo over `CadOperationSession`.
- `src/LeatherNesting.Desktop/Adapters/Import/DefaultImportCoordinatorFactory.cs:39` creates a new, independent Workbench for the M02 secondary tab, proving the editor exists but is not the shared shell session.

## Import boundary

- `src/LeatherNesting.Desktop/Modules/Import/ImportCoordinator.cs:54` prepares loops during inspection without publishing them.
- `src/LeatherNesting.Desktop/Modules/Import/ImportCoordinator.cs:62` publishes loops to the shared `CadHostState` only after millimetre confirmation. This confirmation gate must remain intact.

## Conclusion

The CAD panel is unusable because visual fidelity work substituted a read-only, TODO-wired shell surface for the already implemented interactive surfaces without introducing a shared editing-session contract. The smallest root-cause fix is to make the existing shared host own one Workbench session and reuse `CanvasView` plus existing commands inside the persistent shell.
