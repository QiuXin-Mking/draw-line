# 1比1材料版型与排版设置

## Goal

Reproduce the modal material/layout and nesting-strategy settings without redesigning them as pages.

## Requirements

- Inherit parent decisions and use images 14, 16, 19, 20, 26.
- Preserve dialog dimensions, grouping, field order, visible dropdown options, defaults, focus, and confirm/cancel placement.
- Keep undocumented algorithm semantics configurable/TODO until verified.

## Acceptance Criteria

- [ ] Both modal dialogs overlay the persistent main shell and visually align to their references.
- [ ] Confirm/cancel and validated field state are testable; unsupported algorithm effects are not claimed.

## Notes

- Keep `prd.md` focused on requirements, constraints, and acceptance criteria.
- Lightweight tasks can remain PRD-only.
- For complex tasks, add `design.md` for technical design and `implement.md` for execution planning before `task.py start`.
