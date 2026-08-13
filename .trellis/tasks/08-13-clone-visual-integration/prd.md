# 1比1截图视觉回归与集成验收

## Goal

Integrate all 1:1 children and enforce screenshot-based acceptance for the whole operator workflow.

## Requirements

- Inherit the parent 1:1 decision and all three research files.
- Provide deterministic scenario navigation and same-size captures for every in-scope reference state.
- Define crop masks excluding macOS, ToDesk, input method, notifications, Windows taskbar, and other capture noise.
- Verify geometry, text/order, state, color, and workflow paths; do not accept isolated module pages as a substitute for the combined shell.

## Acceptance Criteria

- [ ] All in-scope images have a mapped deterministic product state and comparison artifact.
- [ ] Image 27 combined main-window acceptance passes.
- [ ] Brand substitution, TODO honesty, build, automated tests, and manual workflow evidence pass.

## Notes

- Keep `prd.md` focused on requirements, constraints, and acceptance criteria.
- Lightweight tasks can remain PRD-only.
- For complex tasks, add `design.md` for technical design and `implement.md` for execution planning before `task.py start`.
