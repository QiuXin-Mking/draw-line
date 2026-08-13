# Technical Design

## Architecture

Use one persistent nesting-workstation shell instead of full-page modules. Stable regions own order/group state, piece records, center CAD/nesting canvas, candidates, output statistics and status. Existing domain/application capabilities adapt into these regions; UI handlers do not duplicate them.

Reference states are deterministic snapshots with provenance (`real`, `demo`, `todo`). Visual parity and business truth are separate: demo values may support screenshot acceptance but never claim real nesting, file or device success.

## 1:1 Contract

Crop references to the product window, excluding macOS, ToDesk, IME, notifications, Dock and Windows taskbar. Same-size tests compare region geometry, control order/text, state and color. Dynamic geometry/antialiasing use documented masks; redesign is not a tolerance. Brand substitution only changes identity content, never geometry.

## Boundaries and Data Flow

Shell owns pane geometry. CAD owns import/edit. Order/piece owns shared records/cards/property table. Settings owns modals. Results owns placements/candidates/statistics. Send owns validated file handoff. Integration owns scenarios/capture. Cutting control is a separate executable and security boundary.

DXF import -> confirmed geometry -> piece/order state -> material/settings snapshot -> nesting result -> candidate/statistics -> validated send. Edits invalidate downstream results. Unknown semantics remain evidence gaps rather than invented formulas.

## Safety and Rollout

Phase 1 is the main nesting application. Phase 2A is the cutting-control UI with offline simulation. Phase 2B hardware activation needs separate protocol and safety approval. Each child lands and rolls back independently; the parent is not an implementation target.
