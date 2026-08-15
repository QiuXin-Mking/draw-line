# CAD panel usability verification ledger

## Quality gate

| Check | Result |
| --- | --- |
| Desktop tests | PASS — 246 passed, 0 failed, 0 skipped |
| Solution build | PASS — 0 warnings, 0 errors |
| Task-scoped format verification | PASS — no output |
| `git diff --check` | PASS — no output |
| Native window | PASS for launch/layout — `Leather Nesting`, 1366×796 outer window (1366×768 client plus macOS title chrome) |
| Native screenshot | [`native-1366x768.png`](native-1366x768.png) |

The final review reran the commands against the shared worktree after the parallel
files became compilable. Desktop tests and the complete solution build both pass.

Commands:

```text
dotnet test tests/LeatherNesting.Desktop.Tests/LeatherNesting.Desktop.Tests.csproj --no-restore
dotnet build LeatherNesting.sln --no-restore
dotnet format LeatherNesting.sln --no-restore --verify-no-changes --include <task files>
git diff --check
```

## Requirement evidence

| Requirement | Evidence | Status |
| --- | --- | --- |
| R1 single CAD session | `Confirmed_import_is_loaded_into_the_single_shared_workbench_session`; M02-to-M03 integration assertion uses the same `CadHostState`, `Workbench.CurrentLoops`, and `CanvasView.Loops` references | PASS |
| R2 reachability | `Fixed_host_uses_the_interactive_canvas_and_shared_workbench_callbacks`; persistent M03 shell screenshot | PASS |
| R3 view interaction | `CanvasView` pointer implementation plus host callback test covers selection/highlight and drag; range button calls `Refit`; existing CanvasView behavior covers wheel zoom and blank pan | PASS |
| R4 real editing loop | `Property_pane_drives_preview_cancel_commit_undo_and_redo_on_the_shared_session` | PASS |
| R5 honest degradation | `Unsupported_drawing_tools_are_disabled_and_marked_todo`; property-pane TODO assertion | PASS |
| R6 import hand-off | `Confirmed_m02_inspection_publishes_real_geometry_to_shared_cad_host` verifies inspect gate, confirmation, M03 return, edit, commit, and undo | PASS |
| R7 regression protection | 246 Desktop tests, including new workbench, host, property-pane, pointer-transaction isolation, and integration contracts | PASS |
| R8 visual constraint | `ShellFrameTests`, `CloneSurfaceColorTests`, and native screenshot preserve the five-pane shell and semantic colors | PASS |
| R9 scoped tools | Automated tests cover close, offset, move, rotate, commit/cancel, undo/redo; CanvasView contracts cover select and view navigation | PASS |
| R10 session boundary | Commit status and button text explicitly say CAD session and state that the project file was not written | PASS |

## Acceptance criteria

| Criterion | Evidence | Status |
| --- | --- | --- |
| Confirmed DXF appears; empty state guides import | Import integration test; native screenshot shows the pre-import guidance | PASS |
| Full view, zoom, pan, selection, highlight | CanvasView implementation/host tests; native layout inspection | PASS |
| Parameter → preview → commit/cancel → undo/redo | Property-pane transaction test | PASS |
| Close, offset, drag move, and fixed rotation | Host/property-pane tests | PASS |
| Supported controls mutate state; unsupported controls do not masquerade | Host/property-pane tests | PASS |
| M02 and main CAD panel share one session | Reference-identity and integration tests | PASS |
| Desktop automation covers reachability and transactions | 246/246 Desktop tests | PASS |
| Native 1366×768 controls visible/clickable | Screenshot confirms the CAD session group and offset editor fit at 1366×768. Full mouse-driven native workflow was not completed because macOS denied Accessibility control to `osascript` (`-1719`); equivalent interactions passed through Avalonia UI contracts. | PARTIAL |
| Tests, build, and format pass without unrelated scope | Quality-gate commands above; scoped diff review below | PASS |

## Native inspection notes

- The application launched successfully as `LeatherNesting.Desktop` with a 1366×796 outer window; the extra 28 px is native macOS title chrome over the requested 1366×768 client.
- The initial M03 surface showed the black interactive canvas, import guidance, CAD session controls, offset command/value, and disabled controls without clipping.
- Screen Recording permission was available and the task screenshot was captured.
- Automated desktop control was blocked by missing macOS Accessibility permission, so import-file selection and pointer gestures could not be replayed against the live process in this run. This limitation is recorded rather than treating automated UI-contract coverage as a manual observation.

## Final review fixes

- Pointer input now filters for the left button, captures the pointer for a complete gesture, and consumes wheel/release events.
- Dragging from an unselected contour selects it instead of moving a previously selected, different contour.
- A pending preview blocks canvas selection/drag mutation and disables the selection command until commit or cancel.
- Offset distance and direction inputs follow the same dynamic enabled state as the offset preview command.
- Host notification suppression is restored with `finally` if session loading ever throws.

## Scope review

Task changes are limited to the shared Desktop CAD state/view-model, shell-owned canvas/property controls, Desktop tests, and these task verification artifacts. No DXF reader/writer, geometry algorithm, project schema, `05-图片/`, `06-首次远程/`, `python-demo/`, README, or unrelated task file was modified by this implementer.
