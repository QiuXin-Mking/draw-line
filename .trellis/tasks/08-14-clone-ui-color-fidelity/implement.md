# Implementation Plan

1. [x] Add failing palette contract tests for evidence RGB values, neutral temperature and distinct chrome/geometry roles.
2. [x] Refactor `AppTheme` into semantic screenshot-evidence tokens with compatibility aliases where needed.
3. [x] Migrate fixed shell, toolbar, pane hosts, order/piece panels, CAD hosts and property window chrome away from raw/biased colors.
4. [x] Add component tests for shared surfaces and state colors; confirm layout and behavior contracts remain unchanged.
5. [x] Run focused Desktop tests, full solution tests, full build and `git diff --check`.
6. [ ] Launch at 1366×768, capture the native window, inspect reference and implementation with `view_image`, and write a five-area fidelity ledger.
7. [ ] Fix visible color drift found by the ledger; repeat capture until no fixable color mismatch remains.

## Verification Results

- Palette RED: focused test failed on the former green/warm values for title, menu, toolbar, panel, header, piece card, progress and icon roles.
- Component RED: focused test failed while status and property surfaces still used header gray or transparent inheritance.
- Focused Desktop color contracts: 19 passed after review added the compatibility-alias contract.
- Desktop test project: 165 passed.
- Full solution tests: 252 passed (165 Desktop, 55 Geometry, 22 Infrastructure, 7 Application, 2 Domain, 1 end-to-end).
- Full solution build: succeeded with 0 warnings and 0 errors.
- `git diff --check`: passed.
- Task-scoped `dotnet format --verify-no-changes`: passed. The repository-wide formatter still reports pre-existing whitespace/final-newline debt in unrelated Geometry, Application, and legacy Desktop files; those files were not modified by this task.
- Native 1366×768 capture and the five-area fidelity ledger remain pending because macOS Screen Recording permission is unavailable. This is an external acceptance blocker, not a code failure; no GUI capture was attempted during review.

## Check Review

- Fixed the palette test boundary so exact evidence RGB values are asserted on the semantic tokens consumed by cloned components, rather than only through legacy alias names.
- Added an explicit same-instance compatibility-alias contract to protect untouched modules.
- Synchronized `.trellis/spec/frontend/component-guidelines.md` with the implemented chrome, interaction, workstation, and geometry role boundaries.
- Reviewed the changed runtime paths and found no layout ratios, visible copy, routing, state transitions, or business/data contracts changed. Remaining raw colors in the reviewed clone scope are data-specific CAD layer swatches or transparent vector fills, not duplicated chrome colors.

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
