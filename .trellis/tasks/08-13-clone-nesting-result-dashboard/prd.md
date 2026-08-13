# 1比1排样画布结果与统计

## Goal

Reproduce the completed/running nesting visualization, candidate list, output chart, and progress/status regions.

## Requirements

- Inherit parent decisions and use images 14–18, 25, 27.
- Render narrow roll layouts at reference default zoom, colored pieces, red material boundary, overlay table, rulers, result thumbnails, and the output pie/statistics panel.
- Maintain separate list and pie utilization values unless their formulas are verified.

## Acceptance Criteria

- [ ] Deterministic scenarios reproduce 80.84%, 82.47%, 80.92%, 75.98%, and 61.60% reference states without false algorithm claims.
- [ ] Image 27 five-pane state shows 1000 pieces, six piece cards, candidate rows, and the complete output-information panel.
- [ ] Running/cancelled/completed command and status states match reference enablement/text.

## Notes

- Keep `prd.md` focused on requirements, constraints, and acceptance criteria.
- Lightweight tasks can remain PRD-only.
- For complex tasks, add `design.md` for technical design and `implement.md` for execution planning before `task.py start`.
