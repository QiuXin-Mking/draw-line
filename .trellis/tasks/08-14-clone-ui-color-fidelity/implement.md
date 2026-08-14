# Implementation Plan

1. [x] Add failing palette contract tests for evidence RGB values, neutral temperature and distinct chrome/geometry roles.
2. [x] Refactor `AppTheme` into semantic screenshot-evidence tokens with compatibility aliases where needed.
3. [x] Migrate fixed shell, toolbar, pane hosts, order/piece panels, CAD hosts and property window chrome away from raw/biased colors.
4. [x] Add component tests for shared surfaces and state colors; confirm layout and behavior contracts remain unchanged.
5. [x] Run focused Desktop tests, full solution tests, full build and `git diff --check`.
6. [x] Launch at 1366×768, capture the native window, inspect reference and implementation with `view_image`, and write a five-area fidelity ledger.
7. [x] Fix visible color drift found by the ledger; repeat capture until no fixable color mismatch remains.

## Verification Results

- Palette RED: focused test failed on the former green/warm values for title, menu, toolbar, panel, header, piece card, progress and icon roles.
- Component RED: focused test failed while status and property surfaces still used header gray or transparent inheritance.
- Focused Desktop palette/top-command contracts: 23 passed before native capture;
  the final menu/status/light-theme regression filter passed 3 tests.
- Desktop test project: 167 passed.
- Full solution tests: 259 passed (167 Desktop, 60 Geometry, 22 Infrastructure,
  7 Application, 2 Domain, 1 end-to-end).
- Full solution build: succeeded with 0 warnings and 0 errors.
- `git diff --check`: passed.
- Task-scoped `dotnet format --verify-no-changes`: passed. The repository-wide formatter still reports pre-existing whitespace/final-newline debt in unrelated Geometry, Application, and legacy Desktop files; those files were not modified by this task.
- Native capture: macOS reported a 1366×796 outer window, matching the requested
  1366×768 Avalonia client plus the 28 px native title bar. The final Retina capture is
  stored as `native-1366x768.png`; the five-area comparison is in `fidelity-ledger.md`.
- The native comparison confirmed the top-menu light-surface/dark-text regression fix.
- The first capture exposed two additional task-scoped inherited-theme defects: default
  controls followed macOS dark mode, and status labels inherited white foreground on the
  light status surface. The fixed workstation now requests the light control theme and its
  normal status labels explicitly use `PrimaryText`.
- The final comparison found no remaining fixable color mismatch. It records left piece-card
  overlap/clipping and sparse right CAD-control placement as out-of-scope layout defects.

## Check Review

- Fixed the palette test boundary so exact evidence RGB values are asserted on the semantic tokens consumed by cloned components, rather than only through legacy alias names.
- Added an explicit same-instance compatibility-alias contract to protect untouched modules.
- Synchronized `.trellis/spec/frontend/component-guidelines.md` with the implemented chrome, interaction, workstation, and geometry role boundaries.
- Reviewed the changed runtime paths and found no layout ratios, visible copy, routing, state transitions, or business/data contracts changed. Remaining raw colors in the reviewed clone scope are data-specific CAD layer swatches or transparent vector fills, not duplicated chrome colors.
- Made the shared palette brushes immutable and kept their palette tests in the Avalonia UI
  collection. This prevents a later test from reading a mutable brush owned by a different
  dispatcher thread when the full suite runs in parallel.
- Final reviewer check: all 167 Desktop tests and the full solution build pass. The current
  full solution test run is blocked only by unrelated Infrastructure DXF round-trip tests in
  the concurrently edited export task; no Desktop palette or clone-surface test fails.

## Risky files / rollback points

- `src/LeatherNesting.Desktop/DesignSystem/AppTheme.cs`: shared blast radius; preserve aliases for legacy modules.
- `src/LeatherNesting.Desktop/Shell/*`: do not alter five-pane geometry while migrating brushes.
- CAD canvas files: distinguish chrome from geometry semantics before replacing colors.
- User-owned unrelated dirty files and image evidence directories must not be staged or committed.

## Validation commands

```bash
dotnet test tests/LeatherNesting.Desktop.Tests/LeatherNesting.Desktop.Tests.csproj --no-restore
dotnet test LeatherNesting.sln --no-restore
dotnet build LeatherNesting.sln --no-restore
git diff --check
```
