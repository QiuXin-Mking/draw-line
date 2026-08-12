# Stage 1 Manual Smoke Checklist

Use this checklist after producing a signed or internal self-contained build. Do not run it on a production workstation until the pilot-machine backup step has completed.

## Evidence to attach

- Build version, SHA-256, release date, and tester name.
- OS edition, version/build, architecture, RAM, GPU/driver, screen resolution, and Windows display scale.
- A screenshot of the DXF inspection result and the saved `.lnproj` reopened successfully.
- Any error text, diagnostic code, and the original DXF file name (do not attach customer geometry to public tickets).

## Test matrix

| Environment | Required scope | Result |
|---|---|---|
| macOS arm64 | Start, import, confirm millimetres, save, reopen | ☐ Pass / ☐ Fail |
| macOS x64 | Start, import, confirm millimetres, save, reopen | ☐ Pass / ☐ Fail |
| Windows 11 x64 | Start, import, confirm millimetres, save, reopen | ☐ Pass / ☐ Fail |
| Windows 10 Enterprise/IoT LTSC 2019 x64 | Same flow | ☐ Pass / ☐ Fail |
| Windows 10 Enterprise/IoT LTSC 2021 x64 | Same flow | ☐ Pass / ☐ Fail |
| Windows 10 Home/Pro 22H2 x64 | Best-effort smoke; record support notice | ☐ Pass / ☐ Fail |

Unsupported Windows 7, 8.1, XP, and Vista must not be treated as a failed test target. Follow [the upgrade guide](../../.trellis/tasks/08-07-leather-nesting-windows-clone/windows-upgrade-guide.md) instead.

## Core workflow

1. On a non-production machine, record the evidence fields above and confirm at least 1 GB free disk space.
2. Start the self-contained app. Expected: the DXF Import Inspector opens without a missing-.NET dialog or an unhandled exception.
3. Create a project named `Stage1-Smoke`.
4. Select `凉鞋.dxf` from the fixture set, or a permitted non-customer copy of it. Expected: the app lists the declared unit, layers, and diagnostics.
5. Confirm millimetres. Expected: the project revision becomes `1`, its state becomes unsaved, and exactly one import is listed.
6. Save to a writable folder whose path contains Chinese characters and spaces, then close and reopen the `.lnproj`. Expected: the project name, revision, source SHA-256, and import record remain present.
7. Save the same project again after a change. Verify a `.lnproj.bak` file exists, then open it. Expected: it opens as the previous complete revision.
8. Select `38.DXF`. Expected: the app reports a blocking open-`POLYLINE` diagnostic and never silently closes or imports it as a closed cut piece.
9. Cancel an in-progress import. Expected: no revision or import record is added.

## Accessibility and low-resolution checks

On Windows at 1366×768, repeat steps 2–9 at 100%, 125%, and 150% display scaling. The project name field, Select DXF, Inspect, Confirm, Cancel, Save, and diagnostics must remain reachable. Complete the same workflow using keyboard focus and Enter/Space activation. Record any clipped text or unreachable control as a failure.

## Pass/fail rule

Stage 1 platform gate passes only after all required environments pass the core workflow and the DPI checks have no blocking control issue. A best-effort Windows 10 Home/Pro result is reported separately; it does not replace LTSC coverage.
