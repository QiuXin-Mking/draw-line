# Native Color Fidelity Ledger

## Evidence and method

- Reference: `05-图片/27.png`, with `10.png`, `14.png`, `16.png`, and `21.png`
  used as cross-checks for the same fixed workstation surfaces.
- Implementation: `native-1366x768.png`, captured from the native Avalonia window
  after the final color repairs. macOS reported a 1366×796 outer window: the requested
  1366×768 client plus the 28 px native title bar. The PNG is Retina-scaled and includes
  capture padding; comparison uses the application client, not that padding.
- A user-provided native screenshot independently confirmed that the formerly unreadable
  top-menu labels now render dark on the white menu surface.
- Evidence values were sampled from broad flat regions in the reference. Rendered values
  were sampled from the final PNG and checked against the semantic `AppTheme` brushes.

## Five-area ledger

| Area | Reference evidence | Final rendered evidence | Mismatch and repair | Result |
| --- | --- | --- | --- | --- |
| Title / menu / toolbar | title near `#1B3030`; menu near `#FEFEFF`; toolbar `#EEF0F2`; icon teal `#469589` | `#1B3030`, `#FEFEFF`, `#EEF0F2`, `#469589` | The first native run inherited white text for top-level menu labels and dark-theme default controls. Menu items now explicitly use `PrimaryText`; the fixed workstation requests the light control theme. | Match within screenshot compression; menu text is readable. |
| Left piece cards | dominant cyan includes `#98D4EF` with compressed near-values such as `#A4D8F0` | card `#98D4EF`; selected/edit surface `#C1E5F5` | Replaced the former green-gray demo surface with `PieceCardCyan`; kept interaction state separate from the base card color. | Base cyan matches. |
| CAD canvas / rulers | canvas is compressed near-black (`#0A0A0A` family); red material boundary and neutral charcoal rulers | canvas `#000000`; ruler `#323232`; geometry boundary `#FF0000` | Removed green tint from ruler chrome and kept red/white/green geometry roles separate from application chrome. Exact black is the evidence-locked semantic value; the reference near-black variation is capture compression. | No fixable color drift. |
| Right panel / property controls | panel near `#FFFFFF`; header `#D9D9D9`; thin neutral-gray borders | panel `#FFFFFF`; header `#D9D9D9`; border `#808080` | Property surface was explicitly bound to `PanelSurface`. The light workstation theme repairs gray dark-mode inputs and white inherited labels without changing control placement. | Neutral light controls match the classic surface family. |
| Status / progress | status near `#F0F0F0`; progress cyan `#51B2C4` | status `#F0F0F0`; progress `#51B2C4`; normal text `#202020` | The first capture exposed white inherited status labels on the light status surface. Status labels now explicitly use `PrimaryText`; the small DEMO notice intentionally remains `WarningText`. | Readable neutral status and matching cyan progress. |

## Intentional and out-of-scope differences

- The macOS native title bar and capture padding are host environment chrome, not part of
  the cloned Windows client. Product branding replaces competitor identity as required.
- The final scenario is the deterministic empty-CAD workstation, while image 27 contains a
  populated nesting result. Geometry population is business/scenario state, not palette drift.
- Left piece-card labels and inputs overlap or clip at the captured width. This is a verified
  layout defect owned by the order/piece or visual-integration work, not this color task.
- Right CAD parameter controls retain large vertical gaps and appear to float in the pane.
  This is a verified layout/density defect owned by the CAD or visual-integration work.
- Those layout defects were deliberately not repaired here because this task forbids changing
  pane geometry, density, ordering, or business state while correcting color fidelity.

## Conclusion

All visible, task-scoped color drift found in the native comparison was repaired. No remaining
fixable mismatch was observed in the five palette areas; the remaining recorded defects are
layout/state differences routed to their owning clone tasks.
