# Implementation Plan

1. [x] Add failing host-integration and CAD evidence-contract tests.
2. [x] Build compact file-operation/drawing rows and right CAD property pane from images 04–12/21.
3. [x] Adapt real M02 confirmed geometry into the central host; imported line geometry remains read-only until editing commands are implemented.
4. [x] Preserve unsupported command TODO boundaries and module cache.
5. [x] Run Desktop tests, build, diff check.

## Implementation notes

- Confirmed M02 imports publish their geometry only after the existing millimetre confirmation gate.
- The fixed shell owns the CAD file row, drawing row, black canvas, rulers, and right property pane; it does not embed another full M03 page.
- Drawing, selection, save-as, replacement, resizing, and line-editing actions remain explicit TODO notices. No production persistence or editing success is claimed.
