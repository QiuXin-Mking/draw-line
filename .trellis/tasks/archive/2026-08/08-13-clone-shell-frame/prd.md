# 1比1主窗口骨架与视觉令牌

## Goal

Build the persistent classic desktop frame and five-pane geometry that every 1:1 product state uses.

## Requirements

- Inherit the parent 1:1 operator-compatibility and brand-substitution decisions.
- Own the title/menu/large toolbar, unit/operator area, persistent order/piece/center/right panes, rulers, and status bar.
- Match pane ratios and density from `research/images-19-27.md` image 27; expose stable host regions for the other children.
- Do not implement business module internals in this child.

## Acceptance Criteria

- [ ] Same-size reference overlay confirms the five persistent body regions and top/bottom rows align within documented tolerance.
- [ ] Brand is substituted without moving controls.
- [ ] 1366×768 and reference aspect ratio keep every primary region visible.

## Notes

- Keep `prd.md` focused on requirements, constraints, and acceptance criteria.
- Lightweight tasks can remain PRD-only.
- For complex tasks, add `design.md` for technical design and `implement.md` for execution planning before `task.py start`.
