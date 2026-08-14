# 1比1 CAD导入编辑工作台

## Goal

Reproduce the evidenced DXF import and CAD editing states inside the fixed main window.

## Requirements

- Inherit parent decisions and use `research/images-01-09.md` plus images 10–12/21 in `images-10-18.md` and `images-19-27.md`.
- Reuse real M02 DXF inspection/unit confirmation and existing CAD geometry; integrate them into the fixed shell rather than a separate modern page.
- Reproduce right CAD fields/defaults, canvas tool order/tooltips, selection colors, rulers, and file-operation row.

## Acceptance Criteria

- [ ] Reference scenarios 01–05 and 07–12 can be reproduced with the evidenced layout and states.
- [ ] CAD tool opens DXF import; confirmed geometry appears in the central CAD canvas.
- [ ] Unsupported formats/actions are explicit, not fake successes.

## Notes

- Keep `prd.md` focused on requirements, constraints, and acceptance criteria.
- Lightweight tasks can remain PRD-only.
- For complex tasks, add `design.md` for technical design and `implement.md` for execution planning before `task.py start`.
