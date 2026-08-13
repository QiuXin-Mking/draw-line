# 1比1发送设置与交接流程

## Goal

Reproduce send settings and the evidenced output-folder handoff while keeping external operations honest.

## Requirements

- Inherit parent decisions and use images 22–24.
- Preserve modal geometry and every evidenced field/control order.
- Real writes use validated registered formats/adapters and report failure; unknown formats/device invocation remain disabled/TODO.

## Acceptance Criteria

- [ ] Send dialog visually aligns with image 22.
- [ ] Supported output produces an actual file in the selected folder; unsupported choices cannot report success.
- [ ] Output-folder scenario reproduces the evidence structure without hard-coded customer paths.

## Notes

- Keep `prd.md` focused on requirements, constraints, and acceptance criteria.
- Lightweight tasks can remain PRD-only.
- For complex tasks, add `design.md` for technical design and `implement.md` for execution planning before `task.py start`.
